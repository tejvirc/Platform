namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Contracts;
    using Contracts.Localization;
    using Contracts.MeterPage;
    using Contracts.OperatorMenu;
    using Contracts.Tickets;
    using Events;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Monaco.Localization.Properties;
    using MVVM;
    using MVVM.Command;
    using OperatorMenu;

    [CLSCompliant(false)]
    public class MainMetersPageViewModel : MetersPageViewModelBase
    {
        private string _dropMessageLabelText;
        private string _periodWarningText;
        private bool _clearButtonEnabled;

        public MainMetersPageViewModel() : base(MeterNodePage.MainPage)
        {
            ClearPeriodCommand = new ActionCommand<object>(ClearPeriod_Clicked);

            PrintVerificationButtonClickedCommand = new ActionCommand<object>(_ => Print(OperatorMenuPrintData.Custom2));
            PrintPeriodicResetButtonClickedCommand = new ActionCommand<object>(_ => Print(OperatorMenuPrintData.Custom1));
            PrintAuditTicketButtonClickedCommand = new ActionCommand<object>(_ => Print(OperatorMenuPrintData.Custom3));

            PrintVerificationButtonIsVisible = GetConfigSetting(OperatorMenuSetting.MainButtonPrintVerificationVisible, true);
            PrintAuditTicketButtonIsVisible = GetConfigSetting(OperatorMenuSetting.MainButtonPrintAuditTicketVisible, false);
            ClearPeriodMetersButtonIsVisible = GetConfigSetting(OperatorMenuSetting.ClearPeriodMetersButtonVisible, true);

            _clearButtonEnabled = GameIdle;
        }

        public ICommand ClearPeriodCommand { get; }

        public ICommand PrintVerificationButtonClickedCommand { get; set; }

        public ICommand PrintPeriodicResetButtonClickedCommand { get; set; }

        public ICommand PrintAuditTicketButtonClickedCommand { get; set; }

        public string PeriodWarningText
        {
            get => _periodWarningText;

            set
            {
                _periodWarningText = value;
                RaisePropertyChanged(nameof(PeriodWarningText));
                UpdateStatusText();
            }
        }

        public bool PrintVerificationButtonIsVisible { get; }

        public bool PrintAuditTicketButtonIsVisible { get; }

        public bool ClearPeriodMetersButtonIsVisible { get; }

        public string DropMessageLabelText
        {
            get => _dropMessageLabelText;
            set => SetProperty(ref _dropMessageLabelText, value);
        }

        public bool ClearButtonEnabled
        {
            get => _clearButtonEnabled && FieldAccessEnabled;

            set
            {
                _clearButtonEnabled = value;
                RaisePropertyChanged(nameof(ClearButtonEnabled));
                if (ClearPeriodMetersButtonIsVisible)
                {
                    // Only show the warning text if the button is not enabled and game is not idle, since the button
                    // can be disabled while printing and incorrect error text will be shown.
                    PeriodWarningText = !_clearButtonEnabled && !GameIdle
                        ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PeriodClearResetNoGameMessage)
                        : string.Empty;
                }
            }
        }

        private void ClearPeriod_Clicked(object sender)
        {
            var serviceManager = ServiceManager.GetInstance();
            var dialogService = serviceManager.GetService<IDialogService>();

            var result = dialogService.ShowYesNoDialog(
                this,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ConfirmClearPeriodMessage));

            if (result == true)
            {
                Print(OperatorMenuPrintData.Custom1, () =>
                {
                    serviceManager.GetService<IMeterManager>().ClearAllPeriodMeters();
                    MvvmHelper.ExecuteOnUI(UpdateUI);
                });
            }
        }

        private List<Ticket> GeneratePeriodicResetTicket()
        {
            var ticketCreator = ServiceManager.GetInstance().TryGetService<IPeriodicResetTicketCreator>();
            var tickets = SplitTicket(ticketCreator?.Create());

            if (tickets.Count > 1)
            {
                var secondTicket = tickets[1];
                var ticket = ticketCreator?.CreateSecondPage();
                if(ticket != null)
                {
                    secondTicket[TicketConstants.Left] = $"{ticket[TicketConstants.Left]}{secondTicket[TicketConstants.Left]}";
                    secondTicket[TicketConstants.Center] = $"{ticket[TicketConstants.Center]}{secondTicket[TicketConstants.Center]}";
                    secondTicket[TicketConstants.Right] = $"{ticket[TicketConstants.Right]}{secondTicket[TicketConstants.Right]}";
                }
            }

            return tickets;
        }

        private static IEnumerable<Ticket> GenerateSingaporeClubsAuditTickets()
        {
            var ticketCreator = ServiceManager.GetInstance().TryGetService<ISingaporeClubsAuditTicketCreator>();
            return ticketCreator?.Create();
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            IEnumerable<Ticket> tickets = null;

            switch (dataType)
            {
                case OperatorMenuPrintData.Main:
                    var ticketCreator = ServiceManager.GetInstance().TryGetService<IMetersTicketCreator>();
                    var printMeters = Meters.Where(m => m.Meter != null && (ShowLifetime || m.DisplayPeriod))
                        .Select(m => new Tuple<IMeter, string>(m.Meter, m.Name)).ToList();
                    tickets = ticketCreator?.CreateMetersTickets(printMeters, ShowLifetime);
                    break;
                case OperatorMenuPrintData.Custom1:
                    tickets = GeneratePeriodicResetTicket();
                    break;
                case OperatorMenuPrintData.Custom2:
                    tickets = GeneratePrintVerificationTickets();
                    break;
                case OperatorMenuPrintData.Custom3:
                    tickets = GenerateSingaporeClubsAuditTickets();
                    break;
            }

            return tickets;
        }

        protected override void OnLoaded()
        {
            UpdateUI();

            base.OnLoaded();

            ClearButtonEnabled = GameIdle;

            EventBus.Subscribe<OperatorMenuPrintJobStartedEvent>(this, _ =>
                {
                    ClearButtonEnabled = false;
                }
            );

            EventBus.Subscribe<OperatorMenuPrintJobCompletedEvent>(this, _ =>
                {
                    ClearButtonEnabled = GameIdle;
                }
            );
        }

        protected override void UpdateStatusText()
        {
            if (!string.IsNullOrEmpty(PeriodWarningText))
            {
                EventBus.Publish(new OperatorMenuWarningMessageEvent(PeriodWarningText));
            }
            else
            {
                base.UpdateStatusText();
            }
        }

        protected override void OnFieldAccessEnabledChanged()
        {
            RaisePropertyChanged(nameof(ClearButtonEnabled));
        }

        private void UpdateUI()
        {
            SplitMeters();

            if (PropertiesManager.GetValue(ApplicationConstants.StackerRemovedBehaviorAutoClearPeriodMetersText, false))
            {
                DropMessageLabelText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DropMessage);
            }
        }
    }
}
