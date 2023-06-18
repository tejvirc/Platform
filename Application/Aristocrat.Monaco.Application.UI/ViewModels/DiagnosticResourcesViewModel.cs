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
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Kernel;
    using LiveCharts;
    using LiveCharts.Configurations;
    using LiveCharts.Wpf;
    using MVVM;
    using MVVM.Command;
    using MVVM.ViewModel;
    using OperatorMenu;
    using Views;
    using Monaco.Common;
    using Monaco.Localization.Properties;
    using PerformanceCounter;
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
            if (!InDesigner)
            {
                _dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
            }

            _performanceCounterManager = ServiceManager.GetInstance().GetService<IPerformanceCounterManager>();

            LoadAvailableMetrics();
            EventBus.Subscribe<OperatorCultureChangedEvent>(this, HandleEvent);
            ViewMemoryCommand = new ActionCommand<object>(ViewMemory);
            ToggleDiagnosticChartViewModeCommand = new ActionCommand<object>(_ => InDiagnosticViewChartMode = !InDiagnosticViewChartMode);
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

            RaisePropertyChanged(nameof(MonacoChart));
            Charts.Add(MonacoChart);
            RaisePropertyChanged(nameof(Charts));
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
                    Label = metric.GetAttribute<LabelAttribute>().Label + " " + metric.GetAttribute<UnitAttribute>().Unit
                };

                Metrics.Add(m);
            }
        }

        private void UpdateMetricLabels()
        {
            if(Metrics == null)
            {
                return;
            }
            foreach (var metric in Metrics)
            {
                var metricLabel = Localizer.For(CultureFor.Operator).GetString(metric.MetricType.GetAttribute<LabelResourceKeyAttribute>().LabelResourceKey);
                var metricUnit = metric.MetricType.GetAttribute<UnitAttribute>().Unit;
                metric.Label = metricLabel+ " " + metricUnit;
            }
            SetYAxesTitles();
            RaisePropertyChanged(nameof(Metrics));
            RaisePropertyChanged(nameof(YAxes));
            RaisePropertyChanged(nameof(MonacoChart));
            RaisePropertyChanged(nameof(Charts));
        }

        private void HandleEvent(OperatorCultureChangedEvent obj)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                UpdateMetricLabels();
            });
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

            RaisePropertyChanged(nameof(YAxes));
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

            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    var axis = new Axis
                    {
                        Title = "Time",
                        LabelFormatter = DateTimeFormatter,
                        Unit = TimeSpan.FromSeconds(1).Ticks
                    };

                    XAxes.Add(axis);
                });
        }

        private void CreateYAxis(int axisNumber, Metric metric)
        {
            MvvmHelper.ExecuteOnUI(
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

            MvvmHelper.ExecuteOnUI(
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
            RaisePropertyChanged(nameof(AxisXStep));

            SetXAxisScale(DateTime.Now);
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
                    MvvmHelper.ExecuteOnUI(
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

                                RaisePropertyChanged(nameof(YAxes));
                                RaisePropertyChanged(nameof(MonacoChart));
                                RaisePropertyChanged(nameof(Charts));
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

            RaisePropertyChanged(nameof(Series));
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
            MvvmHelper.ExecuteOnUI(
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

                    RaisePropertyChanged(nameof(YAxes));

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
    public class Metric : BaseViewModel
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
                RaisePropertyChanged(nameof(MetricName));
            }
        }

        public MetricType MetricType
        {
            get => _metricType;
            set
            {
                _metricType = value;
                RaisePropertyChanged(nameof(MetricType));
            }
        }

        public string InstanceName
        {
            get => _instanceName;
            set
            {
                _instanceName = value;
                RaisePropertyChanged(nameof(InstanceName));
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                RaisePropertyChanged(nameof(Category));
            }
        }

        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                RaisePropertyChanged(nameof(Label));
            }
        }

        public string Unit
        {
            get => _unit;
            set
            {
                _unit = value;
                RaisePropertyChanged(nameof(Unit));
            }
        }

        public int MaxRange
        {
            get => _maxRange;
            set
            {
                _maxRange = value;
                RaisePropertyChanged(nameof(MaxRange));
            }
        }

        public string CounterType
        {
            get => _counterType;
            set
            {
                _counterType = value;
                RaisePropertyChanged(nameof(CounterType));
            }
        }

        public double CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = value;
                RaisePropertyChanged(nameof(CurrentValue));
            }
        }

        public Brush MetricColor
        {
            get => _metricColor;
            set
            {
                _metricColor = value;
                RaisePropertyChanged(nameof(MetricColor));
            }
        }

        public bool MetricEnabled
        {
            get => _metricEnabled;
            set
            {
                _metricEnabled = value;
                RaisePropertyChanged(nameof(MetricEnabled));
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
                RaisePropertyChanged(nameof(Index));
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
