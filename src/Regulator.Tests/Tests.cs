using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using NUnit.Framework;

namespace Richiban.Regulator.Tests
{
    public class RateLimiterTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task TestWaiting()
        {
            var rateLimiter = new RateLimiter(
                new SlidingWindowRateLimit(
                    count: 2,
                    timePeriod: TimeSpan.FromMilliseconds(100)));

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await Task.WhenAll(
                Enumerable.Range(0, 3)
                    .Select(_ => rateLimiter.WhenReady(() => Task.CompletedTask)));

            stopwatch.Stop();

            Assert.That(stopwatch.ElapsedMilliseconds, Is.GreaterThan(100));
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(130));
        }

        [Test]
        public async Task TestConcurrency()
        {
            var rateLimiter = new RateLimiter(new ConcurrencyRateLimit(2));

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await Task.WhenAll(
                Enumerable.Range(0, 3)
                    .Select(_ => rateLimiter.WhenReady(() => Task.Delay(100))));

            stopwatch.Stop();

            Assert.That(stopwatch.ElapsedMilliseconds, Is.GreaterThan(200));
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(230));
        }

        [Test]
        public void TestTimeout()
        {
            var rateLimiter = new RateLimiter(new DodgyRateLimit(TimeSpan.FromMilliseconds(200)))
            {
                WaitTimeout = TimeSpan.FromMilliseconds(100)
            };

            Assert.ThrowsAsync<TimeoutException>(
                () => rateLimiter.WhenReady(() => Task.Delay(200)));
        }

        class DodgyRateLimit : IRateLimit
        {
            public DodgyRateLimit(TimeSpan timeToWait)
            {
                TimeToWait = timeToWait;
            }

            public TimeSpan? TimeToWait { get; }

            public Task WaitAsync() => Task.Delay(TimeToWait.Value);

            public void Done()
            {
            }
        }
    }
}