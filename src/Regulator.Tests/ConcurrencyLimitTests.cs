using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Richiban.Regulator.Tests
{
    public class ConcurrencyLimitTests
    {
        private static readonly Random Random = new();

        [Test]
        public async Task TestConcurrency()
        {
            var concurrencyLimit = Random.Next(2, 10);
            var rateLimit = new ConcurrencyRateLimit(concurrencyLimit);
            var iterations = concurrencyLimit + Random.Next(5, 10);

            var runCounter = 0;
            var concurrentRunCounter = 0;

            async Task asyncOperation()
            {
                await rateLimit.WaitAsync();
                runCounter++;

                concurrentRunCounter = Math.Max(runCounter, concurrentRunCounter);

                await Task.Delay(100);

                runCounter--;
                rateLimit.Done();
            }

            await Enumerable.Range(0, iterations).Select(_ => asyncOperation()).WhenAll();

            Assert.That(concurrentRunCounter, Is.EqualTo(concurrencyLimit));
        }
    }
}