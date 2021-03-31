using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Richiban.Regulator.Tests
{
    public class SlidingWindowTests
    {
        private static Random Random = new();

        [Test]
        public async Task TestSlidingWindow()
        {
            var countLimit = Random.Next(2, 5);
            var timeLimit = TimeSpan.FromMilliseconds(100);
            var iterations = Random.Next(5, 15);

            var rateLimit = new SlidingWindowRateLimit(countLimit, timeLimit);

            var runTimes = new ConcurrentBag<DateTimeOffset>();

            async Task asyncOperation()
            {
                await rateLimit.WaitAsync();
                runTimes.Add(DateTimeOffset.Now);

                rateLimit.Done();
            }

            await Enumerable.Range(0, iterations).Select(_ => asyncOperation()).WhenAll();

            var windows = SplitIntoWindows(runTimes, timeLimit).ToList();

            Assert.That(windows, Has.Count.GreaterThanOrEqualTo(1));

            foreach (var window in windows)
            {
                Assert.That(
                    window.Count,
                    Is.LessThanOrEqualTo(countLimit),
                    string.Join(", ", window.Select(d => d - window.First())));
            }
        }

        IEnumerable<TimeSpan[]> SplitIntoWindows(
            ConcurrentBag<DateTimeOffset> runTimes,
            TimeSpan windowLength)
        {
            var results = new List<DateTimeOffset>();

            foreach (var dt in runTimes.OrderBy(self => self))
            {
                if (results is { Count: 0 })
                {
                    results.Add(dt);

                    continue;
                }

                var split = dt - results.First();

                if (split <= windowLength)
                {
                    results.Add(dt);
                }
                else
                {
                    yield return results.Select(d => d - results.First()).ToArray();

                    results = new() { dt };
                }
            }

            if (results.Any())
            {
                yield return results.Select(d => d - results.First()).ToArray();
            }
        }
    }
}