namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Application.Helpers;
    using Contracts;
    using Contracts.Extensions;
    using Contracts.Localization;
    using Hardware.Contracts.NoteAcceptor;
    using Helpers;
    using Kernel;
    using Monaco.Common;
    using Monaco.Localization.Properties;
    using Newtonsoft.Json.Linq;
    using OperatorMenu;
    using PerformanceCounter;

    /// <summary>
    ///     The view model for the General Information page
    /// </summary>
    [CLSCompliant(false)]
    public class GeneralInformationPageViewModel : OperatorMenuPageViewModelBase
    {
        private static IPerformanceCounterManager PerformanceCounterManager => ServiceManager.GetInstance().GetService<IPerformanceCounterManager>();

        private static INoteAcceptor NoteAcceptor => ServiceManager.GetInstance().TryGetService<INoteAcceptor>();

        private const string Addresses = "addresses";

        public GeneralInformationPageViewModel()
        {
            var hosts = new List<Uri>();
            var hostAddresses = (string)PropertiesManager.GetProperty(ApplicationConstants.HostAddresses, string.Empty);

            if (!string.IsNullOrEmpty(hostAddresses))
            {
                var result = JObject.Parse(hostAddresses);
                if (result?.HasValues ?? false)
                {
                    result.GetValue(Addresses)?.Values<string>()?.ToList().ForEach(a => hosts.Add(new Uri(a)));
                }
            }

            G2SHosts = hosts;

            LoadAvailableMetrics();
        }

        public List<Metric> Metrics { get; set; } = new List<Metric>();

        public string AcceptedDenominations => GetFormattedSupportedNotes();

        public string MegabytesLabel => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MegabytesAbbr);

        public string FreeMemoryLabel => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.FreeMemoryLabel) + " " + MegabytesLabel;

        public string FreeMemory => Metrics.FirstOrDefault(x => x.MetricType == MetricType.FreeMemory).CurrentValue.ToFormattedString();

        public string MonacoPrivateBytesLabel => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PlatformMemoryLabel) + " " + MegabytesLabel;

        public string MonacoPrivateBytes => Metrics.FirstOrDefault(x => x.MetricType == MetricType.MonacoPrivateBytes).CurrentValue.ToFormattedString();

        public string ClrBytesLabel => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClrMemoryLabel) + " " + MegabytesLabel;

        public string ClrBytes => Metrics.FirstOrDefault(x => x.MetricType == MetricType.ClrBytes).CurrentValue.ToFormattedString();

        public IEnumerable<Uri> G2SHosts { get; }

        public string PrinterName => Printer.Name;

        public string PrinterManufacturer => Printer.DeviceConfiguration.Manufacturer;

        public string PrinterModel => Printer.DeviceConfiguration.Model;

        public string PrinterState => Printer.LogicalState.StateToString();

        public string PrinterFirmwareVersion => Printer.DeviceConfiguration.FirmwareId;

        public string PrinterFirmwareRevision => Printer.DeviceConfiguration.FirmwareRevision;

        public string PrinterSerialNumber => Printer.DeviceConfiguration.SerialNumber;

        public string NoteAcceptorName => NoteAcceptor.Name;

        public string NoteAcceptorManufacturer => NoteAcceptor.DeviceConfiguration.Manufacturer;

        public string NoteAcceptorModel => NoteAcceptor.DeviceConfiguration.Model;

        public string NoteAcceptorState => NoteAcceptor.LogicalState.StateToString(NoteAcceptor.WasStackingOnLastPowerUp);

        public string NoteAcceptorFirmwareVersion => NoteAcceptor.DeviceConfiguration.FirmwareId;

        public string NoteAcceptorFirmwareRevision => NoteAcceptor.DeviceConfiguration.FirmwareRevision;

        public string NoteAcceptorSerialNumber => NoteAcceptor.DeviceConfiguration.SerialNumber;

        protected override void InitializeData()
        {
            GetAllMetricsSnapShot();
        }

        protected override void OnLoaded()
        {
            RaisePropertyChanged(nameof(G2SHosts));
        }

        protected override void OnOperatorCultureChanged(OperatorCultureChangedEvent evt)
        {
            RaisePropChangedForAll();
            base.OnOperatorCultureChanged(evt);
        }

        private void RaisePropChangedForAll()
        {
            RaisePropertyChanged(nameof(AcceptedDenominations));
            RaisePropertyChanged(nameof(MegabytesLabel));
            RaisePropertyChanged(nameof(FreeMemoryLabel));
            RaisePropertyChanged(nameof(FreeMemory));
            RaisePropertyChanged(nameof(MonacoPrivateBytesLabel));
            RaisePropertyChanged(nameof(MonacoPrivateBytes));
            RaisePropertyChanged(nameof(ClrBytesLabel));
            RaisePropertyChanged(nameof(ClrBytes));
            RaisePropertyChanged(nameof(G2SHosts));
            RaisePropertyChanged(nameof(PrinterName));
            RaisePropertyChanged(nameof(PrinterManufacturer));
            RaisePropertyChanged(nameof(PrinterModel));
            RaisePropertyChanged(nameof(PrinterState));
            RaisePropertyChanged(nameof(PrinterFirmwareVersion));
            RaisePropertyChanged(nameof(PrinterFirmwareRevision));
            RaisePropertyChanged(nameof(PrinterSerialNumber));
            RaisePropertyChanged(nameof(NoteAcceptorName));
            RaisePropertyChanged(nameof(NoteAcceptorManufacturer));
            RaisePropertyChanged(nameof(NoteAcceptorModel));
            RaisePropertyChanged(nameof(NoteAcceptorState));
            RaisePropertyChanged(nameof(NoteAcceptorFirmwareVersion));
            RaisePropertyChanged(nameof(NoteAcceptorFirmwareRevision));
            RaisePropertyChanged(nameof(NoteAcceptorSerialNumber));
        }

        private void LoadAvailableMetrics()
        {
            var metrics = (MetricType[])Enum.GetValues(typeof(MetricType));

            foreach (var metric in metrics)
            {
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

        private void GetAllMetricsSnapShot()
        {
            var perfData = PerformanceCounterManager.CurrentPerformanceCounter;
            foreach (var metric in Metrics)
            {
                var gotValue = perfData.CounterDictionary.TryGetValue(metric.MetricType, out var value);
                var newData = new MeasureModel
                {
                    MetricName = metric.MetricName,
                    InstanceName = metric.InstanceName,
                    DateTime = perfData.DateTime,
                    Value = gotValue ? value : 0
                };

                metric.CurrentValue = newData.Value; // grab current value for the legend
            }

            RaisePropertyChanged(nameof(FreeMemory));
            RaisePropertyChanged(nameof(ClrBytes));
            RaisePropertyChanged(nameof(MonacoPrivateBytes));
        }

        private string GetFormattedSupportedNotes()
        {
            var culture = GetCurrencyDisplayCulture();
            var supportedNotes = NoteAcceptor.GetSupportedNotes();
            var formattedList = supportedNotes.Select(x => x.FormattedCurrencyString(null, culture));

            return string.Join($"{culture.TextInfo.ListSeparator} ", formattedList);
        }
    }
}
