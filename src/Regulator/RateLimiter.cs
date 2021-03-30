using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Richiban.Regulator
{
    public class RateLimiter
    {
        private readonly IReadOnlyCollection<IRateLimit> _rateLimits;

        public RateLimiter(params IRateLimit[] rateLimits) : this(
            (IEnumerable<IRateLimit>) rateLimits)
        {
        }

        public RateLimiter(IEnumerable<IRateLimit> rateLimits)
        {
            _rateLimits = rateLimits?.ToList()
                          ?? throw new ArgumentNullException(nameof(rateLimits));

            if (_rateLimits is not { Count: > 0 })
            {
                _rateLimits = new[] { new NullRateLimiter() };
            }
        }

        public TimeSpan WaitTimeout { get; init; }

        public TimeSpan TimeToWait =>
            _rateLimits.Select(rl => rl.TimeToWait).Aggregate((x, y) => x > y ? x : y)
            ?? TimeSpan.Zero;

        public async Task<T> WhenReady<T>(Func<Task<T>> f)
        {
            try
            {
                await Task.WhenAll(_rateLimits.Select(rl => rl.WaitAsync()))
                    .WithTimeout(WaitTimeout);

                return await f();
            }
            finally
            {
                foreach (var rl in _rateLimits)
                {
                    rl.Done();
                }
            }
        }

        public async Task WhenReady(Func<Task> f)
        {
            try
            {
                await Task.WhenAll(_rateLimits.Select(rl => rl.WaitAsync()))
                    .WithTimeout(WaitTimeout);

                await f();
            }
            finally
            {
                foreach (var rl in _rateLimits)
                {
                    rl.Done();
                }
            }
        }
    }
}