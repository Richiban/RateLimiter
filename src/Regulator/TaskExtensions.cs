using System;
using System.Threading.Tasks;

namespace Richiban.Regulator
{
    public static class TaskExtensions
    {
        public static async Task WithTimeout(this Task task, TimeSpan timeout)
        {
            if (task.IsCompleted)
            {
                return;
            }

            if (timeout > TimeSpan.Zero)
            {
                var timeoutTask = Task.Delay(timeout);

                var completedTask = await Task.WhenAny(task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException(
                        "The timeout limit was reached waiting for the rate limiter");
                }
            }

            await task;
        }
    }
}