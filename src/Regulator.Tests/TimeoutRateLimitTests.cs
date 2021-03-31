using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Richiban.Regulator.Tests
{
    public class TimeoutRateLimitTests
    {
        [Test]
        public void TestTimeout()
        {
            var rateLimiter =
                new RateLimiter(new TestingRateLimit(TimeSpan.FromMilliseconds(200)))
                {
                    WaitTimeout = TimeSpan.FromMilliseconds(100)
                };

            Assert.ThrowsAsync<TimeoutException>(
                () => rateLimiter.WhenReady(() => Task.Delay(200)));
        }
    }
}