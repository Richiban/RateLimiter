using System;
using System.Threading;
using System.Threading.Tasks;

namespace Richiban.Regulator
{
    public class ConcurrencyRateLimit : IRateLimit
    {
        private readonly SemaphoreSlim _concurrencySemaphore;

        public ConcurrencyRateLimit(int count)
        {
            _concurrencySemaphore = new SemaphoreSlim(count);
        }

        public Task WaitAsync() => _concurrencySemaphore.WaitAsync();

        public void Done() => _concurrencySemaphore.Release();
        public TimeSpan? TimeToWait => null;
    }
}