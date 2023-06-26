﻿namespace Aristocrat.Monaco.Application.UI.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Contracts;
    using Contracts.Localization;
    using Contracts.MeterPage;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using MeterPage;
    using Monaco.UI.Common.Extensions;
    using MVVM;

    /// <summary>
    ///     A MetersPageViewModelBase contains the base logic for meters page view models
    /// </summary>
    /// <seealso cref="OperatorMenuPageViewModelBase" />
    [CLSCompliant(false)]
    public abstract class MetersPageViewModelBase : OperatorMenuPageViewModelBase
    {
        public List<MeterNode> MeterNodes { get; private set; } = new List<MeterNode>();

        // DisplayMeter.Order values over 100 will display in the 2nd column of the view
        protected const int LeftColumnMaximumMeterOrder = 100;

        private const string MetersExtensionPath = "/Application/OperatorMenu/DisplayMeters";
        private readonly MeterNodePage? _meterNodePage;
        private bool _showLifetime = true;

        /// <summary>
        ///     The base ViewModel class constructor for all Meters pages
        /// </summary>
        /// <param name="meterNodePage">The type of display meter, or null where controlled elsewhere</param>
        /// <param name="disablePrintButton">Whether to disable the print button without checking config property</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected MetersPageViewModelBase(MeterNodePage? meterNodePage, bool disablePrintButton = false)
            : base(!disablePrintButton)
        {
            _meterNodePage = meterNodePage;
            LoadMetersToDisplay();
        }

        public ObservableCollection<DisplayMeter> Meters { get; } = new ObservableCollection<DisplayMeter>();

        public bool ShowLifetime
        {
            get => _showLifetime;
            set
            {
                if (_showLifetime == value)
                {
                    return;
                }

                _showLifetime = value;
                UpdateMeters();
            }
        }

        public void PeriodOrMasterButtonClicked(bool showLifetime)
        {
            ShowLifetime = showLifetime;
        }

        public ObservableCollection<DisplayMeter> MetersLeftColumn { get; } = new ObservableCollection<DisplayMeter>();

        public ObservableCollection<DisplayMeter> MetersRightColumn { get; } = new ObservableCollection<DisplayMeter>();

        /// <summary>
        ///     Determines if we should show the right column or not.
        /// </summary>
        public bool ShowRightColumn => MetersRightColumn.Count > 0;

        private void LoadMetersToDisplay()
        {
            if (_meterNodePage.HasValue)
            {
                var configMeters = ConfigurationUtilities.GetConfiguration(MetersExtensionPath,
                    () => new DisplayMetersConfiguration
                    {
                        MeterNodes = new MeterNode[0]
                    });
                var pageMeters = configMeters.MeterNodes.Where(m => m.Page == _meterNodePage.Value);
                foreach (var node in pageMeters)
                {
                    Logger.DebugFormat("Adding meter: {0}", node.Name);
                    MeterNodes.Add(node);
                }

                MeterNodes = MeterNodes.OrderBy(n => n.Order).ToList();
            }

            InitializeMeters();
        }

        protected override void OnLoaded()
        {
            EventBus.Subscribe<PeriodOrMasterButtonClickedEvent>(this, evt => PeriodOrMasterButtonClicked(evt.MasterClicked));
            EventBus.Subscribe<OperatorCultureChangedEvent>(this, HandleOperatorCultureChanged);
            // VLT-12225
            // Fires an event fired when a specific meter page is loaded (switching tabs) so we can
            // synchronize the ShowLifetime and Master/period button status
            EventBus.Publish(new MeterPageLoadedEvent());
            RefreshMeters();
        }

        protected virtual void HandleOperatorCultureChanged(OperatorCultureChangedEvent @event)
        {
            RefreshMeters();
        }

        protected virtual void InitializeMeters()
        {
            // This should only occur the first time the view model is created
            var meterManager = ServiceManager.GetInstance().GetService<IMeterManager>();
            foreach (var meterNode in MeterNodes)
            {
                if (meterManager.IsMeterProvided(meterNode.Name))
                {
                    var meter = meterManager.GetMeter(meterNode.Name);
                    string meterDisplayName = Localizer.For(CultureFor.Operator).GetString(
                            meterNode.DisplayNameKey,
                            _ => meterDisplayName = meterNode.DisplayName);

                    Meters.Add(
                        new DisplayMeter(
                            meterDisplayName,
                            meter,
                            ShowLifetime,
                            meterNode.Order,
                            meterNode.Period,
                            false,
                            UseOperatorCultureForCurrencyFormatting));
                }
                else
                {
                    if (meterNode.ShowNotApplicable)
                    {
                        Meters.Add(
                            new DisplayMeter(
                                meterNode.DisplayName,
                                null,
                                ShowLifetime,
                                meterNode.Order,
                                meterNode.Period,
                                meterNode.ShowNotApplicable,
                                UseOperatorCultureForCurrencyFormatting));
                    }

                    if (meterNode.DisplayName.IsEmpty() && meterNode.Name == "blank line")
                    {
                        var lifetime = new BlankDisplayMeter(ShowLifetime, meterNode.Order);
                        Meters.Add(lifetime);
                    }

                    Logger.ErrorFormat("Meter not found: {0}", meterNode.Name);
                }
            }
        }

        protected virtual void UpdateMeters()
        {
            foreach (var meter in Meters.ToList())
            {
                meter.ShowLifetime = ShowLifetime;
                meter.OnMeterChangedEvent();
            }
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            if (dataType != OperatorMenuPrintData.Main)
            {
                return null;
            }

            var ticketCreator = ServiceManager.GetInstance().TryGetService<IMetersTicketCreator>();
            var printMeters = Meters.Where(m => m.Meter != null).Select(m => new Tuple<IMeter, string>(m.Meter, m.Name))
                .ToList();

            return TicketToList(ticketCreator?.CreateEgmMetersTicket(printMeters, ShowLifetime));
        }

        protected override void DisposeInternal()
        {
            foreach (var meter in Meters)
            {
                meter.Dispose();
            }

            Meters.Clear();

            base.DisposeInternal();
        }

        protected virtual void SplitMeters()
        {
            MetersLeftColumn.Clear();
            MetersRightColumn.Clear();

            foreach (var meter in Meters)
            {
                if (meter.Order <= LeftColumnMaximumMeterOrder)
                {
                    MetersLeftColumn.Add(meter);
                }
                else
                {
                    MetersRightColumn.Add(meter);
                }
            }

            RaisePropertyChanged(nameof(ShowRightColumn));
        }

        protected virtual void RefreshMeters()
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                foreach (var meter in Meters)
                {
                    meter.Dispose();
                }

                Meters.Clear();
                MetersLeftColumn.Clear();
                MetersRightColumn.Clear();
                InitializeMeters();
                SplitMeters();
                RaisePropertyChanged(nameof(Meters));
                RaisePropertyChanged(nameof(MetersLeftColumn));
                RaisePropertyChanged(nameof(MetersRightColumn));
            });
        }
    }
}
