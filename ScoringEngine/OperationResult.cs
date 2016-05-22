using ScoringEngine.Status;

namespace ScoringEngine
{
    public class OperationResult
    {
        public int Score { get; set; }
        public OperationStatus OperationStatus { get; }
        public string Message { get; }

        public OperationResult(OperationStatus operationStatus, string message = null, int score = default(int))
        {
            Score = score;
            OperationStatus = operationStatus;
            Message = message;
        }
    }
}