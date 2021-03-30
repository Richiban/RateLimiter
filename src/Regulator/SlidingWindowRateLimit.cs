using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Richiban.Regulator
{
    public class SlidingWindowRateLimit : IRateLimit
    {
        private readonly int _count;
        private readonly TimeSpan _timePeriod;

        private readonly ConcurrentQueue<DateTimeOffset> _timers = new();

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

        private bool AllTimersInUse => _timers.Count >= _count;

        public TimeSpan? TimeToWait
        {
            get
            {
                if (AllTimersInUse)
                {
                    var timeToWait = _timers.First() - DateTimeOffset.Now;

                    if (timeToWait > TimeSpan.Zero)
                    {
                        return timeToWait;
                    }
                }

                return TimeSpan.Zero;
            }
        }

        public async Task WaitAsync()
        {
            if (AllTimersInUse && _timers.TryPeek(out var nextTimer))
            {
                if (nextTimer >= DateTimeOffset.Now)
                {
                    await Task.Delay(nextTimer - DateTimeOffset.Now);
                }

                _timers.TryDequeue(out _);
            }
        }

        public void Done() => _timers.Enqueue(DateTimeOffset.Now + _timePeriod);
    }
}