namespace Aristocrat.Monaco.Application.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Contracts;
    using Contracts.ConfigWizard;
    using Contracts.Localization;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     This class creates information ticket objects
    /// </summary>
    public class InformationTicketCreator : IInformationTicketCreator
    {
        private static readonly object Lock = new object();

        private ILocalizer _ticketLocalizer;
        private IPropertiesManager _propertiesManager;

        private StringBuilder _centerField;
        private StringBuilder _leftField;
        private StringBuilder _rightField;

        /// <summary>
        ///     Creates an information ticket, a ticket with a common header and free-form body.
        /// </summary>
        /// <param name="title">The text to print in the title area of the ticket</param>
        /// <param name="body">The pre-formatted text to print in the body of the ticket</param>
        /// <returns>A Ticket object with fields required for an information ticket.</returns>
        public Ticket CreateInformationTicket(string title, string body)
        {
            _ticketLocalizer = Localizer.For(CultureFor.OperatorTicket);
            _propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            var ticket = new Ticket
            {
                ["ticket type"] = "text",
                ["title"] = title,
                ["copyright"] = "COPYRIGHT " +
                                ServiceManager.GetInstance().GetService<ITime>().GetLocationTime(DateTime.UtcNow)
                                    .Year +
                                " Aristocrat, INC."
            };


            // Transform and create fields needed by template regions
            lock (Lock)
            {
                _leftField = new StringBuilder();
                _centerField = new StringBuilder();
                _rightField = new StringBuilder();

                var headerSpace = "\n\n\n";
                AddLine(headerSpace, headerSpace, headerSpace);

                AddLine(
                    $"{_ticketLocalizer.GetString(ResourceKeys.SerialNumber)}:",
                    null,
                    string.Format(
                        CultureInfo.CurrentCulture,
                        $"{_propertiesManager.GetValue(ApplicationConstants.SerialNumber, _ticketLocalizer.GetString(ResourceKeys.DataUnavailable))}"));

                var machineId = _propertiesManager.GetValue(ApplicationConstants.MachineId, (uint)0);
                var assetNumber = string.Empty;
                if (machineId != 0)
                {
                    assetNumber = machineId.ToString();
                }

                AddLine(
                    $"{_ticketLocalizer.GetString(ResourceKeys.Machine)}:",
                    null,
                    string.Format(
                        CultureInfo.CurrentCulture,
                        assetNumber));

                if (ConfigWizardUtil.VisibleByConfig(_propertiesManager, ApplicationConstants.ConfigWizardIdentityPageZoneOverride))
                {
                    AddLine(
                        $"{_ticketLocalizer.GetString(ResourceKeys.ZoneText)}:",
                        null,
                        string.Format(
                            CultureInfo.CurrentCulture,
                            $"{_propertiesManager.GetValue(ApplicationConstants.Zone, _ticketLocalizer.GetString(ResourceKeys.DataUnavailable))}"));
                }

                if (ConfigWizardUtil.VisibleByConfig(_propertiesManager, ApplicationConstants.ConfigWizardIdentityPageBankOverride))
                {
                    AddLine(
                        $"{_ticketLocalizer.GetString(ResourceKeys.BankText)}:",
                        null,
                        string.Format(
                            CultureInfo.CurrentCulture,
                            $"{_propertiesManager.GetValue(ApplicationConstants.Bank, _ticketLocalizer.GetString(ResourceKeys.DataUnavailable))}"));
                }

                if (ConfigWizardUtil.VisibleByConfig(_propertiesManager, ApplicationConstants.ConfigWizardIdentityPagePositionOverride))
                {
                    AddLine(
                        $"{_ticketLocalizer.GetString(ResourceKeys.PositionText)}:",
                        null,
                        string.Format(
                            CultureInfo.CurrentCulture,
                            $"{_propertiesManager.GetValue(ApplicationConstants.Position, _ticketLocalizer.GetString(ResourceKeys.DataUnavailable))}"));
                }

                AddLine(null, null, null);

                AddLine(body, null, null);

                ticket["left"] = _leftField.ToString();
                ticket["center"] = _centerField.ToString();
                ticket["right"] = _rightField.ToString();
            }

            return ticket;
        }

        /// <summary>
        ///     Gets the name of the service
        /// </summary>
        public string Name => "Information Ticket Creator";

        /// <summary>
        ///     Gets the interface this service implements and exposes as a service
        /// </summary>
        public ICollection<Type> ServiceTypes => new[] { typeof(IInformationTicketCreator) };

        /// <summary>
        ///     Initializes the service
        /// </summary>
        public void Initialize()
        {
        }

        private void AddLine(string left, string center, string right)
        {
            _leftField.AppendLine(left);
            _centerField.AppendLine(center);
            _rightField.AppendLine(right);
        }
    }
}