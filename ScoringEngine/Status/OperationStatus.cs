namespace ScoringEngine.Status
{
    public enum OperationStatus : ushort
    {
        NotInitializeScoreCard,
        FailedInitializeScoreCard,
        SuccessInitializedScoreCard,
        CompleteCalculate,
        ScoreCardAlreadyInitialized
    }
}