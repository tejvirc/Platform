namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Linq;
    using OxyPlot.Series;

    [CLSCompliant(false)]
    public class ViewChartMetric : Metric
    {
        private bool _metricEnabled;
        private LineSeries _lineSeries;
        private double _maxDataValue;

        public double MaxDataValue
        {
            get
            {
                _maxDataValue = CalculateMaxDateValue();
                return _maxDataValue;
            }
        }

        public new bool MetricEnabled
        {
            get => _metricEnabled;
            set
            {
                if (_metricEnabled == value)
                {
                    return;
                }

                SetProperty(ref _metricEnabled, value, nameof(MetricEnabled));

                if (LineSeries != null)
                {
                    LineSeries.IsVisible = MetricEnabled;
                }

                RaisePropertyChanged(nameof(MetricEnabled), nameof(LineSeries));
            }
        }

        public LineSeries LineSeries
        {
            get => _lineSeries;
            set
            {
                if (value == null)
                {
                    return;
                }

                SetProperty(ref _lineSeries, value, nameof(LineSeries));
                RaisePropertyChanged(nameof(LineSeries));
            }
        }

        private double CalculateMaxDateValue()
        {
            if (LineSeries != null && LineSeries.IsVisible && LineSeries.Points.Count > 0)
            {
                return LineSeries.Points.Max(x => x.Y);
            }

            return 0;
        }
    }
}