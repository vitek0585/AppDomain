using System.ComponentModel.Composition;

namespace CredoLab.ExtractorContract
{
    [InheritedExport]
    public interface IExtractor
    {
        string Version { get; }
        string Name { get; }
        string Identifier { get; }
        string Description { get; }
        double MinValue { get; }
        double MaxValue { get; }
        ExtractorResult Extract(string data);
        bool IsDiscrete { get; }
    }
}