namespace Aristocrat.Monaco.Application.PerformanceCounter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using Kernel;
    using log4net;
    using Timer = System.Timers.Timer;

    /// <summary>
    ///     Implements the IPerformanceCounterManager which allows us to set up and read system
    ///     performance counters such as CPU usage or free memory
    /// </summary>
    internal class PerformanceCounterManager : IPerformanceCounterManager, IService, IDisposable
    {
        private const string PerformanceCounterIntervalKey = "performanceCounterInterval";
        private const string DirectoryName = "PerformanceCounterLogs";
        private const string LogNamePrefix = "PerformanceCounters";
        private const string LogNameSuffix = "dd-MM-yyyy";
        private const string LogFileExtension = ".bin";
        private const string Logs = @"/Logs";
        private const int MinimumDaysToKeepLogs = 30;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPathMapper _pathMapper;
        private readonly object _lock = new object();
        private readonly int _performanceCounterInterval;

        private Timer _pollingTimer;
        private string _logDirectoryPath;
        private bool _disposed;

        public PerformanceCounterManager()
            : this(ServiceManager.GetInstance().GetService<IPathMapper>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        public PerformanceCounterManager(IPathMapper pathMapper, IPropertiesManager properties)
        {
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));
            _performanceCounterInterval =
                int.Parse(properties.GetValue(PerformanceCounterIntervalKey, "15"));
        }

        private List<PlatformMetric> Metrics { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public PerformanceCounters CurrentPerformanceCounter => GetAllCountersData();

        /// <inheritdoc />
        public Task<IList<PerformanceCounters>> CountersForParticularDuration(
            DateTime startDate,
            DateTime endDate,
            CancellationToken token)
        {
            return Task.Run(
                () =>
                {
                    IList<PerformanceCounters> listOfCounters = new List<PerformanceCounters>();

                    Enumerable.Range(0, (endDate - startDate).Days + 1)
                        .Select(days => GetLogFileName(startDate.AddDays(days))).Where(File.Exists)
                        .ToList().ForEach(
                            file =>
                            {
                                token.ThrowIfCancellationRequested();
                                ReadFile(file, listOfCounters);
                            });
                    return listOfCounters;
                },
                token);
        }

        /// <inheritdoc />
        public string Name => nameof(PerformanceCounterManager);

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes { get; } = new[] { typeof(IPerformanceCounterManager) };

        /// <inheritdoc />
        public void Initialize()
        {
            _logDirectoryPath = Path.Combine(_pathMapper.GetDirectory(Logs).FullName, DirectoryName);

            if (!Directory.Exists(_logDirectoryPath))
            {
                Directory.CreateDirectory(_logDirectoryPath);
            }

            Metrics = Enum.GetValues(typeof(MetricType)).Cast<MetricType>().Select(x => new PlatformMetric(x)).ToList();

            ClearRedundantFiles();

            InitializeAndStartTimer();

            Logger.Debug("PerformanceCounterManager Initialized");
        }

        /// <summary>
        ///     Reads a file of saved performance counter values so we can display the history of
        ///     the values that we monitor
        /// </summary>
        private static void ReadFile(string fileName, ICollection<PerformanceCounters> list)
        {
            using (var inputStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var formatter = new BinaryFormatter();

                try
                {
                    while (inputStream.Position < inputStream.Length)
                    {
                        //deserialize each object in the file
                        list.Add((PerformanceCounters)formatter.Deserialize(inputStream));
                    }
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_pollingTimer != null)
                {
                    _pollingTimer.Stop();
                    _pollingTimer.Dispose();
                    _pollingTimer = null;
                }
            }

            _disposed = true;
        }

        private PerformanceCounters GetAllCountersData()
        {
            return new PerformanceCounters { DateTime = DateTime.Now, Values = Metrics.Select(x => x.Value).ToArray() };
        }

        private void InitializeAndStartTimer()
        {
            _pollingTimer = new Timer();
            _pollingTimer.Elapsed += OnLoggingTimerElapsed;
            _pollingTimer.Interval = TimeSpan.FromSeconds(_performanceCounterInterval).TotalMilliseconds;
            _pollingTimer.AutoReset = true;
            _pollingTimer.Enabled = true;
            _pollingTimer?.Start();
        }

        private string GetLogFileName(DateTime dateTime)
        {
            return Path.Combine(
                _logDirectoryPath,
                LogNamePrefix
                + dateTime.ToString(LogNameSuffix)
                + LogFileExtension);
        }

        private void OnLoggingTimerElapsed(object sender, ElapsedEventArgs _)
        {
            lock (_lock)
            {
                var logFileName = GetLogFileName(DateTime.Today);

                if (!File.Exists(logFileName))
                {
                    ClearRedundantFiles();
                }

                // Flush Data to the disk
                var formatter = new BinaryFormatter();

                using (var outputStream =
                    new FileStream(logFileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    try
                    {
                        formatter.Serialize(outputStream, GetAllCountersData());
                    }
                    catch (EndOfStreamException e)
                    {
                        Logger.Debug($"Failed to serialize error message = {e.Message}");
                    }
                }
            }
        }

        private void ClearRedundantFiles()
        {
            try
            {
                new DirectoryInfo(_logDirectoryPath).GetFiles().OrderByDescending(f => f.LastWriteTime)
                    .Skip(MinimumDaysToKeepLogs)
                    .ToList()
                    .ForEach(f => f.Delete());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}