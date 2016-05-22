using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extractors
{
    [InheritedExport]
    public interface IExtractorsProvider
    {
        string GetAll();
    }
    public class ExtractorsProvider: IExtractorsProvider
    {
        public string GetAll()
        {
            Console.WriteLine("New Extractor");
            return "All";
        }
    }

    //public class ExtractorsProviderTwo : IExtractorsProvider
    //{
    //    public string GetAll()
    //    {
    //        Console.WriteLine("Old Extractor");
    //        return "All";
    //    }
    //}
}
