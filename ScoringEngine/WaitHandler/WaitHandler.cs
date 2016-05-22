using System.Threading;

namespace ScoringEngine.WaitHandler
{
    public class WaitHandler
    {
        private ManualResetEventSlim _autoResetEvent;
        private int _countThread;
        public WaitHandler()
        {
            _countThread = default(int);
            _autoResetEvent = new ManualResetEventSlim(true);
        }

        public void IncrementCalculate()
        {
            Interlocked.Increment(ref _countThread);
        }

        public void DecrementCalculate()
        {
            Interlocked.Decrement(ref _countThread);
            if (Interlocked.Exchange(ref _countThread, default(int)) == 0)
            {
                _autoResetEvent.Set();
            }
        }

        public void WaitOne()
        {
            _autoResetEvent.Wait();
        }

        public void Reset()
        {
            _autoResetEvent.Reset();
        }
    }
}