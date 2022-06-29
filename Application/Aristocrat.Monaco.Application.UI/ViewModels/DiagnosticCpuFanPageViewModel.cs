namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Windows.Media;
    using Hardware.Contracts.Fan;
    using OperatorMenu;
    using OxyPlot;
    using OxyPlot.Wpf;

    /// <summary>
    ///     View model for diagnostic network panel
    /// </summary>
    [CLSCompliant(false)]
    public sealed class DiagnosticCpuFanPageViewModel : OperatorMenuPageViewModelBase
    {
        public List<ViewChartMetric> AllMetrics { get; set; } = new();

        private void OnTimerElapsed(object state)
        {
            lock (this.MonacoPlotModel.SyncRoot)
            {
                this.Update();
            }

            this.MonacoPlotModel.InvalidatePlot(true);
        }

        private void Update()
        {
            //if (LatestData)
            {
                foreach (var series in MonacoPlotModel.Series)
                {
                    var s = series as OxyPlot.Series.LineSeries;
                    double x = s.Points.Count > 0 ? s.Points[s.Points.Count - 1].X + 1 : 0;
                    if (s.Points.Count >= 200)
                    {
                        s.Points.RemoveAt(0);
                    }

                    s.Points.Add(new DataPoint(x, LatestData.FanSpeed));
                }
            }
        }

        private CpuMetriInfo LatestData;

        public DiagnosticCpuFanPageViewModel(IFan fanService)
        {
            fanService.FanSpeed.Subscribe(x => this.LatestData = x);
            Observable.Interval(TimeSpan.FromMilliseconds(500)).Subscribe(x => Update());

            MonacoPlotModel = new PlotModel();
            AllMetrics.Add(new ViewChartMetric
            {
                MetricName = "Cpu fan Speed",
                MetricColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#ffaacc")
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

            MonacoPlotModel?.InvalidatePlot(true);
        }

        private PlotModel monacoPlotModel;
        public PlotModel MonacoPlotModel { get => monacoPlotModel; set => SetProperty(ref monacoPlotModel, value); }
    }
}