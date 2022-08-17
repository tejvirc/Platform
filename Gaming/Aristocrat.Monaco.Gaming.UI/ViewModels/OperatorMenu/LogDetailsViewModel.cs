namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Application.Contracts.Localization;
    using Application.Contracts.TiltLogger;
    using Application.UI.Models;
    using Application.UI.OperatorMenu;
    using Common;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Localization.Properties;
    using MVVM;
    using Microsoft.Toolkit.Mvvm.Input;
    //using MVVM.Command;

    [CLSCompliant(false)]
    public class LogDetailsViewModel : OperatorMenuSaveViewModelBase
    {
        private bool _canReprint;
        private bool _isReprintButtonVisible = true;
        private bool _reprintDisabledDueToDoor;
        private string _doorStatusText;
        private bool _isMostRecentRowSelected;

        public RelayCommand<object> ReprintButtonCommand { get; set; }

        private readonly IEventLogAdapter _eventLogAdapter;
        private static long _transactionId;

        /// <summary>
        ///     Set if the reprint button should be activated
        /// </summary>
        public bool CanReprint
        {
            get => _canReprint && PrinterButtonsEnabled;

            set
            {
                _canReprint = value;
                RaisePropertyChanged(nameof(CanReprint));
                if (ReprintButtonCommand != null)
                {
                    MvvmHelper.ExecuteOnUI(() => ReprintButtonCommand.NotifyCanExecuteChanged());
                }
            }
        }

        public bool IsReprintButtonVisible
        {
            get => _isReprintButtonVisible;
            set
            {
                if (_isReprintButtonVisible != value)
                {
                    _isReprintButtonVisible = value;
                    RaisePropertyChanged(nameof(IsReprintButtonVisible));
                }
            }
        }

        public LogDetailsViewModel(EventLog eventDescription, IEventLogAdapter eventLogAdapter)
        {
            _transactionId = eventDescription.Description.TransactionId;
            _eventLogAdapter = eventLogAdapter;
            var logSequence = eventDescription.Description.LogSequence;
            if (eventDescription.HasAdditionalInfo)
            {
                foreach (var (name, value) in eventDescription.Description.AdditionalInfos)
                {
                    AdditionalInfoItems.Add(new Tuple<string, string>(name, value));
                }
            }

            if (logSequence != -1 && _eventLogAdapter != null && _eventLogAdapter.GetMaxLogSequence() == logSequence)
            {
                _isMostRecentRowSelected = true;
                UpdateReprintButtonEnabled(true);
            }

            if (_eventLogAdapter is ILogTicketPrintable printable)
            {
                _isReprintButtonVisible = printable.IsReprintSupported();
            }
            else
            {
                _isReprintButtonVisible = false;
                return;
            }

            SetHandpayReprintButton(true);

            if (!_isReprintButtonVisible)
            {
                return;
            }

            const string defaultValue = "None";

            ReprintBehavior = PropertiesManager.GetValue(
                AccountingConstants.ReprintLoggedVoucherBehavior,
                defaultValue);

            if (ReprintBehavior == defaultValue || ReprintBehavior == "Last" && !IsMostRecentRowSelected)
            {
                _isReprintButtonVisible = false;
                return;
            }

            var doorRequirements = PropertiesManager.GetValue(
                AccountingConstants.ReprintLoggedVoucherDoorOpenRequirement,
                string.Empty);
            if (!string.IsNullOrEmpty(doorRequirements) &&
                !doorRequirements.Equals(defaultValue, StringComparison.InvariantCultureIgnoreCase))
            {
                ReprintDoorOpenRequirement = doorRequirements.Split('|');

                // Subscribe to door events
                EventBus.Subscribe<ClosedEvent>(this, HandleDoorEvent);
                EventBus.Subscribe<OpenEvent>(this, HandleDoorEvent);
            }

            UpdateReprintButtonEnabled(true);
            ReprintButtonCommand = new RelayCommand<object>(
                _ => Print(OperatorMenuPrintData.SelectedItem),
                _ => CanReprint);
        }

        private void SetHandpayReprintButton(bool skipRaisePropertyChanged = false)
        {
            if (_eventLogAdapter.LogType == EventLogType.Handpay.GetDescription(typeof(EventLogType)))
            {
                var printedItem = AdditionalInfoItems.FirstOrDefault(l => string.Equals(l.Item1, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Printed).ToUpper(), StringComparison.InvariantCultureIgnoreCase));
                if (printedItem != null && string.Equals(printedItem.Item2, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.No), StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!skipRaisePropertyChanged)
                    {
                        IsReprintButtonVisible = false;
                    }
                    else
                    {
                        _isReprintButtonVisible = false;
                    }
                    
                }
            }
        }

        protected string[] ReprintDoorOpenRequirement { get; set; }

        protected string ReprintBehavior { get; set; }

        public string DoorStatusText
        {
            get => _doorStatusText;
            set
            {
                if (_doorStatusText != value)
                {
                    _doorStatusText = value;
                    RaisePropertyChanged(nameof(DoorStatusText));
                }
            }
        }

        /// <summary>
        ///     ReprintDisabledDueToDoor
        /// </summary>
        public bool ReprintDisabledDueToDoor
        {
            get => _reprintDisabledDueToDoor;

            set
            {
                if (_reprintDisabledDueToDoor != value)
                {
                    _reprintDisabledDueToDoor = value;
                    RaisePropertyChanged(nameof(ReprintDisabledDueToDoor));
                }
            }
        }

        private void UpdateReprintButtonEnabled(bool skipRaisePropertyChanged = false)
        {
            var setReprintAvailability = new Action<bool>(
                canReprint =>
                {
                    if (skipRaisePropertyChanged)
                    {
                        _canReprint = canReprint;
                    }
                    else
                    {
                        CanReprint = canReprint;
                    }
                });

            _doorStatusText = string.Empty;
            if (!skipRaisePropertyChanged)
            {
                RaisePropertyChanged(nameof(DoorStatusText));
            }

            if (ReprintDoorOpenRequirement?.Length > 0)
            {
                // A door is required.  Is it open?
                var door = ServiceManager.GetInstance().GetService<IDoorService>();
                var doorNames = string.Empty;
                foreach (var d in ReprintDoorOpenRequirement)
                {
                    var logicalDoorId = DoorOpenRequirementToDoorLogicalId(d);
                    if (door.GetDoorClosed(logicalDoorId))
                    {
                        doorNames += door.GetDoorName(logicalDoorId) + ", ";
                    }
                }

                _reprintDisabledDueToDoor = false;
                if (!skipRaisePropertyChanged)
                {
                    RaisePropertyChanged(nameof(ReprintDisabledDueToDoor));
                }

                if (!string.IsNullOrEmpty(doorNames))
                {
                    _doorStatusText =
                        $"{doorNames.TrimEnd(',', ' ')} {Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OpenDoorToReprintText)}";
                    if (!skipRaisePropertyChanged)
                    {
                        RaisePropertyChanged(nameof(DoorStatusText));
                    }

                    setReprintAvailability(false);

                    _reprintDisabledDueToDoor = true;
                    if (!skipRaisePropertyChanged)
                    {
                        RaisePropertyChanged(nameof(ReprintDisabledDueToDoor));
                    }
                    return;
                }
            }

            var canReprint = ReprintBehavior == "Any" || ReprintBehavior == "Last" && IsMostRecentRowSelected;
            setReprintAvailability(canReprint);
        }

        /// <summary>
        ///     Set if the selected row is the most recent one.
        /// </summary>
        public bool IsMostRecentRowSelected
        {
            get => _isMostRecentRowSelected;

            set
            {
                if (_isMostRecentRowSelected != value)
                {
                    _isMostRecentRowSelected = value;
                    RaisePropertyChanged(nameof(IsMostRecentRowSelected));
                    UpdateReprintButtonEnabled();
                }
            }
        }

        protected override void UpdatePrinterButtons()
        {
            UpdateReprintButtonEnabled();
        }

        protected void HandleDoorEvent(IEvent data)
        {
            // Remove reprint button
            UpdateReprintButtonEnabled();
        }

        private static int DoorOpenRequirementToDoorLogicalId(string doorOpenRequirement)
        {
            if (!int.TryParse(doorOpenRequirement, out var doorLogicalId))
            {
                if (Enum.TryParse(doorOpenRequirement, true, out DoorLogicalId doorLogicalIdEnum))
                {
                    return (int)doorLogicalIdEnum;
                }
            }

            return doorLogicalId;
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            IEnumerable<Ticket> tickets = null;

            switch (dataType)
            {
                case OperatorMenuPrintData.SelectedItem:
                    tickets = TicketToList(GenerateReprintVoucher());
                    break;
            }

            return tickets;
        }

        private Ticket GenerateReprintVoucher()
        {
            if (_eventLogAdapter is ILogTicketPrintable printable)
            {
                return printable.GenerateReprintTicket(_transactionId);
            }

            return null;
        }

        public List<Tuple<string, string>> AdditionalInfoItems { get; } = new List<Tuple<string, string>>();
    }
}
