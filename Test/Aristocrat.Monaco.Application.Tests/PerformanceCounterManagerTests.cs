namespace Aristocrat.Monaco.Application.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using PerformanceCounter;
    using Aristocrat.Monaco.Hardware.Contracts.IdReader;
    using ProtoBuf;

    [TestClass]
    [Ignore("These tests will be moved to the integration test suite.")]
    public class PerformanceCounterManagerTests
    {
        private PerformanceCounterManager _performanceCounterManager;
        private Mock<IPathMapper> _pathMapper;
        private Mock<IPropertiesManager> _properties;

        private static string LogDirectoryPath =>
            Path.Combine(Environment.CurrentDirectory, @"./PerformanceCounterLogs/");

        [TestInitialize]
        public void Initialize()
        {
            if (!Directory.Exists(LogDirectoryPath))
            {
                Directory.CreateDirectory(LogDirectoryPath);
            }

            _pathMapper = new Mock<IPathMapper>(MockBehavior.Strict);
            _pathMapper.Setup(x => x.GetDirectory(It.IsAny<string>()))
                .Returns(new DirectoryInfo(Environment.CurrentDirectory));

            _properties = new Mock<IPropertiesManager>(MockBehavior.Loose);
            _properties.Setup(x => x.GetProperty(It.IsAny<string>(), It.IsAny<object>())).Returns("1");
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _performanceCounterManager?.Dispose();
        }

        [DataRow(true, false)]
        [DataRow(false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenArgumentIsNullExpectException(bool nullPathMapper, bool nullProperties)
        {
            _performanceCounterManager = new PerformanceCounterManager(
                nullPathMapper ? null : _pathMapper.Object,
                nullProperties ? null : _properties.Object);
        }

        [TestMethod]
        public void GetCurrentCounters()
        {
            _performanceCounterManager = new PerformanceCounterManager(_pathMapper.Object, _properties.Object);

            _performanceCounterManager.Initialize();

            var counters = _performanceCounterManager.CurrentPerformanceCounter;

            Assert.IsTrue(counters.CounterDictionary[MetricType.TotalProcessorTime] >= 0);
            Assert.IsTrue(counters.CounterDictionary[MetricType.FreeMemory] >= 0);
            Assert.IsTrue(counters.CounterDictionary[MetricType.MonacoThreadCount] >= 0);
            Assert.IsTrue(counters.CounterDictionary[MetricType.MonacoProcessorTime] >= 0);
            Assert.IsTrue(counters.CounterDictionary[MetricType.MonacoPrivateBytes] >= 0);
            Assert.IsTrue(counters.CounterDictionary[MetricType.ClrBytes] >= 0);
            Assert.AreEqual(counters.CounterDictionary[MetricType.GdkPrivateBytes], 0);
            Assert.AreEqual(counters.CounterDictionary[MetricType.GdkThreadCount], 0);
            Assert.AreEqual(counters.CounterDictionary[MetricType.GdkProcessorTime], 0);
        }

        [TestMethod]
        public async Task GetCountersForParticularDurationWrongDate()
        {
            await Task.Run(
                async () =>
                {
                    _performanceCounterManager = new PerformanceCounterManager(_pathMapper.Object, _properties.Object);
                    _performanceCounterManager.Initialize();

                    var counterList = await _performanceCounterManager.CountersForParticularDuration(
                        DateTime.Today.AddDays(-356),
                        DateTime.Today.AddDays(-40),
                        new CancellationTokenSource(TimeSpan.FromMinutes(5)).Token);

                    Assert.AreEqual(counterList.Count, 0);
                }
            );
        }

        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task GetCountersForAlreadyCancelledToken()
        {
            await Task.Run(
                async () =>
                {
                    _performanceCounterManager = new PerformanceCounterManager(_pathMapper.Object, _properties.Object);
                    _performanceCounterManager.Initialize();

                    var counterList = await _performanceCounterManager.CountersForParticularDuration(
                        DateTime.Today.AddDays(-30),
                        DateTime.Today,
                        new CancellationToken(true));

                    Assert.AreEqual(counterList.Count, 0);
                });
        }

        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task GetCountersForCancelledToken()
        {
            await Task.Run(
                async () =>
                {
                    var startDate = DateTime.Today.AddDays(-20);
                    var endDate = DateTime.Today.AddDays(-10);

                    CleanUpLogFiles(startDate, endDate);

                    _performanceCounterManager = new PerformanceCounterManager(_pathMapper.Object, _properties.Object);
                    _performanceCounterManager.Initialize();

                    CreateLogFiles(startDate, endDate);

                    var cancellationToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(10)).Token;

                    var counterList = await _performanceCounterManager.CountersForParticularDuration(
                        DateTime.Today.AddDays(-30),
                        DateTime.Today,
                        cancellationToken);

                    Assert.AreEqual(counterList.Count, 0);
                });
        }

        [TestMethod]
        public void VerifyCurrentCounters()
        {
            _performanceCounterManager = new PerformanceCounterManager(_pathMapper.Object, _properties.Object);
            _performanceCounterManager.Initialize();
            Thread.Sleep(TimeSpan.FromSeconds(5));
            var counterList = _performanceCounterManager.CountersForParticularDuration(
                DateTime.Today,
                DateTime.Today,
                new CancellationTokenSource(TimeSpan.FromMinutes(5)).Token);
            var counterListResult = counterList.Result;
            Assert.IsTrue(counterListResult.Count >= 1);

            foreach (var counter in counterListResult)
            {
                Assert.IsTrue(counter.CounterDictionary[MetricType.TotalProcessorTime] >= 0);
                Assert.IsTrue(counter.CounterDictionary[MetricType.FreeMemory] >= 0);
                Assert.IsTrue(counter.CounterDictionary[MetricType.MonacoThreadCount] >= 0);
                Assert.IsTrue(counter.CounterDictionary[MetricType.MonacoProcessorTime] >= 0);
                Assert.IsTrue(counter.CounterDictionary[MetricType.MonacoPrivateBytes] >= 0);
                Assert.IsTrue(counter.CounterDictionary[MetricType.ClrBytes] >= 0);
                Assert.AreEqual(counter.CounterDictionary[MetricType.GdkPrivateBytes], 0);
                Assert.AreEqual(counter.CounterDictionary[MetricType.GdkThreadCount], 0);
                Assert.AreEqual(counter.CounterDictionary[MetricType.GdkProcessorTime], 0);
            }
        }

        [TestMethod]
        public void CheckRedundantFiles()
        {
            FileStream create;

            for (var i = 0; i < 30; i++)
            {
                var tempFileName = "file_" + i + ".bin";
                create = File.Create(LogDirectoryPath + tempFileName);
                create.Close();
                var _ =
                    new FileInfo(LogDirectoryPath + tempFileName) { LastWriteTime = DateTime.Today.AddDays(-i) };
            }

            const string fileName = "FileToDelete";
            create = File.Create(LogDirectoryPath + fileName);
            create.Close();
            var __ = new FileInfo(LogDirectoryPath + fileName) { LastWriteTime = DateTime.Today.AddDays(-31) };

            _performanceCounterManager = new PerformanceCounterManager(_pathMapper.Object, _properties.Object);
            _performanceCounterManager.Initialize();

            Assert.IsFalse(File.Exists(LogDirectoryPath + fileName));
        }

        private static string GetLogFileName(DateTime dateTime)
        {
            return LogDirectoryPath + "PerformanceCounters" + dateTime.ToString("dd-MM-yyyy") + ".bin";
        }

        private static void CleanUpLogFiles(DateTime startDate, DateTime endDate)
        {
            while (startDate <= endDate)
            {
                var fileName = GetLogFileName(startDate);

                if (File.Exists(fileName))
                {
                    try
                    {
                        File.Delete(fileName);
                    }
                    catch (Exception)
                    {
                        //ignore
                    }
                }

                startDate = startDate.AddDays(1);
            }
        }

        private static void CreateLogFiles(DateTime startDate, DateTime endDate)
        {
            while (startDate <= endDate)
            {
                var fileName = GetLogFileName(startDate);

                if (!File.Exists(fileName))
                {
                    try
                    {
                        var file = File.Create(fileName);
                        file.Close();
                    }
                    catch (Exception)
                    {
                        //ignore
                    }
                }

                var compDate = startDate;
                var baseDate = startDate;


                var rand = new Random();

                while (baseDate < compDate.AddDays(1))
                {
                    var values = new List<double>
                    {
                        rand.NextDouble(),
                        rand.Next(0, 200),
                        rand.NextDouble(),
                        rand.NextDouble(),
                        rand.Next(0, 100),
                        rand.Next(0, 200),
                        rand.NextDouble()
                    };

                    var listOfCountersForDuration =
                        new PerformanceCounters { DateTime = baseDate, Values = values.ToArray() };

                    using (var outputStream =
                        new FileStream(fileName, FileMode.Append, FileAccess.Write))
                    {
                        Serializer.Serialize(outputStream, listOfCountersForDuration);
                    }

                    baseDate = baseDate.AddMinutes(1);
                }

                startDate = startDate.AddDays(1);
            }
        }
    }
}