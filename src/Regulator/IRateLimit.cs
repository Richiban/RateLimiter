using System;
using System.Threading.Tasks;

namespace Richiban.Regulator
{
    public interface IRateLimit
    {
        public TimeSpan? TimeToWait { get; }
        Task WaitAsync();
        void Done();
    }
}