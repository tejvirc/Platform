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

        /// <summary>
        ///     Add a line with a label on the left and a value on the right
        /// </summary>
        /// <param name="label">Either use a ResourceKey or set localizeLabel to false</param>
        /// <param name="value">The value</param>
        /// <param name="localizeLabel">True if the label should be localized.</param>
        public void AddLabeledLine(string label, string value, bool localizeLabel = true)
        {
            AddResolvedLine(FixWidth(localizeLabel ? TicketLocalizer.GetString(label) : label, value));
        }

        /// <summary>
        ///     Add Labels and Values to a ticket given a tuple of string arrays
        /// </summary>
        /// <param name="resolvedLine">A tuple of string arrays</param>
        private void AddResolvedLine((string[] labelMember, string[] valueMember) resolvedLine)
        {
            for (var i = 0; i < resolvedLine.labelMember.Length; i++)
            {
                AddLine(resolvedLine.labelMember[i], null, resolvedLine.valueMember[i]);
            }
        }

        /// <summary>
        ///     Break up lines until they fit on the ticket
        /// </summary>
        /// <param name="label">The unbroken label</param>
        /// <param name="value">The unbroken value</param>
        /// <returns>A tuple of string arrays</returns>
        private static (string[] labelMember, string[] valueMember) FixWidth(string label, string value)
        {
            var original = (labelMember: new[] { label }, valueMember: new[] { value });
            if (CheckStringsLength(original))
            {
                return original;
            }

            // Try reformatting the value
            var (brokenList, fixedList) = BreakupStrings(value, original.labelMember);
            var check = (labelMember: fixedList, valueMember: brokenList);

            if (CheckStringsLength(check))
            {
                return check;
            }

            // Try reformatting the label
            check = BreakupStrings(label, check.valueMember);

            // Nothing more we can do, just return this result
            return check;
        }

        /// <summary>
        ///     Checks whether any of the lines generated by the included string arrays are too long
        /// </summary>
        /// <param name="check">A tuple of string arrays</param>
        /// <returns>True if no lines generated by the included string arrays are too long</returns>
        private static bool CheckStringsLength((string[] labelMember, string[] valueMember) check)
        {
            return !check.labelMember.Where((t, i) => t.Length + check.valueMember[i].Length >= TicketCreatorHelper.MaxCharPerLine).Any();
        }

        /// <summary>
        ///     Break a string up to so that it might fit on a line with the other string
        /// </summary>
        /// <param name="breakString">String to break apart.</param>
        /// <param name="fixedLengthStrings">Strings that will not be broken apart.</param>
        /// <returns></returns>
        private static (string[] brokenList, string[] fixedList) BreakupStrings(string breakString, IEnumerable<string> fixedLengthStrings)
        {
            var reconstitutedStrings = new List<string> { string.Empty };
            var fixedStrings = fixedLengthStrings.ToList();
            var brokenStrings = breakString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var i = 0;
            var firstWord = true;
            foreach (var b in brokenStrings)
            {
                if (firstWord || reconstitutedStrings[i].Length + b.Length +
                    (fixedStrings.Count > i ? fixedStrings[i].Length : 0) < TicketCreatorHelper.MaxCharPerLine)
                {
                    reconstitutedStrings[i] += b + ' ';
                }
                else
                {
                    reconstitutedStrings[i] = reconstitutedStrings[i].Trim();
                    if (reconstitutedStrings.Count == i + 1)
                    {
                        reconstitutedStrings.Add(string.Empty);
                        fixedStrings.Add(string.Empty);
                    }

                    i++;
                    reconstitutedStrings[i] += b + ' ';
                }

                firstWord = false;
            }

            reconstitutedStrings[i] = reconstitutedStrings[i].Trim();

            return (brokenList: reconstitutedStrings.ToArray(), fixedList: fixedStrings.ToArray());
        }
    }
}