using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;

namespace Loader {

	/// <summary>
	/// When hosted in a separate AppDomain, provides a mechanism for loading 
	/// plugin assemblies and instantiating objects within them.
	/// </summary>
	[SecurityPermission(SecurityAction.Demand, Infrastructure = true)]
	internal sealed class PluginLoader : MarshalByRefObject, IDisposable {

		private Sponsor<TextWriter> mLog;

		/// <summary>
		/// Gets or sets the directory containing the assemblies.
		/// </summary>
		private string PluginDir { get; set; }
		/// <summary>
		/// Gets or sets the collection of assemblies that have been loaded.
		/// </summary>
		private List<Assembly> Assemblies { get; set; }
		/// <summary>
		/// Gets or sets the collection of constructors for various interface types.
		/// </summary>
		private Dictionary<Type, LinkedList<ConstructorInfo>> ConstructorCache { get; set; }
		/// <summary>
		/// Gets or sets the TextWriter to use for logging.
		/// </summary>
		public TextWriter Log {
			get {
				return (mLog != null) ? mLog.Instance : null;
			}
			set {
				mLog = (value != null) ? new Sponsor<TextWriter>(value) : null;
			}
		}

		/// <summary>
		/// Initialises a new instance of the PluginLoader class.
		/// </summary>
		public PluginLoader() {
			Log = TextWriter.Null;
			ConstructorCache = new Dictionary<Type, LinkedList<ConstructorInfo>>();
			Assemblies = new List<Assembly>();
		}

		/// <summary>
		/// Finalizer.
		/// </summary>
		~PluginLoader() {
			Dispose(false);
		}

		/// <summary>
		/// Loads plugin assemblies into the application domain and populates the collection of plugins.
		/// </summary>
		/// <param name="pluginDir"></param>
		/// <param name="disabledPlugins"></param>
		public void Init(string pluginDir) {
			Uninit();

			PluginDir = pluginDir;

			foreach (string dllFile in Directory.GetFiles(PluginDir, "*.dll")) {
				try {
					Assembly asm = Assembly.LoadFile(dllFile);
					Log.WriteLine("Loaded assembly {0}.", asm.GetName().Name);

					// TODO: restrict assemblies loaded based on digital signature, 
					// implementing a required interface, DRM, etc

					Assemblies.Add(asm);
				}
				catch (ReflectionTypeLoadException rex) {
					Log.WriteLine("Plugin {0} failed to load.", Path.GetFileName(dllFile));
					foreach (Exception ex in rex.LoaderExceptions) {
						Log.WriteLine("\t{0}: {1}", ex.GetType().Name, ex.Message);
					}
				}
				catch (BadImageFormatException) {
					// ignore, this simply means the DLL was not a .NET assembly
					Log.WriteLine("Plugin {0} is not a valid assembly.", Path.GetFileName(dllFile));
				}
				catch (Exception ex) {
					Log.WriteLine("Plugin {0} failed to load.", Path.GetFileName(dllFile));
					Log.WriteLine("\t{0}: {1}", ex.GetType().Name, ex.Message);
				}
			}
		}

		/// <summary>
		/// Clears all plugin assemblies and type info.
		/// </summary>
		public void Uninit() {
			Assemblies.Clear();
			ConstructorCache.Clear();
		}

		/// <summary>
		/// Returns a sequence of instances of types that implement a 
		/// particular interface. Any instances that are MarshalByRefObject 
		/// must be sponsored to prevent disconnection.
		/// </summary>
		/// <typeparam name="TInterface"></typeparam>
		/// <returns></returns>
		public IEnumerable<TInterface> GetImplementations<TInterface>() {
			LinkedList<TInterface> instances = new LinkedList<TInterface>();

			foreach (ConstructorInfo constructor in GetConstructors<TInterface>()) {
				instances.AddLast(CreateInstance<TInterface>(constructor));
			}

			return instances;
		}

