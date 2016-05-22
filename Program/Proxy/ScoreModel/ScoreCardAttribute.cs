namespace Program.Proxy.ScoreModel
{
    public class ScoreCardAttribute
    {
        public int Id { get; set; }

        public int FeatureId { get; set; }

        public double? StartBound { get; set; }

        public double? EndBound { get; set; }

        public bool IncludeStartBound { get; set; }

        public bool IncludeEndBound { get; set; }

        public double Score { get; set; }

        
    }
}
