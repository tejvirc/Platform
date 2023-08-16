namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Application.Helpers;
    using Contracts.Localization;
    using Kernel;
    using Monaco.Common;
    using Monaco.Localization.Properties;
    using MVVM;
    using MVVM.Command;
    using OperatorMenu;
    using OxyPlot;
    using OxyPlot.Axes;
    using OxyPlot.Legends;
    using OxyPlot.Wpf;
    using PerformanceCounter;
    using Axis = OxyPlot.Axes.Axis;
    using DateTimeAxis = OxyPlot.Axes.DateTimeAxis;
    using LinearAxis = OxyPlot.Axes.LinearAxis;
    using LineSeries = OxyPlot.Series.LineSeries;

    /// <summary>
    ///     The view model for the diagnostics "chart" page, the one that shows the history of
    ///     the platform performance counters
    /// </summary>
    [CLSCompliant(false)]
    public class DiagnosticPerformanceCounterChartViewModel : OperatorMenuPageViewModelBase
    {
        private readonly IPerformanceCounterManager _performanceCounterManager;

        private PlotModel _monacoPlotModel;
        private DateTime _startDate;
        private DateTime _endDate;
        private DateTime _startDateForChart;
        private DateTime _endDateForChart;
        private bool _isLoadingChart;
        private bool _isTextEnabled;
        private bool _isZoomingOrPanningDone;
        private string _text;
        private bool _magnifyPlusEnabled;
        private bool _magnifyMinusEnabled;
        private TimeSpan _timeSpan = TimeSpan.FromMinutes(5);
        private IList<PerformanceCounters> _countersDataList = new List<PerformanceCounters>();
        private CancellationTokenSource _cancellationToken;
        private int _numberOfElementsToSample;

        public DiagnosticPerformanceCounterChartViewModel()
            : this(ServiceManager.GetInstance().GetService<IPerformanceCounterManager>())
        {
        }

        public DiagnosticPerformanceCounterChartViewModel(IPerformanceCounterManager performanceCounterManager)
        {
            _performanceCounterManager = performanceCounterManager ??
                                         throw new ArgumentNullException(nameof(performanceCounterManager));
            _isTextEnabled = true;
            _startDate = DateTime.Today.AddDays(-29);
            _startDateForChart = DateTime.Today;
            _endDate = DateTime.Today;
            _endDateForChart = DateTime.Today;

            PopulateAvailableMetrics();

            ResetZoomOrPanCommand = new ActionCommand<object>(
                _ =>
                {
                    if (!ZoomingOrPanningDone)
                    {
                        return;
                    }

                    MonacoPlotModel?.ResetAllAxes();
                    ZoomingOrPanningDone = false;
                    MonacoPlotModel?.InvalidatePlot(true);
                });

            MagnifyMinusCommand = new ActionCommand<object>(
                _ =>
                {
                    Zoom(0.95);

                    CheckForPanningAndZooming();

                    MonacoPlotModel?.InvalidatePlot(false);
                });

            MagnifyPlusCommand = new ActionCommand<object>(
                _ =>
                {
                    Zoom(1.05);

                    MonacoPlotModel?.InvalidatePlot(false);
                });
        }

        public ICommand ResetZoomOrPanCommand { get; }

        public ICommand MagnifyPlusCommand { get; }

        public ICommand MagnifyMinusCommand { get; }

        public List<ViewChartMetric> AllMetrics { get; set; } = new List<ViewChartMetric>();

        public bool MagnifyPlusEnabled
        {
            get => _magnifyPlusEnabled;
            set
            {
                SetProperty(ref _magnifyPlusEnabled, value, nameof(MagnifyPlusEnabled));
                RaisePropertyChanged(nameof(MagnifyPlusEnabled));
            }
        }

        public bool MagnifyMinusEnabled
        {
            get => _magnifyMinusEnabled;
            set
            {
                SetProperty(ref _magnifyMinusEnabled, value, nameof(MagnifyMinusEnabled));
                RaisePropertyChanged(nameof(MagnifyMinusEnabled));
            }
        }

        public bool TextEnabled
        {
            get => _isTextEnabled;
            set
            {
                if (_isTextEnabled == value)
                {
                    return;
                }

                SetProperty(ref _isTextEnabled, value, nameof(TextEnabled));
                RaisePropertyChanged(nameof(TextEnabled));
                MonacoPlotModel?.InvalidatePlot(true);
            }
        }

        public string Text
        {
            get => _text;
            set
            {
                TextEnabled = !string.IsNullOrEmpty(value);

                SetProperty(ref _text, value, nameof(Text));

                RaisePropertyChanged(nameof(Text), nameof(TextEnabled));

                MonacoPlotModel?.InvalidatePlot(true);
            }
        }

        public bool TestMode
        {
            set
            {
                if (value)
                {
                    Initialize();
                }
                else
                {
                    UnInitialize();
                }
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (value == _startDate)
                {
                    return;
                }

                SetProperty(ref _startDate, value, nameof(StartDate));
                RaisePropertyChanged(nameof(StartDate));
                MonacoPlotModel?.InvalidatePlot(true);
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (value == _endDate)
                {
                    return;
                }

                SetProperty(ref _endDate, value, nameof(EndDate));
                RaisePropertyChanged(nameof(EndDate));
                MonacoPlotModel?.InvalidatePlot(true);
            }
        }

        public DateTime EndDateForChart
        {
            get => _endDateForChart;
            set
            {
                SetProperty(ref _endDateForChart, value, nameof(EndDateForChart));

                if (StartDateForChart > EndDateForChart)
                {
                    _endDateForChart = StartDateForChart;
                }

                EndDate = EndDateForChart;
                RaisePropertyChanged(nameof(EndDate), nameof(EndDateForChart));

                MvvmHelper.ExecuteOnUI(
                    () => { IsLoadingChart = true; });

                ZoomingOrPanningDone = false;

                GetPerformanceCountersForDuration(StartDateForChart, EndDateForChart);
                MonacoPlotModel?.InvalidatePlot(true);
            }
        }

        public DateTime StartDateForChart
        {
            get => _startDateForChart;
            set
            {
                SetProperty(ref _startDateForChart, value, nameof(StartDateForChart));
                EndDate = DateTime.Today;

                RaisePropertyChanged(nameof(StartDateForChart), nameof(EndDate));

                if (StartDateForChart > EndDateForChart)
                {
                    _endDateForChart = StartDateForChart;
                    RaisePropertyChanged(nameof(EndDateForChart));
                }

                MvvmHelper.ExecuteOnUI(
                    () => { IsLoadingChart = true; });

                ZoomingOrPanningDone = false;

                GetPerformanceCountersForDuration(StartDateForChart, EndDateForChart);
                MonacoPlotModel?.InvalidatePlot(true);
            }
        }

        public PlotModel MonacoPlotModel
        {
            get => _monacoPlotModel;
            set
            {
                if (value == null)
                {
                    return;
                }

                SetProperty(ref _monacoPlotModel, value, nameof(MonacoPlotModel));
                MonacoPlotModel.InvalidatePlot(true);
            }
        }

        public bool ZoomingOrPanningDone
        {
            get => _isZoomingOrPanningDone;
            set
            {
                SetProperty(ref _isZoomingOrPanningDone, value, nameof(ZoomingOrPanningDone));
                RaisePropertyChanged(nameof(ZoomingOrPanningDone));
            }
        }

        /// <summary>
        ///     Indicates whether or not we are currently loading the chart.
        /// </summary>
        public bool IsLoadingChart
        {
            get => _isLoadingChart;
            set
            {
                SetProperty(ref _isLoadingChart, value, nameof(IsLoadingChart));
                RaisePropertyChanged(nameof(IsLoadingChart));
            }
        }

        protected override void OnUnloaded()
        {
            UnInitialize();
        }

        protected override void DisposeInternal()
        {
            foreach (var metric in AllMetrics)
            {
                metric.PropertyChanged -= ViewMetric_PropertyChanged;
            }

            UnInitialize();

            AllMetrics?.Clear();

            base.DisposeInternal();
        }

        /// <summary>
        ///     This function is used to find the gaps in the counter readings (e.g. when the system was down),
        ///     so that the chart values are not extrapolated for the duration
        ///     the system was down giving a false representation for counter values during that duration.
        /// </summary>
        /// <param name="counterList"></param>
        /// <returns></returns>
        private List<PerformanceCounters> FillOutMissingCounters(List<PerformanceCounters> counterList)
        {
            if (!counterList.Any())
            {
                return counterList;
            }

            var newCounterList = new List<PerformanceCounters> { counterList[0] };

            for (var i = 1; i < counterList.Count; ++i)
            {
                //We need to add values as "0" so that the plot is not extended for the
                //duration when it was not running. 
                var prevDateTime = newCounterList.Last().DateTime;

                // Here we are checking the difference to be 5 * _numberOfElementsToSample
                if (counterList[i].DateTime > prevDateTime + TimeSpan.FromMinutes(5 * _numberOfElementsToSample))
                {
                    newCounterList.Add(new PerformanceCounters { DateTime = prevDateTime.AddMinutes(1) });
                    newCounterList.Add(new PerformanceCounters { DateTime = counterList[i].DateTime.AddMinutes(-1) });
                }

                newCounterList.Add(counterList[i]);
            }

            return newCounterList;
        }

        private IEnumerable<PerformanceCounters> FilteredCountersList(
            IReadOnlyCollection<PerformanceCounters> unfilteredList,
            DateTime? startDateForChart,
            DateTime? endDateForChart)
        {
            if (!unfilteredList.Any())
            {
                return unfilteredList;
            }

            var duration = endDateForChart - startDateForChart;

            if (duration == null)
            {
                return unfilteredList;
            }

            _numberOfElementsToSample = Math.Abs((int)duration.Value.TotalDays) + 1;

            if (_numberOfElementsToSample <= 1)
            {
                return unfilteredList;
            }

            var result = new List<PerformanceCounters>();

            for (var i = 0; i < unfilteredList.Count / _numberOfElementsToSample; i++)
            {
                var valueSamples = new List<double>();

                var countersSample = unfilteredList.Skip(i * _numberOfElementsToSample)
                    .Take(_numberOfElementsToSample).ToList();

                //we get an average of each reading for a collection of values
                for (var j = 0; j < countersSample.Select(counters => counters.Values.Length).Max(); j++)
                {
                    valueSamples.Add(
                        countersSample.Select(counters => j < counters.Values.Length ? counters.Values[j] : 0)
                            .Average());
                }

                result.Add(
                    new PerformanceCounters
                    {
                        DateTime = countersSample.Last().DateTime,
                        Values = valueSamples.ToArray()
                    });
            }

            return result;
        }

        private void CheckForPanningAndZooming()
        {
            var axesCount = MonacoPlotModel?.Axes.Count;

            if (!axesCount.HasValue)
            {
                return;
            }

            // Check if the chart has reached it's minimum display limit, thus disable the buttons.
            var needReset = false;

            for (var i = 0; i < axesCount.Value; ++i)
            {
                if (Math.Abs(
                        MonacoPlotModel.Axes[i].ActualMinimum -
                        MonacoPlotModel.Axes[i].AbsoluteMinimum) > 0 ||
                    Math.Abs(
                        MonacoPlotModel.Axes[i].AbsoluteMaximum -
                        MonacoPlotModel.Axes[i].ActualMaximum) > 0)
                {
                    needReset = true;
                }
            }

            if (needReset || !ZoomingOrPanningDone)
            {
                return;
            }

            ZoomingOrPanningDone = false;
            MagnifyMinusEnabled = false;
        }

        private void PopulateAvailableMetrics()
        {
            // Add all the available metrics
            var metrics = (MetricType[])Enum.GetValues(typeof(MetricType));

            foreach (var metric in metrics)
            {
                var metricLabel = Localizer.For(CultureFor.Operator).GetString(metric.GetAttribute<LabelResourceKeyAttribute>().LabelResourceKey);
                var metricUnitResourceKey = metric.GetAttribute<UnitResourceKeyAttribute>()?.UnitResourceKey;
                var metricUnit = string.IsNullOrWhiteSpace(metricUnitResourceKey)
                    ? metric.GetAttribute<UnitAttribute>().Unit
                    : Localizer.For(CultureFor.Operator).GetString(metricUnitResourceKey);
                var m = new ViewChartMetric
                {
                    InstanceName = metric.GetAttribute<InstanceAttribute>().Instance,
                    MetricType = metric,
                    MetricName = metric.GetAttribute<CounterAttribute>().Counter,
                    Category = metric.GetAttribute<CategoryAttribute>().Category,
                    Unit = metric.GetAttribute<UnitAttribute>().Unit,
                    CounterType = metric.GetAttribute<CounterTypeAttribute>().CounterType,
                    MaxRange = metric.GetAttribute<MaxRangeAttribute>().MaxRange,
                    Label = metricLabel + " " + metricUnit
                };

                m.PropertyChanged += ViewMetric_PropertyChanged;
                AllMetrics.Add(m);
            }
        }

        private void UpdateMetricLabels()
        {
            foreach (var metric in AllMetrics)
            {
                metric.Label = metric.MetricType.GetMetricLabel();
            }
            foreach (var series in MonacoPlotModel.Series)
            {
                var metric = (MetricType)series.Tag;
                series.Title = metric.GetMetricLabel();
            }
            MonacoPlotModel.Title = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PerformanceCountersPlotting);
            RaisePropertyChanged(nameof(AllMetrics));
            RaisePropertyChanged(nameof(MonacoPlotModel));
            MonacoPlotModel.InvalidatePlot(true);
        }

        protected override void OnOperatorCultureChanged(OperatorCultureChangedEvent evt)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                if (MonacoPlotModel == null || AllMetrics == null)
                {
                    return;
                }
                UpdateMetricLabels();
            });

            base.OnOperatorCultureChanged(evt);
        }

        private void ViewMetric_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (!(sender is ViewChartMetric chartMetric))
            {
                return;
            }

            if (args.PropertyName != nameof(chartMetric.LineSeries) &&
                args.PropertyName != nameof(chartMetric.MetricEnabled))
            {
                return;
            }

            if (args.PropertyName == nameof(chartMetric.LineSeries) &&
                chartMetric.LineSeries != null && MonacoPlotModel?.Axes != null &&
                MonacoPlotModel.Axes.Count > 1)
            {
                var maxYAxisValue = AllMetrics.Select(x => x.MaxDataValue).Max();
                if (maxYAxisValue > 0)
                {
                    MonacoPlotModel.Axes[1].AbsoluteMaximum = maxYAxisValue * 1.01;
                    MonacoPlotModel?.Axes[1].Reset();
                }
            }

            MonacoPlotModel?.InvalidatePlot(true);
        }

        private new void Initialize()
        {
            UnInitialize();

            _startDateForChart = DateTime.Today;
            _endDateForChart = DateTime.Today;
            RaisePropertyChanged(nameof(StartDateForChart), nameof(EndDateForChart));
            ZoomingOrPanningDone = false;
            MagnifyMinusEnabled = false;
            GetPerformanceCountersForDuration(StartDateForChart, EndDateForChart);
            // Forces oxyPlot to update
            MonacoPlotModel?.InvalidatePlot(true);
        }
     
        private void UnInitialize()
        {
            _cancellationToken?.Dispose();

            _countersDataList?.Clear();

            foreach (var metric in AllMetrics)
            {
                metric.MetricEnabled = false;
                metric.LineSeries?.Points?.Clear();
            }

            MonacoPlotModel?.Series?.Clear();

            var axisCount = MonacoPlotModel?.Axes?.Count;

            for (var i = 0; i < axisCount; ++i)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                MonacoPlotModel.Axes[i].AxisChanged -= OnAxisChanged;
#pragma warning restore CS0618 // Type or member is obsolete
            }

            MonacoPlotModel?.Axes?.Clear();
        }

        private void Zoom(double factor)
        {
            var axesCount = MonacoPlotModel?.Axes.Count;
            if (!axesCount.HasValue)
            {
                return;
            }

            for (var i = 0; i < axesCount.Value; ++i)
            {
                MonacoPlotModel?.Axes?[i].ZoomAtCenter(factor);
            }
        }

        private void RegisterForAxisChanged()
        {
            var axisCount = MonacoPlotModel.Axes.Count;

            for (var i = 0; i < axisCount; ++i)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                MonacoPlotModel.Axes[i].AxisChanged += OnAxisChanged;
#pragma warning restore CS0618 // Type or member is obsolete

            }
        }

        private void OnAxisChanged(object sender, AxisChangedEventArgs args)
        {
            switch (args.ChangeType)
            {
                case AxisChangeTypes.Pan:
                    ZoomingOrPanningDone = true;
                    break;
                case AxisChangeTypes.Zoom:
                    if (Math.Abs(args.DeltaMaximum) > 0 || Math.Abs(args.DeltaMinimum) > 0)
                    {
                        //Enable zooming if not already true
                        if (!ZoomingOrPanningDone)
                        {
                            ZoomingOrPanningDone = true;
                        }

                        //Enable MagnifyMinus button if not already true
                        if (!MagnifyMinusEnabled)
                        {
                            MagnifyMinusEnabled = true;
                        }
                    }
                    else
                    {
                        CheckForPanningAndZooming();
                    }

                    break;
                case AxisChangeTypes.Reset:
                    //Disable Reset/ZoomOut button
                    if (ZoomingOrPanningDone)
                    {
                        ZoomingOrPanningDone = false;
                    }

                    if (MagnifyMinusEnabled)
                    {
                        MagnifyMinusEnabled = false;
                    }

                    break;
            }
        }

        private void PopulateAxes()
        {
            var dateTimeAxis = new DateTimeAxis
            {
                // Let the chart decide about the minimum, maximum and major step
                // Also by default the zooming and panning is true
                Title = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Time) +
                        $" ({StartDateForChart:dd/MM/y} " +
                        $"- {EndDateForChart:dd/MM/y})",
                TitleFontSize = 20,
                TitleFontWeight = 5,
                TitleColor = OxyColors.CornflowerBlue, // The Color of the Axis Title
                TitlePosition = 0.5, // Middle
                AxisTitleDistance = 10,
                AbsoluteMinimum = Axis.ToDouble(StartDateForChart.AddSeconds(0)),
                AbsoluteMaximum = Axis.ToDouble(EndDateForChart.AddDays(1)),
                MinimumMajorStep = 1.0 / 24 / 60, // 1/24 = 1 hour, 1/24/60 = 1 minute
                Position = AxisPosition.Bottom,
                StringFormat = "dd/MM/y" + Environment.NewLine + "HH:mm:ss",
                MinorIntervalType = DateTimeIntervalType.Auto,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                IsAxisVisible = true,
                TextColor = OxyColors.CornflowerBlue,
                TicklineColor = OxyColors.CornflowerBlue, // The Color of the Ticks
                AxislineColor = OxyColors.CornflowerBlue,
                MajorGridlineColor = OxyColors.CornflowerBlue, // The Color of the Major Grid Lines
                MinorGridlineColor = OxyColors.CornflowerBlue, // The Color of the Minor Grid Lines
                MaximumPadding = 0.03,
                Minimum = Axis.ToDouble(StartDateForChart.AddSeconds(0)),
                Maximum = Axis.ToDouble(EndDateForChart.AddDays(1))
            };

            MonacoPlotModel.Axes.Add(dateTimeAxis);

            var linearAxis = new LinearAxis
            {
                Title = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Value),
                TitleFontSize = 20,
                TitleFontWeight = 5,
                TitleColor = OxyColors.CornflowerBlue, // The Color of the Axis Title
                Position = AxisPosition.Left,
                AxisTitleDistance = 15,
                Minimum = 0,
                IsAxisVisible = true,
                MajorGridlineColor = OxyColors.CornflowerBlue,
                MinorGridlineColor = OxyColors.CornflowerBlue,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                AxislineColor = OxyColors.CornflowerBlue,
                TicklineColor = OxyColors.CornflowerBlue,
                TextColor = OxyColors.CornflowerBlue,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = 100
            };

            MagnifyMinusEnabled = false;

            MonacoPlotModel.Axes.Add(linearAxis);

            RegisterForAxisChanged();

            MonacoPlotModel.InvalidatePlot(true);
        }

        private void PopulateSeries()
        {
            var index = 0;
            foreach (var metric in AllMetrics)
            {
                metric.MetricColor = FillingColors.Colors[index++ % FillingColors.Colors.Count];

                metric.LineSeries =
                    new LineSeries
                    {
                        Title = metric.Label,
                        TrackerFormatString =
                            "{0} " + Environment.NewLine + "Time: {2} " + Environment.NewLine + "{3}: {4} ",
                        Color = metric.MetricColor.ToOxyColor(),
                        MarkerStroke = metric.MetricColor.ToOxyColor(),
                        StrokeThickness = 1,
                        IsVisible = true,
                        Tag = metric.MetricType
                    };

                metric.MetricEnabled = true;
                MonacoPlotModel.Series.Add(metric.LineSeries);
            }

            MonacoPlotModel?.InvalidatePlot(true);
        }

        private void PopulateData(
            IEnumerable<PerformanceCounters> countersDataList,
            DateTime? startDateForChart,
            DateTime? endDateForChart)
        {
            var filteredList = FillOutMissingCounters(
                FilteredCountersList(
                    countersDataList.ToList(),
                    startDateForChart,
                    endDateForChart).ToList());

            foreach (var metric in AllMetrics)
            {
                metric.LineSeries.Points.AddRange(
                    filteredList.Select(
                        counter =>
                            DateTimeAxis.CreateDataPoint(
                                counter.DateTime,
                                counter.CounterDictionary.TryGetValue(metric.MetricType, out var value) ? value : 0)));
                
            }

            //After Population of the data, set the Absolute Maximum Value.
            var maxYAxisValue = AllMetrics.Select(x => x.MaxDataValue).Max();
            if (maxYAxisValue > 0)
            {
                MonacoPlotModel.Axes[1].AbsoluteMaximum = maxYAxisValue * 1.01;
                MonacoPlotModel.Axes[1].Reset();
            }

            MonacoPlotModel?.InvalidatePlot(true);
        }

        private void SetTodayAsEndDate()
        {
            EndDate = DateTime.Today;
            RaisePropertyChanged(nameof(EndDate));
        }

        private void DisableZoomAndResetButtons()
        {
            ZoomingOrPanningDone = false;
            MagnifyMinusEnabled = false;
            MagnifyPlusEnabled = false;
        }

        private void OnException()
        {
            SetTodayAsEndDate();

            MvvmHelper.ExecuteOnUI(
                () => { IsLoadingChart = false; });

            DisableZoomAndResetButtons();

            UnInitialize();
        }

        private async void GetPerformanceCountersForDuration(DateTime startDateForChart, DateTime endDateForChart)
        {
            UnInitialize();

            MonacoPlotModel = new PlotModel
            {
                Title = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PerformanceCountersPlotting),
                TitleFontSize = 25,
                TitleColor = OxyColors.CornflowerBlue,
                IsLegendVisible = true,
                PlotAreaBorderColor = OxyColors.CornflowerBlue,
                PlotMargins = new OxyThickness(60, 10, 20, 70)
            };
            MonacoPlotModel.Legends = new ElementCollection<LegendBase>(MonacoPlotModel)
            {
                new Legend()
                {
                    FontSize = 14,
                    FontWeight = 800,
                    LegendTextColor = OxyColors.White,
                    LegendSymbolLength = 30,
                    LegendSymbolMargin = 10,
                }
            };
            PopulateAxes();

            PopulateSeries();

            _countersDataList.Clear();

            _cancellationToken = new CancellationTokenSource(_timeSpan);

            await FetchChartData(startDateForChart, endDateForChart);

            if (_countersDataList.Any())
            {
                // Make the text blank, indication success for fetching the results.
                Text = string.Empty;

                MagnifyPlusEnabled = true;

                //reset the time stamp value to original value on success
                _timeSpan = TimeSpan.FromMinutes(5);

                PopulateData(_countersDataList, startDateForChart, endDateForChart);

                MonacoPlotModel.InvalidatePlot(true);

                _countersDataList.Clear();
            }
            else
            {
                DisableChartOperations();
            }

            MvvmHelper.ExecuteOnUI(
                () => { IsLoadingChart = false; });

            SetTodayAsEndDate();
        }

        private void DisableChartOperations()
        {
            DisableZoomAndResetButtons();

            foreach (var metric in AllMetrics)
            {
                metric.MetricEnabled = false;
            }

            Text = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorWhileFetchingData);

            foreach (var axis in MonacoPlotModel.Axes)
            {
                axis.IsZoomEnabled = false;
                axis.IsPanEnabled = false;
            }
        }

        private async Task FetchChartData(DateTime startDateForChart, DateTime endDateForChart)
        {
            try
            {
                _countersDataList = await _performanceCounterManager.CountersForParticularDuration(
                    startDateForChart,
                    endDateForChart,
                    _cancellationToken.Token);
            }
            catch (OperationCanceledException)
            {
                // Add 5 minutes for every cancelled exception.
                _timeSpan += TimeSpan.FromMinutes(5);
            }
            catch (Exception ex)
            {
                Logger.Debug($"Got exception {ex.Message}");

                OnException();

                throw;
            }
            finally
            {
                _cancellationToken?.Dispose();
                _cancellationToken = null;
            }
        }
    }
}