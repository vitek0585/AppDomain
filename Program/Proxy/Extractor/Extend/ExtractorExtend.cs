namespace Program.Proxy.Extractor.Extend
{
    public class ExtractorExtend : IExtractorExtend
    {
        private IExtractor _extractor;
        private IExtractorInfo _info;
        public ExtractorExtend(IExtractor extractor, IExtractorInfo info)
        {
            _info = info;
            _extractor = extractor;
        }
        public ExtractorExtend(IExtractor extractor, string dllPath)
        {
            FullPath = dllPath;
            _extractor = extractor;
        }
        public string Version => _extractor.Version;

        public string Name => _extractor.Name;
        public string Identifier => _extractor.Identifier;
        public string Description => _extractor.Description;
        public double MinValue => _extractor.MinValue;
        public double MaxValue => _extractor.MaxValue;

        public string FullPath { get; }

        public ExtractorResult Extract(string data)
        {
            return _extractor.Extract(data);
        }

        public bool IsDiscrete => _extractor.IsDiscrete;
    }

    public interface IExtractorInfo
    {
    }
}