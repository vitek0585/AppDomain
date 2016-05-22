using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonIterface;
using Extractors;

namespace AddIn
{
    public class BasePlugin : MarshalByRefObject, IPlugin
    {
        [ImportMany(RequiredCreationPolicy = CreationPolicy.NonShared,AllowRecomposition = true)]
        IEnumerable<IExtractorsProvider> _extractors;

        public void Load()
        {
            DirectoryCatalog directoryCatalog = new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory);
            CompositionContainer composition = new CompositionContainer(directoryCatalog);
            composition.SatisfyImportsOnce(this);
        }
        public string GetAllPlugin()
        {
            foreach (var extr in _extractors)
            {
                Console.WriteLine("sdfsfsdfsdfsdf");
                Console.WriteLine(extr.GetAll());
            }
            return string.Empty;
        }
    }
}
