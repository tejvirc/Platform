namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Timers;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using Application.Helpers;
    using Aristocrat.Extensions.CommunityToolkit;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Kernel;
    using LiveCharts;
    using LiveCharts.Configurations;
    using LiveCharts.Wpf;
    using Monaco.Common;
    using Monaco.Localization.Properties;
    using OperatorMenu;
    using PerformanceCounter;
    using Views;
    using Brushes = System.Windows.Media.Brushes;
    using Timer = System.Timers.Timer;
    using UIElement = System.Windows.UIElement;

    /// <summary>
    ///     The view model for the diagnostics "resources" page, the one that shows the live values
    ///     of the platform performance counters
    /// </summary>
    [CLSCompliant(false)]
    public class DiagnosticResourcesViewModel : OperatorMenuPageViewModelBase
    {
        private const string RuntimeInstanceName = "GDKRuntimeHost";
        private readonly IDialogService _dialogService;
        private const int PollingTimeoutMilliSeconds = 1000;
        private const int MaxProcessingRecords = 25;
        private const double NumberSteps = 10; // same range:step ratio for all measurements, so chart grid lines are uniform

        private Timer _pollingTimer;
        private readonly List<Brush> _colors = FillingColors.Colors;

        private bool _inTestMode;

        private readonly IPerformanceCounterManager _performanceCounterManager;

        public DiagnosticResourcesViewModel()
        {
            if (!Execute.InDesigner)
            {
                _dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
            }

            _performanceCounterManager = ServiceManager.GetInstance().GetService<IPerformanceCounterManager>();

            LoadAvailableMetrics();

            ViewMemoryCommand = new RelayCommand<object>(ViewMemory);
            ToggleDiagnosticChartViewModeCommand = new RelayCommand<object>(_ => InDiagnosticViewChartMode = !InDiagnosticViewChartMode);
        }

        private static Process GdkProcess => Process.GetProcessesByName(RuntimeInstanceName).FirstOrDefault();

        public SeriesCollection Series { get; set; }
        public List<Metric> Metrics { get; set; }
        public AxesCollection YAxes { get; set; } = new AxesCollection();
        public AxesCollection XAxes { get; set; } = new AxesCollection();

        public Func<double, string> DateTimeFormatter { get; set; }
        public Func<double, string> PercentageValueFormatter { get; set; }
        public Func<double, string> BytesValueFormatter { get; set; }
        public Func<double, string> NumberValueFormatter { get; set; }

        public ObservableCollection<UIElement> Charts { get; set; } = new ObservableCollection<UIElement>();

        public CartesianChart MonacoChart { get; set; } // main binding object for the view

        public double AxisXStep { get; set; }

        public ICommand ViewMemoryCommand { get; }

        public ICommand ToggleDiagnosticChartViewModeCommand { get; }

        public DiagnosticPerformanceCounterChartViewModel TestDiagnosticPerformanceCounterChartViewModel { get; } = new DiagnosticPerformanceCounterChartViewModel();

        public bool InDiagnosticViewChartMode
        {
            get => _inTestMode;
            set
            {
                TestDiagnosticPerformanceCounterChartViewModel.TestMode = value;
                if (value)
                {
                    StopTimer();
                }
                else
                {
                    StartTimer();
                }

                SetProperty(ref _inTestMode, value, nameof(InDiagnosticViewChartMode));
            }
        }

        protected override void OnLoaded()
        {
            InitializeChart();

            InitializeTimer();

            GetAllMetricsSnapShot();

            Execute.OnUIThread(UpdateMetricLabels);

            SetXAxisScale(DateTime.Now);

            StartTimer();
        }

        private void InitializeChart()
        {
            var mapper = Mappers.Xy<MeasureModel>()
                .X(model => model.DateTime.Ticks) //use DateTime.Ticks as X
                .Y(model => model.Value); //use the value property as Y

            ClearData();

            Series = new SeriesCollection();
            //lets save the mapper globally.
            Charting.For<MeasureModel>(mapper);

            LoadMetricSources();

            CreateXAxis();

            SetYAxesFormatters();

            SetYAxesTitles();

            EventBus.Subscribe<ChangeChartSeriesVisibilityEvent>(this, OnMetricEnabledCheckedCommand);

            MonacoChart = new CartesianChart
            {
                Name = "MonacoChart",
                Series = Series,
                AnimationsSpeed = new TimeSpan(0, 0, 2),
                Hoverable = false,
                DataTooltip = null,
                AxisY = YAxes,
                AxisX = XAxes
            };

            OnPropertyChanged(nameof(MonacoChart));
            Charts.Add(MonacoChart);
            OnPropertyChanged(nameof(Charts));
        }

        private void LoadAvailableMetrics()
        {
            Metrics = new List<Metric>();
            var metrics = (MetricType[])Enum.GetValues(typeof(MetricType));

            foreach (var metric in metrics)
            {
                // if Gdk is not up do not create metrics/counters
                if (metric.GetAttribute<InstanceAttribute>().Instance == RuntimeInstanceName && GdkProcess == null)
                {
                    continue;
                }
                var metricLabel = Localizer.For(CultureFor.Operator).GetString(metric.GetAttribute<LabelResourceKeyAttribute>().LabelResourceKey);
                var metricUnitResourceKey = metric.GetAttribute<UnitResourceKeyAttribute>()?.UnitResourceKey;
                var metricUnit = string.IsNullOrWhiteSpace(metricUnitResourceKey)
                    ? metric.GetAttribute<UnitAttribute>().Unit
                    : Localizer.For(CultureFor.Operator).GetString(metricUnitResourceKey);

                var m = new Metric
                {
                    InstanceName = metric.GetAttribute<InstanceAttribute>().Instance,
                    MetricType = metric,
                    MetricName = metric.GetAttribute<CounterAttribute>().Counter,
                    Category = metric.GetAttribute<CategoryAttribute>().Category,
                    MetricEnabled = true,
                    Unit = metric.GetAttribute<UnitAttribute>().Unit,
                    CounterType = metric.GetAttribute<CounterTypeAttribute>().CounterType,
                    MaxRange = metric.GetAttribute<MaxRangeAttribute>().MaxRange,
                    Label = metric.GetMetricLabel()
                };

                Metrics.Add(m);
            }
        }

        private void UpdateMetricLabels()
        {
            if (Metrics == null)
            {
                return;
            }
            foreach (var metric in Metrics)
            {
                var metricLabel = Localizer.For(CultureFor.Operator).GetString(metric.MetricType.GetAttribute<LabelResourceKeyAttribute>().LabelResourceKey);
                var metricUnit = metric.MetricType.GetAttribute<UnitAttribute>().Unit;
                metric.Label = metricLabel + " " + metricUnit;
            }
            SetXAxesTitles();
            SetYAxesTitles();
            OnPropertyChanged(nameof(Metrics));
            OnPropertyChanged(nameof(YAxes));
            OnPropertyChanged(nameof(XAxes));
            OnPropertyChanged(nameof(MonacoChart));
            OnPropertyChanged(nameof(Charts));
        }

        protected override void OnOperatorCultureChanged(OperatorCultureChangedEvent evt)
        {
            Execute.OnUIThread(UpdateMetricLabels);
            base.OnOperatorCultureChanged(evt);
        }

        private void LoadMetricSources()
        {
            var seriesNumber = 0;
            var axisNumber = 0;
            Series.Clear(); // for the chart

            foreach (var metric in Metrics)
            {
                metric.Index = seriesNumber;
                metric.MetricColor = _colors.Count > 0 ? _colors[seriesNumber % _colors.Count] : Brushes.White;
                var source = new ChartValues<MeasureModel>();

                CreateLineSeries(source, metric, axisNumber);
                CreateYAxis(axisNumber, metric);
                seriesNumber++;
                axisNumber = seriesNumber;
            }

            OnPropertyChanged(nameof(YAxes));
        }

        private void CreateLineSeries(ChartValues<MeasureModel> source, Metric metric, int axisNumber)
        {
            var line = new LineSeries
            {
                Values = source,
                IsEnabled = true,
                Title = metric.Label,
                Stroke = metric.MetricColor,
                LineSmoothness = 0,
                PointGeometry = null,
                StrokeThickness = 4,
                Fill = Brushes.Transparent,
                Visibility = Visibility.Visible,
                ScalesYAt = axisNumber
            };

            Series?.Add(line);
        }

        private void CreateXAxis()
        {
            //lets set how to display the axis Labels
            DateTimeFormatter = value => new DateTime((long)value).ToString("hh:mm:ss");

            Execute.OnUIThread(
                () =>
                {
                    var axis = new Axis
                    {
                        Title = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Time),
                        LabelFormatter = DateTimeFormatter,
                        Unit = TimeSpan.FromSeconds(1).Ticks
                    };

                    XAxes.Add(axis);
                });
        }

        private void CreateYAxis(int axisNumber, Metric metric)
        {
            Execute.OnUIThread(
                () =>
                {
                    var axis = CreateYAxisFromMetric(axisNumber, metric);
                    YAxes.Add(axis);
                });
        }

        private Axis CreateYAxisFromMetric(int axisNumber, Metric metric)
        {
            return new Axis
            {
                Title = metric.MetricName + " " + metric.Unit,
                Foreground = metric.MetricColor,
                Position = axisNumber % 2 == 0 ? AxisPosition.LeftBottom : AxisPosition.RightTop,
                MaxValue = metric.MaxRange,
                MinValue = 0,
                Unit = metric.MaxRange / NumberSteps,
                LabelFormatter = metric.CounterType == "Memory" ? BytesValueFormatter :
                    metric.CounterType == "CPU" ? PercentageValueFormatter : NumberValueFormatter
            };
        }

        private void SetXAxisScale(DateTime now)
        {
            if (XAxes == null || YAxes == null)
            {
                return;
            }

            Execute.OnUIThread(
                () =>
                {
                    if (XAxes.Count > 0)
                    {
                        XAxes[0].MaxValue =
                            now.Ticks + TimeSpan.FromSeconds(0).Ticks; // lets force the axis to be 0 second ahead
                    }

                    if (YAxes.Count > 0)
                    {
                        XAxes[0].MinValue = now.Ticks - TimeSpan.FromSeconds(8).Ticks; // and 8 seconds behind
                    }
                });
        }

        private void SetYAxesFormatters()
        {
            PercentageValueFormatter = value => (value / 100).ToString("P");
            BytesValueFormatter = value => value.ToString("N");
            NumberValueFormatter = value => value.ToString("N");

            //AxisStep forces the distance between each separator in the X axis
            AxisXStep = TimeSpan.FromSeconds(1).Ticks;
            OnPropertyChanged(nameof(AxisXStep));

            SetXAxisScale(DateTime.Now);
        }

        private void SetXAxesTitles()
        {
            foreach (var x in XAxes)
            {
                x.Title = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Time);
            }
        }

        private void SetYAxesTitles()
        {
            for (int index = 0; index < Metrics.Count; index++)
            {
                var axis = YAxes[index];
                axis.Title = Metrics[index].Label;
            }
        }

        private void GetAllMetricsSnapShot()
        {
            var data = new Dictionary<int, MeasureModel>();
            var perfData = _performanceCounterManager.CurrentPerformanceCounter;
            foreach (var metric in Metrics)
            {
                bool gotValue = perfData.CounterDictionary.TryGetValue(metric.MetricType, out var value);
                var newData = new MeasureModel
                {
                    MetricName = metric.MetricName,
                    InstanceName = metric.InstanceName,
                    DateTime = perfData.DateTime,
                    Value = gotValue ? value : 0
                };

                metric.CurrentValue = newData.Value; // grab current value for the legend
                data.Add(metric.Index, newData);

                if (metric.CurrentValue > metric.MaxRange && metric.MaxRange > 0)
                {
                    Execute.OnUIThread(
                        () =>
                        {
                            var yAxis = YAxes.FirstOrDefault(y => y.Title.StartsWith(metric.MetricName));
                            var index = YAxes.IndexOf(yAxis);

                            if (yAxis != null)
                            {
                                // If the value is too big, increase the scale by 50%.
                                metric.MaxRange = (int)Math.Ceiling(metric.MaxRange * 1.5);

                                var newAxis = CreateYAxisFromMetric(index, metric);
                                YAxes[index] = newAxis;

                                OnPropertyChanged(nameof(YAxes));
                                OnPropertyChanged(nameof(MonacoChart));
                                OnPropertyChanged(nameof(Charts));
                            }
                        });
                }
            }

            UpdateSeries(data);
        }

        private void UpdateSeries(Dictionary<int, MeasureModel> data)
        {
            if (Series == null || Series.Count == 0)
            {
                return;
            }

            foreach (var measure in data)
            {
                Series[measure.Key].Values.Add(measure.Value); // update the series collection to update the chart

                if (Series[measure.Key].Values.Count > MaxProcessingRecords)
                {
                    Series[measure.Key].Values.RemoveAt(0); // remove oldest data point
                }
            }

            OnPropertyChanged(nameof(Series));
        }

        protected override void OnUnloaded()
        {
            InDiagnosticViewChartMode = false;
            StopTimer();
            ReleaseTimer();
            base.OnUnloaded();
        }

        private void OnPollingUpdate(object sender, ElapsedEventArgs e)
        {
            GetAllMetricsSnapShot();
            SetXAxisScale(DateTime.Now);
        }

        private void InitializeTimer()
        {
            _pollingTimer = new Timer();
            _pollingTimer.Elapsed += OnPollingUpdate;
            _pollingTimer.Interval = PollingTimeoutMilliSeconds;
            _pollingTimer.AutoReset = true;
            _pollingTimer.Enabled = true;
        }

        private void StartTimer()
        {
            _pollingTimer?.Start();
        }

        private void StopTimer()
        {
            _pollingTimer?.Stop();
        }

        private void ReleaseTimer()
        {
            if (_pollingTimer == null)
            {
                return;
            }

            _pollingTimer.Elapsed -= OnPollingUpdate;
            _pollingTimer.Dispose();
            _pollingTimer = null;
        }

        protected override void DisposeInternal()
        {
            StopTimer();
            ReleaseTimer();

            base.DisposeInternal();
        }

        private void ClearData()
        {
            foreach (var metric in Metrics) // reset the enable flags
            {
                metric.MetricEnabled = true;
            }

            YAxes?.Clear();
            XAxes?.Clear();
        }

        private void OnMetricEnabledCheckedCommand(ChangeChartSeriesVisibilityEvent evt)
        {
            Execute.OnUIThread(
                () =>
                {
                    var line = (LineSeries)Series[evt
                        .SeriesIndex]; // get the LineSeries by index in the SeriesCollection

                    line.Visibility = line.Visibility == Visibility.Visible
                        ? Visibility.Hidden
                        : Visibility.Visible;

                    if (line.Visibility == Visibility.Hidden)
                    {
                        line.Values.Clear(); // prevent the back filling and snake-like render
                    }

                    var axis = YAxes[evt.SeriesIndex];
                    axis.ShowLabels = !axis.ShowLabels;
                    axis.Separator.IsEnabled = !axis.Separator.IsEnabled;
                    if (!axis.ShowLabels)
                    {
                        axis.Title = string.Empty;
                    }
                    else
                    {
                        axis.Title = Metrics[evt.SeriesIndex].Label + " " + Metrics[evt.SeriesIndex].Unit;
                    }

                    OnPropertyChanged(nameof(YAxes));

                    MonacoChart.Update();
                });
        }

        private void ViewMemory(object obj) // pops the view memory dialog
        {
            var viewModel = new DiagnosticViewMemoryViewModel();

            _dialogService.ShowInfoDialog<DiagnosticViewMemoryView>(
                this,
                viewModel,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SystemMemory));
        }

    }

    public class MeasureModel
    {
        public string InstanceName { get; set; }
        public string MetricName { get; set; }
        public DateTime DateTime { get; set; }
        public double Value { get; set; }
    }

    [CLSCompliant(false)]
    public class Metric : ObservableObject
    {
        private string _metricName;
        private double _currentValue;
        private Brush _metricColor;
        private bool _metricEnabled;
        private MetricType _metricType;
        private string _instanceName;
        private string _category;
        private string _label;
        private string _unit;
        private int _maxRange;
        private string _counterType;
        private int _index;

        public string MetricName
        {
            get => _metricName;
            set
            {
                _metricName = value;
                OnPropertyChanged(nameof(MetricName));
            }
        }

        public MetricType MetricType
        {
            get => _metricType;
            set
            {
                _metricType = value;
                OnPropertyChanged(nameof(MetricType));
            }
        }

        public string InstanceName
        {
            get => _instanceName;
            set
            {
                _instanceName = value;
                OnPropertyChanged(nameof(InstanceName));
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }

        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                OnPropertyChanged(nameof(Label));
            }
        }

        public string Unit
        {
            get => _unit;
            set
            {
                _unit = value;
                OnPropertyChanged(nameof(Unit));
            }
        }

        public int MaxRange
        {
            get => _maxRange;
            set
            {
                _maxRange = value;
                OnPropertyChanged(nameof(MaxRange));
            }
        }

        public string CounterType
        {
            get => _counterType;
            set
            {
                _counterType = value;
                OnPropertyChanged(nameof(CounterType));
            }
        }

        public double CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = value;
                OnPropertyChanged(nameof(CurrentValue));
            }
        }

        public Brush MetricColor
        {
            get => _metricColor;
            set
            {
                _metricColor = value;
                OnPropertyChanged(nameof(MetricColor));
            }
        }

        public bool MetricEnabled
        {
            get => _metricEnabled;
            set
            {
                _metricEnabled = value;
                OnPropertyChanged(nameof(MetricEnabled));
                ServiceManager.GetInstance().GetService<IEventBus>().Publish(new ChangeChartSeriesVisibilityEvent(Index));
            }
        }

        // series number
        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                OnPropertyChanged(nameof(Index));
            }
        }
    }

    public static class FillingColors
    {
        public static readonly List<Brush> Colors = new List<Brush>
        {
            Brushes.Crimson,
            Brushes.DodgerBlue,
            Brushes.LimeGreen,
            Brushes.Magenta,
            Brushes.DarkOrange,
            Brushes.Gold,
            Brushes.LightCoral,
            Brushes.LightGreen,
            Brushes.LightPink,
            Brushes.LightGray,
            Brushes.LightBlue,
            Brushes.Yellow
        };
    }

    //public static class EnumHelper
    //{
    //    public static T StringToEnum<T>(string name)
    //    {
    //        return (T)Enum.Parse(typeof(T), name);
    //    }

    //    public static TAttribute GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
    //    {
    //        var enumType = value.GetType();
    //        var name = Enum.GetName(enumType, value);
    //        return enumType.GetField(name).GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();
    //    }
    //}
}
