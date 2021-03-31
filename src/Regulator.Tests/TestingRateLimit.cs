using System;
using System.Threading.Tasks;

namespace Richiban.Regulator.Tests
{
    class TestingRateLimit : IRateLimit
    {
        private readonly TimeSpan _timeToDelay;

        public TestingRateLimit(TimeSpan timeToWait)
        {
            TimeToWait = timeToWait;
            _timeToDelay = timeToWait;
        }

        public TimeSpan? TimeToWait { get; }

        public Task IsReady() => Task.Delay(_timeToDelay);

        public Task WaitAsync() => Task.Delay(_timeToDelay);

        public void Done()
        {
        }
    }
}