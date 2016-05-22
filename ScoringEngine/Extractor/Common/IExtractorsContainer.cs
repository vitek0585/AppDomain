using System.Collections.Generic;
using CredoLab.ExtractorContract;

namespace ScoringEngine.Extractor.Common
{
    public interface IExtractorsContainer : IContainerManage
    {
        IEnumerable<IExtractor> GetAllExtractors();
        IEnumerable<IExtractorExtend> GetAllExtractorsExtend();
    }
}