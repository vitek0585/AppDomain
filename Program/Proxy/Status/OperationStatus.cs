namespace Program.Proxy.Status
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