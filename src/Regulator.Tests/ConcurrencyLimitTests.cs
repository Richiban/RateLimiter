using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Richiban.Regulator.Tests
{
    public class ConcurrencyLimitTests
    {
        [Test]
        public async Task TestConcurrency()
        {
            var concurrencyLimit = 2;
            var rateLimiter = new RateLimiter(new ConcurrencyRateLimit(concurrencyLimit));

            var runCounter = 0;
            var concurrentRunCounter = 0;

            async Task asyncOperation()
            {
                runCounter++;

                concurrentRunCounter = Math.Max(runCounter, concurrentRunCounter);

                await Task.Delay(100);

                runCounter--;
            }

            var opCount = 3;

            await Enumerable.Range(0, opCount)
                .Select(_ => rateLimiter.WhenReady(asyncOperation))
                .WhenAll();

            Assert.That(concurrentRunCounter, Is.EqualTo(concurrencyLimit));
        }
    }
}