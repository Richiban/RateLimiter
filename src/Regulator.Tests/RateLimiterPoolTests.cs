using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using NUnit.Framework;

namespace Richiban.Regulator.Tests
{
    public class RateLimiterPoolTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task TestTokenRotation()
        {
            var sut = new RateLimiterPool<string>(
                new[]
                {
                    ("tokenA",
                        new RateLimiter(
                            new SlidingWindowRateLimit(1, TimeSpan.FromMilliseconds(25)))),
                    ("tokenB",
                        new RateLimiter(
                            new SlidingWindowRateLimit(1, TimeSpan.FromMilliseconds(10))))
                });

            var usedTokens = new List<string>();

            await Task.WhenAll(
                Enumerable.Range(0, 5)
                    .Select(
                        _ => sut.WithNextAvailableDataAsync(
                            async token => { usedTokens.Add(token); })));

            Assert.That(
                usedTokens,
                Is.EquivalentTo(new[] { "tokenA", "tokenB", "tokenB", "tokenB", "tokenA" }));
        }

        [Test]
        public async Task TestTokenRotation2()
        {
            var sut = new RateLimiterPool<string>(
                new[] { "tokenA", "tokenB" },
                new[] { new SlidingWindowRateLimit(1, TimeSpan.FromMilliseconds(10)) });

            var usedTokens = new List<string>();

            await Task.WhenAll(
                Enumerable.Range(0, 4)
                    .Select(
                        _ => sut.WithNextAvailableDataAsync(
                            async token => { usedTokens.Add(token); })));

            Assert.That(
                usedTokens,
                Is.EquivalentTo(new[] { "tokenA", "tokenB", "tokenA", "tokenB" }));
        }
    }
}