using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public static class ParallelExecutor
    {
        [DebuggerStepThrough]
        public static void RunParallelTest(long totalParallelIter, long totalIter, Action action)
        {
            var tasks = new List<Task>();
            for (var parallelIter = 0; parallelIter < totalParallelIter; parallelIter++)
            {
                var parallelIterObj = parallelIter;
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    for (int iter = 0; iter < totalIter; iter++)
                    {
                        try
                        {
                            Trace.WriteLine($"[{DateTime.UtcNow.ToString("u")}][@{parallelIterObj}/{iter}] start");
                            action.Invoke();
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine($"[{DateTime.UtcNow.ToString("u")}][@{parallelIterObj}/{iter}] Exception: {ex.Message}");
                            Thread.Sleep(1000);
                        }
                    }
                }, TaskCreationOptions.LongRunning));
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}