using System;
using System.Collections.Generic;

namespace ScoringEngine.ScoreModel
{
    public class ScoreCard
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public double Importance { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public List<ScoreCardFeature> Features { get; set; }
    }
}