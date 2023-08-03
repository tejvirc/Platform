namespace Aristocrat.Monaco.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security.Cryptography;
    using System.Threading;

    /// <summary>
    /// 
    /// </summary>
    public class Benchmark : IDisposable
    {
        private readonly Stopwatch timer = new Stopwatch();
        private readonly string benchmarkName;

        private static object syncCumulatedValues = new();
        private static long cumulatedTimeSpan = 0;
        private static int cumulatedBenchmarkCount = 0;

        private static long cumulatedCommitTimeSpan = 0;
        private static int cumulatedCommitBenchmarkCount = 0;
        private static long cumulatedCommitsAvg = 0;

        private static long cumulatedDuplicateCommitsTimeSpan = 0;
        private static int cumulatedDuplicateCommitsCount = 0;

        private static int totalPendingCommits = 0;
        private static int pendingCommitsCount = 0;

        private int threadId;
        private static object syncHashCompute = new();
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

            if (benchmarkName == "Commit")
            {
                var pending = Interlocked.Increment(ref pendingCommitsCount);
                var totalPending = Interlocked.Add(ref totalPendingCommits, pendingCommitsCount);
                if (pending > 1)
                {
                    var totalCommits = cumulatedCommitBenchmarkCount;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"----- PENDING: [{pending}] - " +
                        $"Thread Id=[{Environment.CurrentManagedThreadId}] - Avg=({totalPending})/({totalCommits})=[{(totalCommits > 0 ? ((double)(totalPending) / totalCommits) : totalPending):F4}] ----");
                }
            }

            threadId = Environment.CurrentManagedThreadId;
            this.benchmarkName = benchmarkName;
            timer.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (benchmarkName == "Commit")
                Interlocked.Decrement(ref pendingCommitsCount);

            threadId = Environment.CurrentManagedThreadId;
            timer.Stop();

            var elapsedTime = timer.Elapsed.Ticks;

            var totalAvg = TimeSpan.Zero;
            lock (syncCumulatedValues)
            {
                Interlocked.Add(ref cumulatedTimeSpan, elapsedTime);
                Interlocked.Increment(ref cumulatedBenchmarkCount);
                totalAvg = TimeSpan.FromTicks(cumulatedTimeSpan / cumulatedBenchmarkCount);
            }

            if (benchmarkName == "Commit")
            {
                //lock (syncCumulatedCommitValues)
                //{
                Interlocked.Add(ref cumulatedCommitTimeSpan, elapsedTime);
                Interlocked.Increment(ref cumulatedCommitBenchmarkCount);
                Interlocked.Exchange(ref cumulatedCommitsAvg, cumulatedCommitTimeSpan / cumulatedCommitBenchmarkCount);
                //}
            }

            if (elapsedTime > 1000000)
                Console.ForegroundColor = ConsoleColor.Red;
            else if (elapsedTime > 100000)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else
                Console.ForegroundColor = ConsoleColor.Gray;

            if (keyValuePairs != null)
            {
                if (Console.ForegroundColor == ConsoleColor.White)
                    Console.ForegroundColor = ConsoleColor.Gray;

                //Console.WriteLine($"Thread Id=[{Environment.CurrentManagedThreadId}] : ");

                foreach (var kv in keyValuePairs)
                {
                    var hash = string.Empty;
                    lock (syncHashCompute)
                    {
                        hash = kv.Value is byte[]? Convert.ToHexString(benchmarkSHA256.ComputeHash((byte[])kv.Value)) : kv.Value?.ToString();
                    }
                    var length = kv.Value is byte[]? ((byte[])kv.Value).Length : kv.Value?.ToString().Length;

                    if (benchmarkName == "Commit" && length > 100)
                    {
                        if (commitHistoryDictionary.TryGetValue(kv.Key, out var value))
                        {
                            if (value.ContainsKey(hash))
                            {
                                Console.BackgroundColor = ConsoleColor.DarkCyan;
                                Console.WriteLine($"[{Environment.CurrentManagedThreadId}][{kv.Key}]=hash[{hash}][{length}];");
                                Console.BackgroundColor = ConsoleColor.Black;

                                Interlocked.Increment(ref cumulatedDuplicateCommitsCount);
                                Interlocked.Add(ref cumulatedDuplicateCommitsTimeSpan, elapsedTime);

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

                    Console.WriteLine($"[{Environment.CurrentManagedThreadId}][{kv.Key}]=hash[{hash}][{length}];");
                }
            }

            Console.WriteLine($"{benchmarkName} - [{Environment.CurrentManagedThreadId}] - [{DateTime.Now}] - [{elapsedTime}];Total=[{TimeSpan.FromTicks(cumulatedTimeSpan)}][{cumulatedBenchmarkCount}];" +
                $"Avg=[{totalAvg}];Commit Total=[{TimeSpan.FromTicks(cumulatedCommitTimeSpan)}][{cumulatedCommitBenchmarkCount}];" +
                $"Commit Avg=[{cumulatedCommitsAvg}];Duplicates Total=[{TimeSpan.FromTicks(cumulatedDuplicateCommitsTimeSpan)}][{cumulatedDuplicateCommitsCount}];");
        }
    }
}
