using CredoLab.ExtractorContract;
using CredoLab.ExtractorContract.ExtractorInfo;

namespace ScoringEngine.Extractor.Common
{
    public interface IExtractorExtend : IExtractor, IExtractorInfo
    {
        string FullPath { get; }
    }
}