using System.Collections.Generic;
using Program.Proxy.Extractor;
using Program.Proxy.ScoreModel;


namespace Program.Proxy
{
    public interface IScoreCalculate
    {
        CalculateResult Calculate(string data, ScoreCard scoreCard, IEnumerable<IExtractor> extractors);
    }
}