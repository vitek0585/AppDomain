﻿using System.Collections.Generic;
using CredoLab.ExtractorContract;
using ScoringEngine.ScoreModel;

namespace ScoringEngine.Interfaces
{
    public interface IScoreCalculate
    {
        CalculateResult Calculate(string data, ScoreCard scoreCard, IEnumerable<IExtractor> extractors);
    }
}