using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Richiban.Regulator
{
    public class RateLimiterPool<T>
    {
        private readonly List<(T data, RateLimiter rateLimiter)> _rateLimiters;

        public RateLimiterPool(
            IEnumerable<(T data, RateLimiter rateLimiter)> dataRateLimiterPairs)
        {
            _rateLimiters = dataRateLimiterPairs.ToList();

            if (_rateLimiters.Count < 1)
            {
                throw new ArgumentException($"{nameof(dataRateLimiterPairs)} was empty");
            }
        }

        public RateLimiterPool(
            IEnumerable<T> data,
            IEnumerable<IRateLimit> rateLimits,
            TimeSpan? waitTimeout = null)
        {
            _rateLimiters = data.Select(
                    t => (t,
                        new RateLimiter(rateLimits)
                        {
                            WaitTimeout = waitTimeout ?? TimeSpan.Zero
                        }))
                .ToList();

            if (_rateLimiters.Count < 1)
            {
                throw new ArgumentException($"{nameof(data)} was empty");
            }
        }

        public async Task<R> WithNextAvailableDataAsync<R>(
            Func<T, Task<R>> continueWithData)
        {
            var (data, rateLimiter) = await _rateLimiters.Select(
                    async rl =>
                    {
                        await rl.rateLimiter.IsReady();

                        return rl;
                    })
                .WhenAny();

            return await rateLimiter.WhenReady(() => continueWithData(data));
        }

        public async Task WithNextAvailableDataAsync(Func<T, Task> continueWithData)
        {
            var (data, rateLimiter) = await _rateLimiters.Select(
                    async rl =>
                    {
                        await rl.rateLimiter.IsReady();

                        return rl;
                    })
                .WhenAny();

            await rateLimiter.WhenReady(() => continueWithData(data));
        }
    }
}