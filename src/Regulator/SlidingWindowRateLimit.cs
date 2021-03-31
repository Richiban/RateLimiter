using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Richiban.Regulator
{
    public class SlidingWindowRateLimit : IRateLimit
    {
        private readonly int _count;
        private readonly ConcurrentQueue<DateTimeOffset> _operationRunRecords = new();
        private readonly SemaphoreSlim _sem = new(1);
        private readonly TimeSpan _timePeriod;

        public SlidingWindowRateLimit(int count, TimeSpan timePeriod)
        {
            if (count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (timePeriod == default)
            {
                throw new ArgumentOutOfRangeException(nameof(timePeriod));
            }

            _count = count;
            _timePeriod = timePeriod;
        }

        private bool IsFull => _operationRunRecords.Count >= _count;

        public TimeSpan? TimeToWait
        {
            get
            {
                if (IsFull)
                {
                    var timeToWait = _operationRunRecords.First() - DateTimeOffset.Now;

                    if (timeToWait > TimeSpan.Zero)
                    {
                        return timeToWait;
                    }
                }

                return TimeSpan.Zero;
            }
        }

        public async Task IsReady()
        {
            if (TimeToWait is { } time && time > TimeSpan.Zero)
            {
                await Task.Delay(time);
            }
        }

        public async Task WaitAsync()
        {
            await _sem.WaitAsync();

            if (IsFull)
            {
                _operationRunRecords.TryDequeue(out var nextTimer);

                if (nextTimer >= DateTimeOffset.Now)
                {
                    await Task.Delay(nextTimer - DateTimeOffset.Now);
                }
            }

            _operationRunRecords.Enqueue(DateTimeOffset.Now + _timePeriod);

            _sem.Release();

            if (_operationRunRecords.Count > _count)
            {
                throw new Exception("Overfull");
            }
        }

        public void Done()
        {
        }
    }
}