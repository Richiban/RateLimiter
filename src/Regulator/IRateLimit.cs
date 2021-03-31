using System;
using System.Threading.Tasks;

namespace Richiban.Regulator
{
    public interface IRateLimit
    {
        Task IsReady();
        Task WaitAsync();
        void Done();
    }
}