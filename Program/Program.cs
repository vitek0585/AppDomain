using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using CommonIterface;

namespace Program
{
    class Program
    {
        static string pathToPlugins = Path.Combine(Environment.CurrentDirectory, "plugins");
        static string DomainName = "MyDomain";
        static void Main(string[] args)
        {
            AppDomainManager manager = new AppDomainManager();
            manager.CreateNonTrustDomain(DomainName, pathToPlugins);
            var typeAddin = manager.FindType(pathToPlugins, typeof(IPlugin));
            IPlugin instance;
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                Task.Run(() =>
                {
                    instance = manager.CreateInstanceInsideDomain<IPlugin>(DomainName, typeAddin);
                    instance.Load();
                    Console.WriteLine(instance.GetAllPlugin());
                });
            }
            for (int i = 0; i < 10; i++)
            {
                var t = Task.Run(() =>
                {
                    manager.UnloadDomain(DomainName);
                });
                tasks.Add(t);
            }
            Task.WhenAll(tasks).ContinueWith((t, o) =>
            {
                Console.WriteLine(manager.IsExistDomain(DomainName));
            }, null);
            PrintAllAssembly(AppDomain.CurrentDomain);
            PrintAllAssembly(manager.GetAllAssembliesDomain(DomainName));
            Console.ReadKey();
        }
        //static void Main(string[] args)
        //{
        //    AppDomainSetup domainSetup = new AppDomainSetup
        //    {
        //        ShadowCopyFiles = "true",
        //        ShadowCopyDirectories = "true",//AppDomain.CurrentDomain.BaseDirectory,
        //        ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
        //        //PrivateBinPath = pathToPlugins,
        //        //PrivateBinPathProbe = AppDomain.CurrentDomain.BaseDirectory

        //        //                ApplicationName = "vit",
        //        //                DynamicBase = pathToPlugins
        //    };
        //    PermissionSet permissionSet = new PermissionSet(PermissionState.Unrestricted);
        //    permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
        //    permissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read, pathToPlugins));
        //    permissionSet.AddPermission(new ReflectionPermission(PermissionState.Unrestricted));

        //    var newDomain = AppDomain.CreateDomain("Plugin my", null, domainSetup, permissionSet);
        //    var oldDomain = AppDomain.CreateDomain("Plugin my", null, domainSetup, permissionSet);

        //    //newDomain.DoCallBack(() => new AppDomainManager().Initialize(pathToPlugins,typeof(IPlugin)));

        //    //newDomain.DoCallBack(PluginInvoker.InvokePlugin);

        //    IPlugin basePlug = newDomain.CreateInstanceAndUnwrap("AddIn", "AddIn.BasePlugin") as IPlugin;
        //    if (basePlug != null)
        //    {
        //        basePlug.Load();
        //        basePlug.GetAllPlugin();
        //        Console.WriteLine(basePlug.GetHashCode());
        //    }
        //    try
        //    {
        //        AppDomain.Unload(newDomain);
        //        AppDomain.Unload(newDomain);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //    }
        //    basePlug = oldDomain.CreateInstanceAndUnwrap("AddIn", "AddIn.BasePlugin") as IPlugin;
        //    if (basePlug != null)
        //    {
        //        basePlug.Load();
        //        basePlug.GetAllPlugin();
        //        Console.WriteLine(basePlug.GetHashCode());

        //    }
        //    PrintAllAssembly(AppDomain.CurrentDomain);
        //    //PrintAllAssembly(newDomain);
        //    PrintAllAssembly(oldDomain);
        //    //            AppDomain.Unload(newDomain);


        //    Console.ReadKey();
        //}

        private static void PrintAllAssembly(AppDomain domain)
        {
            Console.WriteLine(new string('-', 40));
            Console.WriteLine(domain.FriendlyName + " - " + domain.Id);
            foreach (var assembly in domain.GetAssemblies())
            {
                Console.WriteLine(assembly.FullName);
            }
        }
        private static void PrintAllAssembly(Assembly[] assemblies)
        {
            Console.WriteLine(new string('-', 40));

            foreach (var assembly in assemblies)
            {
                Console.WriteLine(assembly.FullName);
            }
        }
        [Serializable]
        public static class PluginInvoker
        {
            public static void InvokePlugin()
            {
                var plugins = Directory.GetFiles(pathToPlugins, "*.dll");
                var discovery = new Discover();
                Assembly ass = null;
                foreach (var plugin in plugins)
                {
                    var plName = discovery.GetPluginsByTypeName(plugin, typeof(IPlugin));
                    foreach (var name in plName)
                    {
                        Console.WriteLine(name);
                        var nameAss = AssemblyName.GetAssemblyName(plugin);
                        nameAss = AssemblyName.GetAssemblyName(plugin);
                        Assembly.Load(nameAss);
                    }
                }
            }
        }

    }
}
