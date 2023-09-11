namespace Aristocrat.Monaco.Application.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Contracts.Extensions;
    using Contracts.Localization;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using Localization.Properties;

    /// <inheritdoc />
    public class AuditTicket : TextTicket
    {
        private const string Culture = CultureFor.OperatorTicket;
        private const int MaxPrintPropertyNameLength = 29;

        /// <inheritdoc />
        public AuditTicket(string titleOverride = null) : base(Localizer.For(Culture))
        {
            Title = string.IsNullOrEmpty(titleOverride) ? string.Empty : titleOverride;
            var operatorTicketDateFormat = PropertiesManager.GetValue(
                ApplicationConstants.LocalizationOperatorTicketDateFormat,
                ApplicationConstants.DefaultDateFormat);
            DateTimeFormat = $"{operatorTicketDateFormat} {TimeFormat}";
        }

        /// <summary>
        ///     Update the title of this ticket
        /// </summary>
        /// <param name="title">The new title</param>
        public void UpdateTitle(string title)
        {
            Title = title;
        }

        /// <summary>
        ///     Gets the interface this service implements and exposes as a service
        /// </summary>
        public string DateTimeFormat { get; }

        /// <inheritdoc />
        public override int TicketCasinoInfoLineCount => 0;

        /// <inheritdoc />
        public override int TicketHeaderLineCount => 11;

        /// <inheritdoc />
        public override void AddCasinoInfo()
        {
            // Moved to Header
        }

        /// <inheritdoc />
        public override void AddTicketHeader()
        {
            // Space for title
            AddLabeledLine(string.Empty, string.Empty, false);

            // Date and time of printing
            AddLabeledLine(ResourceKeys.DateAndTime, Time.GetLocationTime().ToString(DateTimeFormat));

            // Serial number
            AddLabeledLine(
                ResourceKeys.SerialNumberLabel,
                PropertiesManager.GetValue(
                    ApplicationConstants.SerialNumber,
                    TicketLocalizer.GetString(ResourceKeys.DataUnavailable)));

            // Asset number
            var machineId = PropertiesManager.GetValue(ApplicationConstants.MachineId, (uint)0);
            var assetNumber = string.Empty;
            if (machineId != 0)
            {
                assetNumber = machineId.ToString();
            }

            AddLabeledLine(ResourceKeys.AssetNumber, assetNumber);

            // MAC Address
            AddLabeledLine(
                ResourceKeys.MacAddressLabel,
                string.Join(":", Enumerable.Range(0, 6).Select(i => NetworkInterfaceInfo.DefaultPhysicalAddress.Substring(i * 2, 2))));

            // Jurisdiction
            AddLabeledLine(
                ResourceKeys.JurisdictionLabel,
                PropertiesManager.GetValue(ApplicationConstants.JurisdictionKey, string.Empty));

            // Property name
            // *NOTE* Property name (ticket text line 1) can support up to 40 characters, only 30 characters will fit on the
            // audit ticket layout without issue so we split any remaining characters to print on the subsequent line.
            var propertyName = (string)PropertiesManager.GetProperty(PropertyKey.TicketTextLine1, string.Empty);
            if (propertyName.Length > MaxPrintPropertyNameLength)
            {
                AddLabeledLine(ResourceKeys.PropertyName, propertyName.Substring(0, MaxPrintPropertyNameLength));
                AddLabeledLine(string.Empty, propertyName.Substring(MaxPrintPropertyNameLength).PadRight(MaxPrintPropertyNameLength, ' '), false);
            }
            else
            {
                AddLabeledLine(ResourceKeys.PropertyName, propertyName);
            }

            // Currency name
            AddLabeledLine(ResourceKeys.Currency, CurrencyExtensions.Currency.CurrencyName);

            AddLine(null, null, null);
        }

        /// <inheritdoc />
        public override void AddTicketContent()
        {
        }

        /// <summary>
        ///     Creates a list of tickets
        /// </summary>
        /// <returns>A list of Tickets</returns>
        public List<Ticket> CreateAuditTickets()
        {
            return new List<Ticket> { CreateTextTicket() };
        }
    }
}