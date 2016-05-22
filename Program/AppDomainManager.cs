using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace Program
{
    public class AppDomainManager
    {
        private static ConcurrentDictionary<string, AppDomain> _createdDomains;

        static AppDomainManager()
        {
            _createdDomains = new ConcurrentDictionary<string, AppDomain>();
        }
        public IEnumerable<Type> FindTypes(string aseamblyPath, Type interfaceType)
        {
            var currentTypes = new List<Type>();
            var allDll = Directory.GetFiles(aseamblyPath, "*.dll");
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ad2_ReflectionOnlyAssemblyResolve;
            var reflectedAssembly = allDll.Select(Assembly.ReflectionOnlyLoadFrom).ToArray();
            foreach (var assembly in reflectedAssembly)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass && type.IsAbstract == false
                     && type.GetInterfaces().Any(@interface => @interface.GUID == interfaceType.GUID)
                        && typeof(MarshalByRefObject).IsAssignableFrom(type))
                    {
                        currentTypes.Add(type);
                    }
                }
            }
            return currentTypes;
        }
        public Type FindType(string aseamblyPath, Type interfaceType)
        {
            var types = FindTypes(aseamblyPath, interfaceType);
            if (types.Any())
            {
                Type maxVersionType = types.First();
                foreach (var type in types)
                {
                    if (maxVersionType.Assembly.GetName().Version
                        .CompareTo(type.Assembly.GetName().Version) < 0)
                    {
                        maxVersionType = type;
                    }
                }
                return maxVersionType;
            }
            return null;
        }
        public void CreateNonTrustDomain(string domainName, string pathToAssembly, string applicationName = null)
        {
            var permissionSet = new PermissionSet(PermissionState.None);
            permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            permissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read, pathToAssembly));
            permissionSet.AddPermission(new ReflectionPermission(PermissionState.Unrestricted));

            var domainSetup = new AppDomainSetup
            {
                ShadowCopyFiles = "true",
                ShadowCopyDirectories = "true",
                ApplicationBase = pathToAssembly,
                ApplicationName = applicationName ?? string.Empty
            };
            CreateDomain(domainName, pathToAssembly, domainSetup, permissionSet);
        }

        public bool IsExistDomain(string domainName)
        {
            return _createdDomains.ContainsKey(domainName);
        }
        public void CreateDomain(string domainName, string pathToAssembly, AppDomainSetup appDomainSetup, PermissionSet permissionSet)
        {
            if (_createdDomains.ContainsKey(domainName))
            {
                throw new ArgumentException("Domain name already exists inside current process");
            }
            if (string.IsNullOrWhiteSpace(domainName))
            {
                throw new ArgumentException("Domain name can not be null");
            }
            var createdDomain = AppDomain.CreateDomain(domainName, null, appDomainSetup, permissionSet);
            _createdDomains.TryAdd(domainName, createdDomain);
        }

        public TInstance CreateInstanceInsideDomain<TInstance>(string domainName, Type typeOfInstance) where TInstance : class
        {
            AppDomain domain;
            _createdDomains.TryGetValue(domainName, out domain);
            return domain?.CreateInstanceAndUnwrap(typeOfInstance.Assembly.GetName().Name, typeOfInstance.FullName) as TInstance;
        }

        public void UnloadDomain(string domainName)
        {
            AppDomain removedDomain;
            _createdDomains.TryGetValue(domainName, out removedDomain);
            if (removedDomain != null)
            {
                AppDomain.Unload(removedDomain);
                _createdDomains.TryRemove(domainName, out removedDomain);
            }
        }

        public Assembly[] GetAllAssembliesDomain(string domainName)
        {
            AppDomain domain;
            _createdDomains.TryGetValue(domainName, out domain);
            return domain.GetAssemblies();
        }
        private static Assembly ad2_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return null;
        }

    }
}