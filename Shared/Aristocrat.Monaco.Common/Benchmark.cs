namespace Aristocrat.Monaco.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;

    /// <summary>
    /// 
    /// </summary>
    public class Benchmark : IDisposable
    {
        private readonly Stopwatch timer = new Stopwatch();
        private readonly string benchmarkName;

        private static TimeSpan cumulatedTimeSpan = TimeSpan.Zero;

        private int threadId;

        private Dictionary<string, object> keyValuePairs = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get => keyValuePairs?.GetValueOrDefault(key);
            set
            {
                if (keyValuePairs == null)
                {
                    keyValuePairs = new Dictionary<string, object>();
                }
                keyValuePairs[key] = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="benchmarkName"></param>
        public Benchmark(string benchmarkName)
        {
            threadId = Environment.CurrentManagedThreadId;
            this.benchmarkName = benchmarkName;
            timer.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            threadId = Environment.CurrentManagedThreadId;
            timer.Stop();
            
            if (keyValuePairs != null)
            {
                var strBuilder = new StringBuilder();
                foreach (var kv in keyValuePairs)
                {
                    strBuilder.Append($"{kv.Key}=[{kv.Value}];");
                }

                Console.WriteLine(strBuilder.ToString());
            }

            var elapsedTime = timer.Elapsed;
            cumulatedTimeSpan += elapsedTime;

            if (elapsedTime > TimeSpan.FromSeconds(0.1))
                Console.ForegroundColor = ConsoleColor.Red;
            else if (elapsedTime > TimeSpan.FromSeconds(0.05))
                Console.ForegroundColor = ConsoleColor.Yellow;
            else
                Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine($"{DateTime.Now} - {benchmarkName} - {elapsedTime}; Thread Id={Environment.CurrentManagedThreadId}; Total=[{cumulatedTimeSpan}] -------------------------");
        }
    }
}
