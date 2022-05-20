namespace Aristocrat.Monaco.Application.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Contracts;
    using Contracts.ConfigWizard;
    using Contracts.Localization;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     This class creates identity ticket objects
    /// </summary>
    public class IdentityTicketCreator : IIdentityTicketCreator, IService
    {
        /// <summary>
        ///     The key to access the game system property.
        /// </summary>
        private const string SystemSpecificationKey = "GamePlay.GameSystemString";

        /// <summary>
        ///     The key to access the ticket text property.
        /// </summary>
        private const string TicketTextLine1Key = "TicketProperty.TicketTextLine1";

        /// <summary>
        ///     The key to access the ticket text property.
        /// </summary>
        private const string TicketTextLine2Key = "TicketProperty.TicketTextLine2";

        /// <summary>
        ///     The key to access the ticket text property.
        /// </summary>
        private const string TicketTextLine3Key = "TicketProperty.TicketTextLine3";

        /// <summary>
        ///     The key to access the ticket text property.
        /// </summary>
        private const string TicketTextLine4Key = "TicketProperty.TicketTextLine4";

        /// <summary>
        ///     Property key used for accessing the bingo system version.
        /// </summary>
        private const string GameServerVersionKey = "GameServer.Version";

        /// <summary>
        ///     Creates an identity ticket.
        /// </summary>
        /// <returns>A Ticket object with fields required for an identity ticket.</returns>
        public Ticket CreateIdentityTicket()
        {
            // Needed services
            var serviceManager = ServiceManager.GetInstance();
            var propertiesManager = serviceManager.GetService<IPropertiesManager>();
            var timeService = serviceManager.GetService<ITime>();

            // Needed properties
            var systemSpecification =
                (string)propertiesManager.GetProperty(SystemSpecificationKey, "System Unknown");
            var cabinetStyle = (string)propertiesManager.GetProperty(ApplicationConstants.CabinetStyle, string.Empty);
            var clientVersion = (string)propertiesManager.GetProperty(KernelConstants.SystemVersion,
                Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.NotSet));
            var gameServerVersion = (string)propertiesManager.GetProperty(GameServerVersionKey,
                Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.NotSet));

            // Fill out the ticket
            var ticket = new Ticket
            {
                ["ticket type"] = "text",
                ["title"] = "IDENTITY TICKET",
                ["establishment name"] = (string)propertiesManager.GetProperty(
                    TicketTextLine1Key,
                    Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.DataUnavailable)),
                ["header 1"] = (string)propertiesManager.GetProperty(
                    TicketTextLine2Key,
                    Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.DataUnavailable)),
                ["header 2"] = (string)propertiesManager.GetProperty(
                    TicketTextLine3Key,
                    Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.DataUnavailable)),
                ["header 3"] = (string)propertiesManager.GetProperty(
                    TicketTextLine4Key,
                    Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.DataUnavailable))
            };


            var localTime = timeService.GetLocationTime(DateTime.UtcNow);
            ticket["datetime"] = localTime.ToString("ddd MMM dd\nHH:mm:ss yyyy", CultureInfo.CurrentCulture);
            ticket["datetime numbers"] = localTime.ToString(CultureInfo.CurrentCulture);

            // this shows as Tkt. # on the ticket
            ticket["sequence number"] = "00000000";

            // The validation number is a random number between 0 and 999998
            var random = new Random(DateTime.UtcNow.Millisecond);
            ticket["legacy validation"] = random.Next(999999).ToString(CultureInfo.InvariantCulture);

            ticket["validation"] = NetworkInterfaceInfo.DefaultPhysicalAddress;

            ticket["serial id"] = propertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty);
            ticket["machine id"] = propertiesManager.GetValue(ApplicationConstants.MachineId, (uint)0).ToString(CultureInfo.CurrentCulture);

            var visible = ConfigWizardUtil.VisibleByConfig(propertiesManager, ApplicationConstants.ConfigWizardIdentityPageZoneOverride);
            ticket["zone"] = (string)propertiesManager.GetProperty(visible ? ApplicationConstants.Zone : "", string.Empty);

            visible = ConfigWizardUtil.VisibleByConfig(propertiesManager, ApplicationConstants.ConfigWizardIdentityPageBankOverride);
            ticket["bank"] = (string)propertiesManager.GetProperty(visible ? ApplicationConstants.Bank : "", string.Empty);

            visible = ConfigWizardUtil.VisibleByConfig(propertiesManager, ApplicationConstants.ConfigWizardIdentityPagePositionOverride);
            ticket["position"] = (string)propertiesManager.GetProperty(visible ? ApplicationConstants.Position : "", string.Empty);

            ticket["client version"] = clientVersion;
            ticket["version"] = systemSpecification + " v " + gameServerVersion + cabinetStyle;
            ticket["copyright"] = "COPYRIGHT 1996 - " +
                                  ServiceManager.GetInstance().GetService<ITime>().GetLocationTime(DateTime.UtcNow)
                                      .Year +
                                  " VGT, INC.";

            return ticket;
        }

        /// <summary>
        ///     Gets the name of the service
        /// </summary>
        public string Name => "Identity Ticket Creator";

        /// <summary>
        ///     Gets the interface this service implements and exposes as a service
        /// </summary>
        public ICollection<Type> ServiceTypes => new[] { typeof(IIdentityTicketCreator) };

        /// <summary>
        ///     Initializes the service
        /// </summary>
        public void Initialize()
        {
        }
    }
}