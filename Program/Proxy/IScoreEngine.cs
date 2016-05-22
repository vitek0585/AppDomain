namespace Program.Proxy
{
    public interface IScoreEngine
    {
        OperationResult TryCalculate(string data);
        OperationResult InitializeScoreCard(string scoreCard);
        OperationResult UpdateScoreCard(string scoreCard);
        string GetKeyForUpdate();
    }
}