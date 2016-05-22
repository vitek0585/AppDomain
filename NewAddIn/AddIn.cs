using System;
using System.Collections.Generic;
//using System.ComponentModel.Composition;
//using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CommonIterface;

//using CommonIterface;
//using Extractors;

namespace AddIn
{
    public class BasePlugin : MarshalByRefObject, IPlugin
    {
        //[ImportMany(RequiredCreationPolicy = CreationPolicy.NonShared, AllowRecomposition = true)]
        //IEnumerable<IExtractorsProvider> _extractors;

        public void Load()
        {
            //DirectoryCatalog directoryCatalog = new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory);
            //CompositionContainer composition = new CompositionContainer(directoryCatalog);
            //composition.SatisfyImportsOnce(this);
            Console.WriteLine("load");
        }
        public string GetAllPlugin()
        {
            //foreach (var extr in _extractors)
            //{
            //    Console.WriteLine("1.0.0.0");
            //    Console.WriteLine(extr.GetAll());
            //}
            HttpClient client = new HttpClient();
            try
            {
                Console.WriteLine("Send");

                var res = client.GetAsync("http://www.infoworld.com/article/3018997/application-development/working-with-application-domains-in-net.html").Result;
                var t = res.Content.ReadAsStringAsync().Result.Length;
                Console.WriteLine("End");

               // return t.ToString() + " length";
                return "l";
            }
            catch (Exception e)
            {
                throw new Exception("plugin");
            }
            return string.Empty;
        }
    }
}
