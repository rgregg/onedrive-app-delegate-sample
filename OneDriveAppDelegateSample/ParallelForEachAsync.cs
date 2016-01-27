using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneDriveAppDelegateSample
{
    internal static class ParallelForEachAsync
    {
        /// <summary>
        /// Parallel enabled for each processor that supports async lambdas. Copied from 
        /// http://blogs.msdn.com/b/pfxteam/archive/2012/03/05/10278165.aspx
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">Collection of items to iterate over</param>
        /// <param name="dop">Degree of parallelism to execute.</param>
        /// <param name="body">Lambda expression executed for each operation</param>
        /// <returns></returns>
        public static Task ForEachAsync<T>(this IEnumerable<T> source, int dop, Func<T, Task> body)
        {
            return Task.WhenAll(
                from partition in System.Collections.Concurrent.Partitioner.Create(source).GetPartitions(dop)
                select Task.Run(async delegate
                {
                    using (partition)
                        while (partition.MoveNext())
                            await body(partition.Current);
                }));
        }
    }
}
