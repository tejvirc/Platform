namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Windows.Media;
    using Hardware.Contracts.Fan;
    using OperatorMenu;
    using OxyPlot;
    using OxyPlot.Axes;
    using OxyPlot.Wpf;

    /// <summary>
    ///     View model for diagnostic network panel
    /// </summary>
    [CLSCompliant(false)]
    public sealed class DiagnosticCpuFanPageViewModel : OperatorMenuPageViewModelBase
    {
        public List<ViewChartMetric> AllMetrics { get; set; } = new();

        private void Update()
        {
            if (_latestData != null)
            {
                foreach (var series in MonacoPlotModel.Series)
                {
                    var s = series as OxyPlot.Series.LineSeries;
                    var x = s.Points.Count > 0 ? s.Points[s.Points.Count - 1].X + 1 : 0;
                    if (s.Points.Count >= 200)
                    {
                        s.Points.RemoveAt(0);
                    }

                    s.Points.Add(new DataPoint(x, _latestData.FanSpeed));
                }

                MonacoPlotModel?.InvalidatePlot(true);
            }
        }

        private CpuMetricsInfo _latestData;

        public DiagnosticCpuFanPageViewModel(IFan fanService)
        {
            fanService.CpuMetrics.Subscribe(x => this._latestData = x);
            Observable.Interval(TimeSpan.FromMilliseconds(500)).Subscribe(x => Update());

            MonacoPlotModel = new PlotModel();
            AllMetrics.Add(new ViewChartMetric
            {
                MetricName = "Cpu fan Speed",
                MetricColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#ffaacc"),
                MaxRange = 7200,
            });

            foreach (var metric in AllMetrics)
            {
                metric.LineSeries =
                    new OxyPlot.Series.LineSeries
                    {
                        Title = metric.DisplayName,
                        TrackerFormatString = "{0}",
                        Color = metric.MetricColor.ToOxyColor(),
                        MarkerStroke = metric.MetricColor.ToOxyColor(),
                        StrokeThickness = 1,
                        IsVisible = true
                    };

                metric.MetricEnabled = true;
                MonacoPlotModel.Series.Add(metric.LineSeries);
            }

            MonacoPlotModel.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = AxisPosition.Left, Minimum = 0, Maximum = 7200 });
            MonacoPlotModel?.InvalidatePlot(true);
        }

        private PlotModel _monacoPlotModel;
        public PlotModel MonacoPlotModel
        {
            get => _monacoPlotModel;
            set => SetProperty(ref _monacoPlotModel, value);
        }
    }
}