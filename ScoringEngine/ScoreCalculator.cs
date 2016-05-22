using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CredoLab.ExtractorContract;
using ScoringEngine.Interfaces;
using ScoringEngine.ScoreModel;

namespace ScoringEngine
{
    public class ScoreCalculator : IScoreCalculate
    {
        public CalculateResult Calculate(string data, ScoreCard scoreCard, IEnumerable<IExtractor> extractors)
        {
            try
            {
                int totalScore = 0;
                Parallel.ForEach(scoreCard.Features, () => 0, (featureDto, state, resultScore) =>
                {
                    var extractor = extractors.First(ext => ext.Identifier == featureDto.ExtractorId);
                    var totalPoint = extractor.Extract(data).Result;
                    foreach (var attribute in featureDto.Attributes)
                    {
                        if (attribute.Contains(totalPoint, extractor.IsDiscrete))
                        {
                            resultScore += (int)attribute.Score;
                        }
                    }
                    return resultScore;
                }, (resultScore) =>
                {
                    Interlocked.Add(ref totalScore, resultScore);
                });

                return new CalculateResult()
                {
                    Score = totalScore,
                    ScoreCardId = scoreCard.Id
                };
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}