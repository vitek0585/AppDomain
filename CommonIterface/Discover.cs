using System;
using System.Collections.Generic;
using System.Reflection;

namespace CommonIterface
{
    public class Discover
    {
        public IEnumerable<string> GetPluginsByTypeName(string aseamblyPath, Type pluginType)
        {
            var typeNames = new List<string>();
            Assembly a = AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(aseamblyPath));
            foreach (var type in a.GetTypes())
            {
                if (pluginType.IsAssignableFrom(type))
                    typeNames.Add(type.FullName);
            }
            return typeNames;
        }
        
    }
}