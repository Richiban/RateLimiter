using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Richiban.Regulator
{
    public static class Extensions
    {
        public static async Task<T> WhenAny<T>(this IEnumerable<Task<T>> tasks)
        {
            return await await Task.WhenAny(tasks);
        }

        public static async Task WhenAny(this IEnumerable<Task> tasks)
        {
            await await Task.WhenAny(tasks);
        }
        public static async Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks)
        {
            return await Task.WhenAll(tasks);
        }

        public static async Task WhenAll(this IEnumerable<Task> tasks)
        {
            await Task.WhenAll(tasks);
        }
    }
}