		/// <summary>
		/// Returns the name of the assembly that owns the specified instance 
		/// of a particular interface. (If you try to obtain the assembly using 
		/// Object.GetType(), you will get MarshalByRefObject.)
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public AssemblyName GetOwningAssembly(object instance) {
			Type type = instance.GetType();
			return type.Assembly.GetName();
		}

		/// <summary>
		/// Returns the name of the type of the specified instance of a 
		/// particular interface. (If you try to obtain the type using 
		/// Object.GetType(), you will get MarshalByRefObject.)
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public string GetTypeName(object instance) {
			Type type = instance.GetType();
			return type.FullName;
		}

		/// <summary>
		/// Returns the first implementation of a particular interface type. 
		/// Default implementations are not favoured.
		/// </summary>
		/// <typeparam name="TInterface"></typeparam>
		/// <returns></returns>
		public TInterface GetImplementation<TInterface>() {
			return GetImplementations<TInterface>().FirstOrDefault();
		}

		/// <summary>
		/// Returns the constructors for implementations of a particular interface 
		/// type. Constructor info is cached after the initial crawl.
		/// </summary>
		/// <typeparam name="TInterface"></typeparam>
		/// <returns></returns>
		private IEnumerable<ConstructorInfo> GetConstructors<TInterface>() {
			if (ConstructorCache.ContainsKey(typeof(TInterface))) {
				return ConstructorCache[typeof(TInterface)];
			}
			else {
				LinkedList<ConstructorInfo> constructors = new LinkedList<ConstructorInfo>();

				foreach (Assembly asm in Assemblies) {
					foreach (Type type in asm.GetTypes()) {
						if (type.IsClass && !type.IsAbstract) {
							if (type.GetInterfaces().Contains(typeof(TInterface))) {
								ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
								constructors.AddLast(constructor);
							}
						}
					}
				}

				ConstructorCache[typeof(TInterface)] = constructors;
				return constructors;
			}
		}

		/// <summary>
		/// Returns instances of all implementations of a particular interface 
		/// type in the specified assembly.
		/// </summary>
		/// <typeparam name="TInterface"></typeparam>
		/// <param name="assembly"></param>
		/// <returns></returns>
		private IEnumerable<TInterface> GetImplementations<TInterface>(Assembly assembly) {
			List<TInterface> instances = new List<TInterface>();

			foreach (Type type in assembly.GetTypes()) {
				if (type.IsClass && !type.IsAbstract) {
					if (type.GetInterfaces().Contains(typeof(TInterface))) {
						TInterface instance = default(TInterface);
						ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
						instance = CreateInstance<TInterface>(constructor);
						if (instance != null) instances.Add(instance);
					}
				}
			}

			return instances;
		}

		/// <summary>
		/// Invokes the specified constructor to create an instance of an 
		/// interface type.
		/// </summary>
		/// <typeparam name="TInterface"></typeparam>
		/// <param name="constructor"></param>
		/// <returns></returns>
		private TInterface CreateInstance<TInterface>(ConstructorInfo constructor) {
			TInterface instance = default(TInterface);

			try {
				instance = (TInterface)constructor.Invoke(null);
			}
			catch (Exception ex) {
				Log.WriteLine(
					"Unable to instantiate type {0} in plugin {1}",
					constructor.ReflectedType.FullName,
					Path.GetFileName(constructor.ReflectedType.Assembly.Location)
				);
				Log.WriteLine("\t{0}: {1}", ex.GetType().Name, ex.Message);
			}

			return instance;
		}

		/// <summary>
		/// Gets the first implementation of a particular interface type in 
		/// the specified assembly. Default implementations are not favoured.
		/// </summary>
		/// <typeparam name="TInterface"></typeparam>
		/// <param name="assembly"></param>
		/// <returns></returns>
		private TInterface GetImplementation<TInterface>(Assembly assembly) {
			return GetImplementations<TInterface>(assembly).FirstOrDefault();
		}

		#region IDisposable Members

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			if (disposing) {
				Uninit();
				if (mLog != null) mLog.Dispose();
			}
		}

		#endregion
	}
}