using System.Collections.Generic;

namespace ScoringEngine.ScoreModel
{
    public class ScoreCardFeature
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ExtractorId { get; set; }

        public int ScoreCardId { get; set; }

        public bool IsDiscrete { get; set; }

        public List<ScoreCardAttribute> Attributes { get; set; }
    }
}
