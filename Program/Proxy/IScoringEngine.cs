namespace Program.Proxy
{
    public interface IScoringEngine
    {
        OperationResult TryCalculate(string data);
        OperationResult InitializeScoreCard(string scoreCard);
        OperationResult UpdateScoreCard(string scoreCard);
        string GetKeyForUpdate();
    }
}