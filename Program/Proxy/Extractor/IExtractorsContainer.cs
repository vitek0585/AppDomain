using System.Collections.Generic;
using Program.Proxy.Extractor.Extend;

namespace Program.Proxy.Extractor
{
    public interface IExtractorsContainer : IContainerManage
    {
        IEnumerable<IExtractor> GetAllExtractors();
        IEnumerable<IExtractorExtend> GetAllExtractorsExtend();
    }
}