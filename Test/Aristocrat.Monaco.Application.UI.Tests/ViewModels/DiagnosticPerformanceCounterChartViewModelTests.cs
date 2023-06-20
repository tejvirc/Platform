namespace Aristocrat.Monaco.Application.UI.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using Contracts.Localization;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Localization.Properties;
    using Moq;
    using OxyPlot.Axes;
    using PerformanceCounter;
    using Test.Common;
    using UI.ViewModels;

    [TestClass]
    public class DiagnosticPerformanceCounterChartViewModelTests
    {
        private DiagnosticPerformanceCounterChartViewModel _diagnosticViewDetailChartViewModel;
        private Mock<IPerformanceCounterManager> _performanceCounterManagerMock;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Strict);
            _performanceCounterManagerMock =
                MoqServiceManager.CreateAndAddService<IPerformanceCounterManager>(MockBehavior.Default);

            if (Application.Current == null)
            {
                var _ = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            }
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void DefaultCreation()
        {
            _diagnosticViewDetailChartViewModel =
                new DiagnosticPerformanceCounterChartViewModel(_performanceCounterManagerMock.Object);

            Assert.AreEqual(_diagnosticViewDetailChartViewModel.StartDate, DateTime.Today.AddDays(-29));
            Assert.AreEqual(_diagnosticViewDetailChartViewModel.StartDateForChart, DateTime.Today);
            Assert.AreEqual(_diagnosticViewDetailChartViewModel.EndDate, DateTime.Today);
            Assert.AreEqual(_diagnosticViewDetailChartViewModel.EndDateForChart, DateTime.Today);
            Assert.IsNull(_diagnosticViewDetailChartViewModel.MonacoPlotModel);
            Assert.IsFalse(_diagnosticViewDetailChartViewModel.IsLoadingChart);
            Assert.IsFalse(_diagnosticViewDetailChartViewModel.MagnifyMinusEnabled);
            Assert.IsFalse(_diagnosticViewDetailChartViewModel.ZoomingOrPanningDone);
            Assert.IsFalse(_diagnosticViewDetailChartViewModel.MagnifyPlusEnabled);
            Assert.IsNull(_diagnosticViewDetailChartViewModel.Text);
        }

        [TestMethod]
        public void StartDateGreaterThanEndDate()
        {
            _performanceCounterManagerMock.Setup(
                m => m.CountersForParticularDuration(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>())).Returns(
                Task.FromResult<IList<PerformanceCounters>>(
                    new List<PerformanceCounters>()));

            DefaultCreation();

            _diagnosticViewDetailChartViewModel.StartDateForChart = DateTime.Today.AddDays(-25);
            _diagnosticViewDetailChartViewModel.EndDateForChart = DateTime.Today.AddDays(-25);
            _diagnosticViewDetailChartViewModel.StartDateForChart = DateTime.Today.AddDays(-20);

            Assert.AreEqual(
                _diagnosticViewDetailChartViewModel.EndDateForChart,
                _diagnosticViewDetailChartViewModel.StartDateForChart);
            Assert.AreEqual(_diagnosticViewDetailChartViewModel.EndDateForChart, DateTime.Today.AddDays(-20));

            CheckForErrorScenarios();
        }

        [TestMethod]
        public void FetchingDataCorrectly()
        {
            _performanceCounterManagerMock.Setup(
                m => m.CountersForParticularDuration(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>())).Returns(
                Task.FromResult<IList<PerformanceCounters>>(
                    new List<PerformanceCounters>
                    {
                        new PerformanceCounters
                        {
                            DateTime = DateTime.Today.AddMinutes(1),
                            Values = new[] { 10.123, 23, 1234.5678, 3456.789, 123.456, 5, 3.12 }
                        },
                        new PerformanceCounters
                        {
                            DateTime = DateTime.Today.AddMinutes(2),
                            Values = new[] { 11.23, 32, 102.678, 313.789, 123.456, 5, 3.12 }
                        }
                    }));

            DefaultCreation();

            _diagnosticViewDetailChartViewModel.StartDateForChart = DateTime.Today;

            Assert.IsFalse(_diagnosticViewDetailChartViewModel.TextEnabled);
            Assert.IsTrue(string.IsNullOrEmpty(_diagnosticViewDetailChartViewModel.Text));
            Assert.IsTrue(_diagnosticViewDetailChartViewModel.MagnifyPlusEnabled);
            Assert.IsNotNull(_diagnosticViewDetailChartViewModel.MonacoPlotModel);
            Assert.IsNotNull(_diagnosticViewDetailChartViewModel.MonacoPlotModel.Axes);
            Assert.AreEqual(_diagnosticViewDetailChartViewModel.MonacoPlotModel.Axes.Count, 2);

            foreach (var axis in _diagnosticViewDetailChartViewModel.MonacoPlotModel.Axes)
            {
                Assert.IsTrue(axis.IsPanEnabled);
                Assert.IsTrue(axis.IsZoomEnabled);
            }

            foreach (var metric in _diagnosticViewDetailChartViewModel.AllMetrics)
            {
                Assert.IsTrue(metric.MetricEnabled);
                Assert.IsNotNull(metric.LineSeries);
                Assert.AreEqual(metric.LineSeries.Points.Count, 2);
            }

            Assert.AreEqual(_diagnosticViewDetailChartViewModel.AllMetrics.Select(x => x.MaxDataValue).Max(), 3456.789);
        }

        [TestMethod]
        public void WhenOperatorCancelledExceptionIsThrown()
        {
            _performanceCounterManagerMock.Setup(
                m => m.CountersForParticularDuration(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>())).ThrowsAsync(new OperationCanceledException());

            DefaultCreation();

            _diagnosticViewDetailChartViewModel.StartDateForChart = DateTime.Today;

            CheckForErrorScenarios();
        }

        [TestMethod]
        public void WhenTaskCancelledExceptionIsThrown()
        {
            _performanceCounterManagerMock.Setup(
                m => m.CountersForParticularDuration(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>())).ThrowsAsync(new TaskCanceledException());

            DefaultCreation();

            _diagnosticViewDetailChartViewModel.StartDateForChart = DateTime.Today;

            CheckForErrorScenarios();
        }

        [TestMethod]
        public void TestingFilteringAndFillingOutOfData()
        {
            _performanceCounterManagerMock.Setup(
                m => m.CountersForParticularDuration(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>())).Returns(
                Task.FromResult<IList<PerformanceCounters>>(
                    new List<PerformanceCounters>
                    {
                        new PerformanceCounters // is used for average value calculation
                        {
                            DateTime = DateTime.Today.AddDays(-2).AddMinutes(10),
                            Values = new[] { 1.0, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17}
                        },
                        new PerformanceCounters // is used for average value calculation
                        {
                            DateTime = DateTime.Today.AddDays(-2).AddMinutes(20),
                            Values = new[] { 1.0, 1, 3.1, 4, 5, 1.5, 7, 8, 8, 10.5, 12, 13.5, 15, 18, 13, 16.5, 24 }
                        },
                        new PerformanceCounters // is used for average value calculation
                        {
                            DateTime = DateTime.Today.AddDays(-2).AddMinutes(30),
                            Values = new[] { 1.0, 3, 2.9, 4, 5, 10.5, 7, 8, 10, 9.5, 10, 10.5, 11, 10, 17, 15.5, 10 }
                        },
                        new PerformanceCounters
                        {
                            DateTime = DateTime.Today.AddDays(-1).AddMinutes(1),
                            Values = new[] { 1.0, 2, 3, 4, 5, 6, 6, 5, 4, 3, 10.1, 20.1, 100.1, 200.1, 200, 100, 50 }
                        },
                        new PerformanceCounters
                        {
                            DateTime = DateTime.Today.AddDays(-1).AddMinutes(2),
                            Values = new[] { 2.0, 4, 2, 4, 12, 6, 2, 8, 10, 6, 20, 40, 200, 404, 999, 666, 100 }
                        },
                        new PerformanceCounters
                        {
                            DateTime = DateTime.Today.AddDays(-1).AddMinutes(3),
                            Values = new[] { 3.0, 6, 1, 4, 1, 6, 10, 7, 3, 5, 3.0, 6, 1, 4, 1, 6, 10, }
                        },
                        new PerformanceCounters
                        {
                            DateTime = DateTime.Today.AddMinutes(10),
                            Values = new[] { 10.123, 23, 1234.5678, 3456.789, 123.456, 5, 3.12, 0.1, 0.3, 0.79, 20.345, 40, 1234.567, 3456.789, 123.456, 50, 30.1}
                        },
                        new PerformanceCounters
                        {
                            DateTime = DateTime.Today.AddMinutes(15),
                            Values = new[] { 11.23, 32, 102.678, 3456.789, 123.456, 5, 3.12, 0.2, 0.6, 0.34, 11.23, 32, 300, 888.88, 77.7, 55, 33.3 }
                        },
                        new PerformanceCounters
                        {
                            DateTime = DateTime.Today.AddMinutes(30),
                            Values = new[] { 10.123, 23, 1234.5678, 3456.789, 123.456, 5, 3.12, 0.9, 0.8, 0.7, 10.123, 23.23, 1111.11, 3333.33, 222.22, 0.5, 0.1 }
                        }
                    }));

            DefaultCreation();

            _diagnosticViewDetailChartViewModel.StartDateForChart = DateTime.Today.AddDays(-2);

            Assert.IsFalse(_diagnosticViewDetailChartViewModel.TextEnabled);
            Assert.IsTrue(string.IsNullOrEmpty(_diagnosticViewDetailChartViewModel.Text));
            Assert.IsTrue(_diagnosticViewDetailChartViewModel.MagnifyPlusEnabled);
            Assert.IsNotNull(_diagnosticViewDetailChartViewModel.MonacoPlotModel);
            Assert.IsNotNull(_diagnosticViewDetailChartViewModel.MonacoPlotModel.Axes);
            Assert.AreEqual(_diagnosticViewDetailChartViewModel.MonacoPlotModel.Axes.Count, 2);

            foreach (var axis in _diagnosticViewDetailChartViewModel.MonacoPlotModel.Axes)
            {
                Assert.IsTrue(axis.IsPanEnabled);
                Assert.IsTrue(axis.IsZoomEnabled);
            }

            var i = 1;
            foreach (var metric in _diagnosticViewDetailChartViewModel.AllMetrics)
            {
                Assert.IsTrue(metric.MetricEnabled);
                Assert.IsNotNull(metric.LineSeries);
                Assert.AreEqual(7, metric.LineSeries.Points.Count);
                Assert.AreEqual(0, metric.LineSeries.Points[1].Y);
                Assert.AreEqual(0, metric.LineSeries.Points[2].Y);
                Assert.AreEqual(0, metric.LineSeries.Points[4].Y);
                Assert.AreEqual(0, metric.LineSeries.Points[5].Y);
                // Note: What this is actually testing is the average of the first 3 datapoints.
                Assert.AreEqual(i++, metric.LineSeries.Points[0].Y);
            }

            Assert.AreEqual(
                _diagnosticViewDetailChartViewModel.AllMetrics[0].LineSeries.Points[0].X,
                Axis.ToDouble(DateTime.Today.AddDays(-2).AddMinutes(30)));
            Assert.AreEqual(
                _diagnosticViewDetailChartViewModel.AllMetrics[1].LineSeries.Points[0].X,
                Axis.ToDouble(DateTime.Today.AddDays(-2).AddMinutes(30)));
            Assert.AreEqual(
                _diagnosticViewDetailChartViewModel.AllMetrics[2].LineSeries.Points[0].X,
                Axis.ToDouble(DateTime.Today.AddDays(-2).AddMinutes(30)));
            Assert.AreEqual(
                _diagnosticViewDetailChartViewModel.AllMetrics[3].LineSeries.Points[0].X,
                Axis.ToDouble(DateTime.Today.AddDays(-2).AddMinutes(30)));
            Assert.AreEqual(
                _diagnosticViewDetailChartViewModel.AllMetrics[4].LineSeries.Points[0].X,
                Axis.ToDouble(DateTime.Today.AddDays(-2).AddMinutes(30)));
            Assert.AreEqual(
                _diagnosticViewDetailChartViewModel.AllMetrics[5].LineSeries.Points[0].X,
                Axis.ToDouble(DateTime.Today.AddDays(-2).AddMinutes(30)));
            Assert.AreEqual(
                _diagnosticViewDetailChartViewModel.AllMetrics[6].LineSeries.Points[0].X,
                Axis.ToDouble(DateTime.Today.AddDays(-2).AddMinutes(30)));

            Assert.AreEqual(
                _diagnosticViewDetailChartViewModel.AllMetrics[0].LineSeries.Points[1].X,
                Axis.ToDouble(DateTime.Today.AddDays(-2).AddMinutes(31)));
            Assert.AreEqual(
                _diagnosticViewDetailChartViewModel.AllMetrics[1].LineSeries.Points[1].X,
                Axis.ToDouble(DateTime.Today.AddDays(-2).AddMinutes(31)));
            Assert.AreEqual(
                _diagnosticViewDetailChartViewModel.AllMetrics[2].LineSeries.Points[1].X,
                Axis.ToDouble(DateTime.Today.AddDays(-2).AddMinutes(31)));
            Assert.AreEqual(
                _diagnosticViewDetailChartViewModel.AllMetrics[3].LineSeries.Points[1].X,
                Axis.ToDouble(DateTime.Today.AddDays(-2).AddMinutes(31)));
            Assert.AreEqual(
                _diagnosticViewDetailChartViewModel.AllMetrics[4].LineSeries.Points[1].X,
                Axis.ToDouble(DateTime.Today.AddDays(-2).AddMinutes(31)));
            Assert.AreEqual(
                _diagnosticViewDetailChartViewModel.AllMetrics[5].LineSeries.Points[1].X,
                Axis.ToDouble(DateTime.Today.AddDays(-2).AddMinutes(31)));
            Assert.AreEqual(
                _diagnosticViewDetailChartViewModel.AllMetrics[6].LineSeries.Points[1].X,
                Axis.ToDouble(DateTime.Today.AddDays(-2).AddMinutes(31)));

            Assert.AreEqual(_diagnosticViewDetailChartViewModel.AllMetrics[0].LineSeries.Points[3].Y, 2);
            Assert.AreEqual(_diagnosticViewDetailChartViewModel.AllMetrics[1].LineSeries.Points[3].Y, 4);
            Assert.AreEqual(_diagnosticViewDetailChartViewModel.AllMetrics[2].LineSeries.Points[3].Y, 2);
            Assert.AreEqual(_diagnosticViewDetailChartViewModel.AllMetrics[3].LineSeries.Points[3].Y, 4);
            Assert.AreEqual(_diagnosticViewDetailChartViewModel.AllMetrics[4].LineSeries.Points[3].Y, 6);
            Assert.AreEqual(_diagnosticViewDetailChartViewModel.AllMetrics[5].LineSeries.Points[3].Y, 6);
            Assert.AreEqual(_diagnosticViewDetailChartViewModel.AllMetrics[6].LineSeries.Points[3].Y, 6);

            Assert.AreEqual(_diagnosticViewDetailChartViewModel.AllMetrics.Select(x => x.MaxDataValue).Max(), 3456.789);
        }

        private void CheckForErrorScenarios()
        {
            Assert.AreEqual(
                _diagnosticViewDetailChartViewModel.Text,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorWhileFetchingData));
            Assert.IsFalse(_diagnosticViewDetailChartViewModel.MagnifyPlusEnabled);
            Assert.IsFalse(_diagnosticViewDetailChartViewModel.MagnifyMinusEnabled);
            Assert.IsFalse(_diagnosticViewDetailChartViewModel.ZoomingOrPanningDone);
            Assert.IsNotNull(_diagnosticViewDetailChartViewModel.MonacoPlotModel);
            Assert.IsNotNull(_diagnosticViewDetailChartViewModel.MonacoPlotModel.Axes);
            Assert.AreEqual(_diagnosticViewDetailChartViewModel.MonacoPlotModel.Axes.Count, 2);

            foreach (var axis in _diagnosticViewDetailChartViewModel.MonacoPlotModel.Axes)
            {
                Assert.IsFalse(axis.IsPanEnabled);
                Assert.IsFalse(axis.IsZoomEnabled);
            }

            foreach (var metric in _diagnosticViewDetailChartViewModel.AllMetrics)
            {
                Assert.IsFalse(metric.MetricEnabled);
                Assert.IsNotNull(metric.LineSeries);
                Assert.AreEqual(metric.LineSeries.Points.Count, 0);
            }
        }
    }
}