namespace Aristocrat.Monaco.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// 
    /// </summary>
    public class Benchmark : IDisposable
    {
        private readonly Stopwatch timer = new Stopwatch();
        private readonly string benchmarkName;

        private static TimeSpan cumulatedTimeSpan = TimeSpan.Zero;
        private static TimeSpan cumulatedCommitTimeSpan = TimeSpan.Zero;
        private static int cumulatedBenchmarkCount = 0;
        private static int cumulatedCommitBenchmarkCount = 0;

        private int threadId;
        private static SHA256 benchmarkSHA256 = SHA256.Create();

        private Dictionary<string, object> keyValuePairs = null;

        private static ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> commitHistoryDictionary = new ConcurrentDictionary<string, ConcurrentDictionary<string, byte>>();

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

            var elapsedTime = timer.Elapsed;
            cumulatedTimeSpan += elapsedTime;
            cumulatedBenchmarkCount++;

            if (benchmarkName == "Commit")
            {
                cumulatedCommitBenchmarkCount++;
                cumulatedCommitTimeSpan += elapsedTime;
            }

            if (elapsedTime > TimeSpan.FromSeconds(0.1))
                Console.ForegroundColor = ConsoleColor.Red;
            else if (elapsedTime > TimeSpan.FromSeconds(0.01))
                Console.ForegroundColor = ConsoleColor.Yellow;
            else
                Console.ForegroundColor = ConsoleColor.Gray;

            if (keyValuePairs != null)
            {
                if (Console.ForegroundColor == ConsoleColor.White)
                    Console.ForegroundColor = ConsoleColor.Gray;

                Console.WriteLine($"Thread Id=[{Environment.CurrentManagedThreadId}] : ");

                foreach (var kv in keyValuePairs)
                {
                    var hash = kv.Value is byte[]? Convert.ToHexString(benchmarkSHA256.ComputeHash((byte[])kv.Value)) : kv.Value?.ToString();
                    var length = kv.Value is byte[]? ((byte[])kv.Value).Length : kv.Value?.ToString().Length;

                    if (benchmarkName == "Commit" && length > 32)
                    {
                        if (commitHistoryDictionary.TryGetValue(kv.Key, out var value))
                        {
                            if (value.ContainsKey(hash))
                            {
                                Console.BackgroundColor = ConsoleColor.Cyan;
                                Console.WriteLine($"[{kv.Key}]=hash[{hash}][{length}];");
                                Console.BackgroundColor = ConsoleColor.Black;

                                continue;
                            }
                            else
                            {
                                value[hash] = 1;
                            }
                        }
                        else
                        {
                            var newCommitHistory = new ConcurrentDictionary<string, byte>();
                            newCommitHistory[hash] = 1;
                            commitHistoryDictionary[kv.Key] = newCommitHistory;
                        }
                    }

                    Console.WriteLine($"[{kv.Key}]=hash[{hash}][{length}];");
                }
            }

            Console.WriteLine($"[{DateTime.Now} - {benchmarkName} - {elapsedTime}];Thread Id=[{Environment.CurrentManagedThreadId}];Total=[{cumulatedTimeSpan}][{cumulatedBenchmarkCount}];Avg=[{(cumulatedBenchmarkCount > 0 ? cumulatedTimeSpan.Ticks / cumulatedBenchmarkCount : 0)}];Commit Total=[{cumulatedCommitTimeSpan}][{cumulatedCommitBenchmarkCount}];Commit Avg=[{(cumulatedCommitBenchmarkCount > 0 ? cumulatedCommitTimeSpan.Ticks / cumulatedCommitBenchmarkCount : 0)}]");
        }
    }
}
