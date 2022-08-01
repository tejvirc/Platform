namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.MeterPage;
    using Application.UI.MeterPage;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Contracts.Meters;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Monaco.UI.Common.Extensions;
    using MVVM;
    using MVVM.Command;

    [CLSCompliant(false)]
    public class DenomMetersPageViewModel : MetersPageViewModelBase
    {
        private readonly IGameMeterManager _meters;

        private Denomination _selectedDenom;
        private int _selectedDenomIndex;

        public DenomMetersPageViewModel() : base(MeterNodePage.Denom, true)
        {
            _meters = ServiceManager.GetInstance().TryGetService<IGameMeterManager>();

            var games = PropertiesManager.GetValues<IGameDetail>(GamingConstants.Games);
            Denoms = new List<Denomination>(
                games.SelectMany(g => g.SupportedDenominations).Distinct().OrderBy(d => d).Select(d => new Denomination(d)));
            _selectedDenom = Denoms.FirstOrDefault();

            PreviousDenomCommand = new ActionCommand<object>(PreviousDenom);
            NextDenomCommand = new ActionCommand<object>(NextDenom);
        }

        public ICommand PreviousDenomCommand { get; }

        public ICommand NextDenomCommand { get; }

        public bool PreviousDenomIsEnabled => SelectedDenomIndex > 0 && !DataEmpty;

        public bool NextDenomIsEnabled => SelectedDenomIndex < Denoms.Count - 1 && !DataEmpty;

        public List<Denomination> Denoms { get; }

        public Denomination SelectedDenom
        {
            get => _selectedDenom;
            set
            {
                _selectedDenom = value;
                RaisePropertyChanged(nameof(SelectedDenom));
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
                RaisePropertyChanged(nameof(PreviousDenomIsEnabled));
                RaisePropertyChanged(nameof(NextDenomIsEnabled));
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
            MvvmHelper.ExecuteOnUI(
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
                            try
                            {
                                var meter = _meters.GetMeter(SelectedDenom.Millicents, meterNode.Name);
                                Meters.Add(new DisplayMeter(meterNode.DisplayName, meter, ShowLifetime, meterNode.Order));
                            }
                            catch (MeterNotFoundException)
                            {
                                Meters.Add(new DisplayMeter(meterNode.DisplayName, null, ShowLifetime, meterNode.Order));
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
                    RaisePropertyChanged(nameof(MetersRightColumnVisible));
                });
        }

        protected override void UpdatePrinterButtons()
        {
            RaisePropertyChanged(nameof(PreviousDenomIsEnabled));
            RaisePropertyChanged(nameof(NextDenomIsEnabled));
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
            SelectedDenomIndex = 0;
            base.OnLoaded();
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

        public struct Denomination
        {
            public Denomination(long millicents)
            {
                Millicents = millicents;
                DisplayValue = millicents.MillicentsToDollars().FormattedCurrencyString();
            }

            public long Millicents { get; }

            public string DisplayValue { get; }
        }
    }
}
