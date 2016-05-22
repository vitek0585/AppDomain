using System;
namespace ScoringEngine.ScoreModel
{
    public static class ScoreCardExtentions
    {
        public static bool Contains(this ScoreCardAttribute scoreCardAttributeDto, double point, bool isDiscrete = false)
        {
            return scoreCardAttributeDto.StartBound == null && scoreCardAttributeDto.EndBound > point
                   || scoreCardAttributeDto.EndBound == null && scoreCardAttributeDto.StartBound < point
                   || scoreCardAttributeDto.StartBound < point && point < scoreCardAttributeDto.EndBound
                   ||
                   (scoreCardAttributeDto.IncludeStartBound || isDiscrete) && scoreCardAttributeDto.StartBound != null &&
                   Math.Abs(scoreCardAttributeDto.StartBound.Value - point) < 0.0001
                   ||
                   (scoreCardAttributeDto.IncludeEndBound || isDiscrete) && scoreCardAttributeDto.EndBound != null &&
                   Math.Abs(point - scoreCardAttributeDto.EndBound.Value) < 0.0001;
        }
    }
}