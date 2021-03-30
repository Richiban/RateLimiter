using System;
using System.Threading.Tasks;

namespace Richiban.Regulator
{
    internal class NullRateLimiter : IRateLimit
    {
        public TimeSpan? TimeToWait => TimeSpan.Zero;
        public Task WaitAsync() => Task.CompletedTask;

        public void Done()
        {
        }
    }
}