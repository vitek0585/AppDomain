using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace Program
{
    public class DomainManager
    {
        private static readonly ConcurrentDictionary<string, AppDomain> CreatedDomains;

        static DomainManager()
        {
            CreatedDomains = new ConcurrentDictionary<string, AppDomain>();
        }
        public IEnumerable<Type> GetTypes(string aseamblyPath, Type interfaceType)
        {
            var findedTypes = new List<Type>();
            var allDll = Directory.GetFiles(aseamblyPath, "*.dll");
            var reflectedAssembly = allDll.Select(Assembly.ReflectionOnlyLoadFrom).ToArray();
            foreach (var assembly in reflectedAssembly)
            {
                Type[] types = null;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types;
                }
                foreach (var type in types.Where(t => t != null))
                {
                    if (type.IsClass && !type.IsAbstract
                        && type.GetInterfaces().Any(@interface => @interface.GUID == interfaceType.GUID)
                        && typeof(MarshalByRefObject).IsAssignableFrom(type))
                    {
                        findedTypes.Add(type);
                    }
                }
            }
            return findedTypes;
        }
        public Type GetType(string assemblyPath, Type interfaceType)
        {
            var findedTypes = GetTypes(assemblyPath, interfaceType).ToList();
            if (findedTypes.Any())
            {
                Type actualType = findedTypes.First();
                foreach (var type in findedTypes)
                {
                    if (actualType.Assembly.GetName().Version
                        .CompareTo(type.Assembly.GetName().Version) < 0)
                    {
                        actualType = type;
                    }
                }
                return actualType;
            }
            return null;
        }
        public void CreateNonTrustedDomain(string domainName, string pathToAssembly, string applicationBasePath = null, string applicationName = null)
        {
            var permissionSet = new PermissionSet(PermissionState.None);
            permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            permissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read, pathToAssembly));
            permissionSet.AddPermission(new ReflectionPermission(PermissionState.Unrestricted));

            var domainSetup = new AppDomainSetup
            {
                ShadowCopyFiles = "true",
                ShadowCopyDirectories = AppDomain.CurrentDomain.BaseDirectory,
                ApplicationBase = applicationBasePath ?? AppDomain.CurrentDomain.BaseDirectory,
                ApplicationName = applicationName ?? string.Empty,
                PrivateBinPath = pathToAssembly
            };
            CreateDomain(domainName, pathToAssembly, domainSetup, permissionSet);
        }

        public bool IsDomainExist(string domainName)
        {
            lock (CreatedDomains)
            {
                return CreatedDomains.ContainsKey(domainName);
            }
        }

        public void CreateDomain(string domainName, string pathToAssembly, AppDomainSetup appDomainSetup, PermissionSet permissionSet)
        {
            lock (CreatedDomains)
            {
                if (CreatedDomains.ContainsKey(domainName))
                {
                    throw new ArgumentException("Domain name already exists inside current process");
                }
                if (string.IsNullOrWhiteSpace(domainName))
                {
                    throw new ArgumentException("Domain name can not be null");
                }
                var createdDomain = AppDomain.CreateDomain(domainName, null, appDomainSetup, permissionSet);
                createdDomain.DomainUnload += DomainUnload;
                CreatedDomains.TryAdd(domainName, createdDomain);
            }
        }

        private static void DomainUnload(object sender, EventArgs e)
        {
            if (sender is AppDomain)
            {
                var unloadedDomain = sender as AppDomain;
                var deletedDomain = CreatedDomains.FirstOrDefault(domain => domain.Value.Id == unloadedDomain.Id);
                if (deletedDomain.Value != null)
                {
                    AppDomain removedDomain;
                    CreatedDomains.TryRemove(unloadedDomain.FriendlyName, out removedDomain);
                }
            }
        }

        public TInstance CreateInstanceInsideDomain<TInstance>(string domainName, Type typeOfInstance, params object[] ctorParameters) where TInstance : class
        {
            AppDomain domain;
            CreatedDomains.TryGetValue(domainName, out domain);
            return domain?.CreateInstanceAndUnwrap(typeOfInstance.Assembly.GetName().Name, typeOfInstance.FullName, true, BindingFlags.Default, null, ctorParameters, CultureInfo.InvariantCulture, null) as TInstance;
        }

        public static void UnloadDomain(string domainName)
        {
            lock (CreatedDomains)
            {
                AppDomain removedDomain;
                CreatedDomains.TryGetValue(domainName, out removedDomain);
                if (removedDomain != null)
                {
                    var nameRemovedDomain = removedDomain.FriendlyName;
                    AppDomain.Unload(removedDomain);
                    CreatedDomains.TryRemove(nameRemovedDomain, out removedDomain);
                }
            }
        }

    }
}
