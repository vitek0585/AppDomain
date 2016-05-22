using System;
using System.IO;
using ExampleCommon;
using Loader;

namespace ExampleHost {

	/// <summary>
	/// An example program to demonstrate PluginHost.
	/// </summary>
	public class Program {

		static void Main(string[] args) {
			// subdirectory where ExamplePlugin.dll resides
			string path = Path.Combine(Environment.CurrentDirectory, "Plugins");

			using (PluginHost host = new PluginHost(path)) {
				// events which allow actions to be performed after plug-ins are loaded
				host.PluginsLoaded += new EventHandler(host_PluginsLoaded);
				host.PluginsUnloaded += new EventHandler(host_PluginsUnloaded);

				// load the example plug-in into a separate AppDomain
				if (host.LoadPlugins()) {
					// load the first available implementation of our example interface
					Console.WriteLine("Loading first available implementation of {0}...", typeof(IExampleType).Name);
					Sponsor<IExampleType> objectFromPlugin = host.GetImplementation<IExampleType>();

					if (objectFromPlugin != null) {
						// get instance from remoting sponsor
						IExampleType exampleType = objectFromPlugin.Instance;

						// show metadata
						Console.WriteLine(
							"Implementation {0} found in assembly {1}",
							host.GetTypeName(exampleType),
							Path.GetFileName(host.GetOwningAssembly(exampleType).CodeBase)
						);

						// call interface method
						Console.WriteLine("Calling DisplayMessage() method...");
						Console.ForegroundColor = ConsoleColor.Cyan;
						exampleType.DisplayMessage();
						Console.ResetColor();
                    }
                    else {
						Console.WriteLine("No implementations of {0} were found.", typeof(IExampleType).Name);
					}
				}
				else {
					Console.WriteLine("Plug-ins were not loaded.");
				}
			}
			// wait for user input
			Console.WriteLine("Press any key to continue...");
			Console.ReadLine();
		}

		static void host_PluginsUnloaded(object sender, EventArgs e) {
			Console.WriteLine("Plug-ins unloaded.");
		}

		static void host_PluginsLoaded(object sender, EventArgs e) {
			Console.WriteLine("Plug-ins loaded.");
		}
	}
}
