namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.MeterPage;
    using Application.UI.MeterPage;
    using Application.UI.OperatorMenu;
    using Aristocrat.Extensions.CommunityToolkit;
    using CommunityToolkit.Mvvm.Input;
    using Contracts;
    using Contracts.Meters;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Monaco.UI.Common.Extensions;

    [CLSCompliant(false)]
    public class DenomMetersPageViewModel : MetersPageViewModelBase
    {
        private readonly IGameMeterManager _meters;

        private Denomination _selectedDenom;
        private int _selectedDenomIndex;

        public DenomMetersPageViewModel() : base(MeterNodePage.Denom, true)
        {
            _meters = ServiceManager.GetInstance().TryGetService<IGameMeterManager>();

            PreviousDenomCommand = new RelayCommand<object>(PreviousDenom);
            NextDenomCommand = new RelayCommand<object>(NextDenom);
        }

        public ICommand PreviousDenomCommand { get; }

        public ICommand NextDenomCommand { get; }

        public bool PreviousDenomIsEnabled => SelectedDenomIndex > 0 && !DataEmpty;

        public bool NextDenomIsEnabled => SelectedDenomIndex < Denoms?.Count - 1 && !DataEmpty;

        public List<Denomination> Denoms { get; private set; }

        public Denomination SelectedDenom
        {
            get => _selectedDenom;
            set
            {
                _selectedDenom = value;
                OnPropertyChanged(nameof(SelectedDenom));
                InitializeMeters();
            }
        }

        public int SelectedDenomIndex
        {
            get => _selectedDenomIndex;
            set
            {
                if (value < 0 || value >= Denoms.Count)
                {
                    return;
                }

                _selectedDenomIndex = value;
                SelectedDenom = Denoms[value];
                OnPropertyChanged(nameof(PreviousDenomIsEnabled));
                OnPropertyChanged(nameof(NextDenomIsEnabled));
            }
        }

        public bool MetersRightColumnVisible => MetersRightColumn.Count > 0;

        protected override void InitializeMeters()
        {
            if (Denoms == null || !Denoms.Any())
            {
                return;
            }

            // This will occur each time a different denom is selected
            Execute.OnUIThread(
                () =>
                {
                    foreach (var meter in Meters)
                    {
                        meter.Dispose();
                    }

                    Meters.Clear();
                    MetersLeftColumn.Clear();
                    MetersRightColumn.Clear();

                    foreach (var meterNode in MeterNodes)
                    {
                        if (meterNode.DisplayName.IsEmpty() && meterNode.Name == "blank line")
                        {
                            var lifetime = new BlankDisplayMeter(ShowLifetime, meterNode.Order);
                            Meters.Add(lifetime);
                        }
                        else
                        {
                            string meterDisplayName = GetDisplayNameFromMeterNode(meterNode);

                            try
                            {
                                var meter = _meters.GetMeter(SelectedDenom.Millicents, meterNode.Name);
                                Meters.Add(
                                    new DisplayMeter(
                                        meterDisplayName,
                                        meter,
                                        ShowLifetime,
                                        meterNode.Order,
                                        true,
                                        false,
                                        UseOperatorCultureForCurrencyFormatting));
                            }
                            catch (MeterNotFoundException)
                            {
                                Meters.Add(
                                    new DisplayMeter(
                                        meterDisplayName,
                                        null,
                                        ShowLifetime,
                                        meterNode.Order,
                                        true,
                                        false,
                                        UseOperatorCultureForCurrencyFormatting));
                                Logger.ErrorFormat("Meter not found: {0}", meterNode.Name);
                            }
                        }
                    }

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
                    OnPropertyChanged(nameof(MetersRightColumnVisible));
                });
        }

        protected override void UpdatePrinterButtons()
        {
            OnPropertyChanged(nameof(PreviousDenomIsEnabled));
            OnPropertyChanged(nameof(NextDenomIsEnabled));
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            IEnumerable<Ticket> tickets = null;

            switch (dataType)
            {
                case OperatorMenuPrintData.Main:
                    var ticketCreator = ServiceManager.GetInstance().TryGetService<IDenomMetersTicketCreator>();
                    tickets = ticketCreator.CreateDenomMetersTicket(SelectedDenom.Millicents, ShowLifetime);
                    break;
            }

            return tickets;
        }

        protected override void OnLoaded()
        {
            LoadDenoms();
            SelectedDenomIndex = 0;
            base.OnLoaded();
        }

        protected override void OnOperatorCultureChanged(OperatorCultureChangedEvent evt)
        {
            Execute.OnUIThread(LoadDenoms);

            base.OnOperatorCultureChanged(evt);
        }

        private void NextDenom(object sender)
        {
            // cycle on last game
            SelectedDenomIndex = SelectedDenomIndex == Denoms.Count - 1
                ? 0
                : SelectedDenomIndex + 1;
        }

        private void PreviousDenom(object sender)
        {
            // cycle on first game
            SelectedDenomIndex = SelectedDenomIndex == 0
                ? Denoms.Count - 1
                : SelectedDenomIndex - 1;
        }

        private void LoadDenoms()
        {
            Denoms?.Clear();

            var games = PropertiesManager.GetValues<IGameDetail>(GamingConstants.Games);
            Denoms = new List<Denomination>(
                games.SelectMany(g => g.SupportedDenominations).Distinct().OrderBy(d => d).Select(d => new Denomination(d, d.MillicentsToDollars().FormattedCurrencyString(false, GetCurrencyDisplayCulture()))));
            OnPropertyChanged(nameof(Denoms));
            SelectedDenom = Denoms.FirstOrDefault();
        }

        public struct Denomination
        {
            public Denomination(long millicents, string displayValue)
            {
                Millicents = millicents;
                DisplayValue = displayValue;
            }

            public long Millicents { get; }

            public string DisplayValue { get; }
        }
    }
}
