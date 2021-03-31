using System;
using System.Threading;
using System.Threading.Tasks;

namespace Richiban.Regulator
{
    public class ConcurrencyRateLimit : IRateLimit
    {
        private readonly int _maxConcurrentRequests;
        private readonly SemaphoreSlim _concurrencySemaphore;

        public ConcurrencyRateLimit(int maxConcurrentRequests)
        {
            _maxConcurrentRequests = maxConcurrentRequests;
            _concurrencySemaphore = new SemaphoreSlim(maxConcurrentRequests);
        }

        public async Task IsReady()
        {
            while (_concurrencySemaphore.CurrentCount < 1)
            {
                await Task.Delay(25);
            }
        }

        public Task WaitAsync() => _concurrencySemaphore.WaitAsync();
        public void Done() => _concurrencySemaphore.Release();
        public TimeSpan? TimeToWait => null;
    }
}