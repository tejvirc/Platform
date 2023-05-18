﻿namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using Aristocrat.G2S.Client;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Application.PerformanceCounter;
    using Aristocrat.Monaco.Application.UI.OperatorMenu;
    using Aristocrat.Monaco.Common;
    using Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor;
    using Aristocrat.Monaco.Hardware.Contracts.Printer;
    using Aristocrat.Monaco.Kernel;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;


    /// <summary>
    ///     The view model for the General Information page
    /// </summary>
    [CLSCompliant(false)]
    public class GeneralInformationPageViewModel : OperatorMenuPageViewModelBase
    {
        private IPerformanceCounterManager _performanceCounterManager => ServiceManager.GetInstance().GetService<IPerformanceCounterManager>();
        private INoteAcceptor NoteAcceptor => ServiceManager.GetInstance().TryGetService<INoteAcceptor>();

        public GeneralInformationPageViewModel()
        {
            var hosts = PropertiesManager.GetValues<IHost>(Aristocrat.G2S.Client.Constants.RegisteredHosts);
            G2SHosts = hosts.Select(x=>x.Address).ToList();

            LoadAvailableMetrics();
            RaisePropertyChanged(nameof(G2SHosts));
        }

        public List<Metric> Metrics { get; set; }

        public string AcceptedDenominations => GetFormattedSupportedNotes();

        public string FreeMemoryLabel => Metrics.Where(x => x.MetricType == MetricType.FreeMemory).FirstOrDefault().Label;
        public string FreeMemory => Metrics.Where(x => x.MetricType == MetricType.FreeMemory).FirstOrDefault().CurrentValue.ToString("F");
        public string MonacoPrivateBytesLabel => Metrics.Where(x => x.MetricType == MetricType.MonacoPrivateBytes).FirstOrDefault().Label;
        public string MonacoPrivateBytes => Metrics.Where(x => x.MetricType == MetricType.MonacoPrivateBytes).FirstOrDefault().CurrentValue.ToString("F");
        public string ClrBytesLabel => Metrics.Where(x => x.MetricType == MetricType.ClrBytes).FirstOrDefault().Label;
        public string ClrBytes => Metrics.Where(x => x.MetricType == MetricType.ClrBytes).FirstOrDefault().CurrentValue.ToString("F");

        public IEnumerable<Uri> G2SHosts { get; }

        public string PrinterName => Printer.Name;
        public string PrinterManufacturer => Printer.DeviceConfiguration.Manufacturer;
        public string PrinterModel => Printer.DeviceConfiguration.Model;
        public string PrinterState => Printer.LogicalState.ToString();
        public string PrinterFirmwareVersion => Printer.DeviceConfiguration.FirmwareBootVersion;
        public string PrinterFirmwareRevision => Printer.DeviceConfiguration.FirmwareRevision;
        public string PrinterSerialNumber => Printer.DeviceConfiguration.SerialNumber;
        public string NoteAcceptorName => NoteAcceptor.Name;
        public string NoteAcceptorManufacturer => NoteAcceptor.DeviceConfiguration.Manufacturer;
        public string NoteAcceptorModel => NoteAcceptor.DeviceConfiguration.Model;
        public string NoteAcceptorState => NoteAcceptor.LogicalState.ToString();
        public string NoteAcceptorFirmwareVersion => NoteAcceptor.DeviceConfiguration.FirmwareBootVersion;
        public string NoteAcceptorFirmwareRevision => NoteAcceptor.DeviceConfiguration.FirmwareRevision;
        public string NoteAcceptorSerialNumber => NoteAcceptor.DeviceConfiguration.SerialNumber;

        protected override void InitializeData()
        {
            base.InitializeData();
            GetAllMetricsSnapShot();
        }

        private void LoadAvailableMetrics()
        {
            Metrics = new List<Metric>();
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
                    Label = metric.GetAttribute<LabelAttribute>().Label + " " + metric.GetAttribute<UnitAttribute>().Unit
                };

                Metrics.Add(m);
            }
        }

        private void GetAllMetricsSnapShot()
        {
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
            }
            RaisePropertyChanged(nameof(FreeMemory));
            RaisePropertyChanged(nameof(ClrBytes));
            RaisePropertyChanged(nameof(MonacoPrivateBytes));
        }

        private string GetFormattedSupportedNotes()
        {
            var supportedNotes = NoteAcceptor.GetSupportedNotes();
            var formattedList = supportedNotes.Select(x => x.FormattedCurrencyString());

            return string.Join(", ", formattedList);
        }
    }
}