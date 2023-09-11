namespace Aristocrat.Monaco.Application.Contracts.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using ConfigWizard;
    using Extensions;
    using Hardware.Contracts;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using Localization;
    using Monaco.Localization.Properties;

    /// <summary>
    /// </summary>
    public abstract class TextTicket
    {
        /// <summary>
        ///     The number of header/footer lines for a text ticket.
        ///     Ideally we would calculate this from the actual lines added
        ///     but we need the number before we create the ticket headers
        /// </summary>
        public int HeaderFooterLineCount => TicketCasinoInfoLineCount + TicketHeaderLineCount + TicketFooterLineCount;

        /// <summary>
        ///     The number of lines in the ticket casino info header
        /// </summary>
        public virtual int TicketCasinoInfoLineCount => 4;

        /// <summary>
        ///     The number of lines in the ticket header
        /// </summary>
        public virtual int TicketHeaderLineCount => 11;

        /// <summary>
        ///     The number of lines in the ticket footer (currently same for all tickets)
        /// </summary>
        private const int TicketFooterLineCount = 1;

        /// <summary>
        /// </summary>
        protected ILocalizer TicketLocalizer { get; }

        /// <summary>
        ///    Create date time formatter for use in this class.
        /// </summary>
        protected readonly DateTimeFormatInfo DateTimeFormatInfo;

        /// <summary>
        ///     Gets the date format for this class
        /// </summary>
        public string DateFormat => DateTimeFormatInfo.ShortDatePattern;

        /// <summary>
        ///     Gets the date and time format for this class
        /// </summary>
        public string DateAndTimeFormat => DateFormat + " " + TimeFormat;

        /// <summary>
        /// </summary>
        /// <param name="localizer"></param>
        protected TextTicket(ILocalizer localizer)
        {
            _leftField = new StringBuilder();
            _centerField = new StringBuilder();
            _rightField = new StringBuilder();
            ServiceManager = Kernel.ServiceManager.GetInstance();
            PropertiesManager = ServiceManager.GetService<IPropertiesManager>();
            Time = ServiceManager.GetService<ITime>();

            TicketLocalizer = PropertiesManager.GetValue(ApplicationConstants.LocalizationOperatorTicketLanguageSettingOperatorOverride, false) ?
                Localizer.For(CultureFor.Operator) :
                localizer;

            DateTimeFormatInfo = TicketLocalizer.CurrentCulture.DateTimeFormat;

            StateText = new Dictionary<PaperStates, Func<string>> // lazily evaluate these values to account for localization
            {
                { PaperStates.Full, () => TicketLocalizer.GetString(ResourceKeys.PaperFullText)},
                { PaperStates.Low, () => TicketLocalizer.GetString(ResourceKeys.PaperLowText)},
                { PaperStates.Empty, () => TicketLocalizer.GetString(ResourceKeys.PaperOutText)},
                { PaperStates.Jammed, () => TicketLocalizer.GetString(ResourceKeys.PaperJamText)}
            };
        }

        /// <summary>
        /// </summary>
        public enum CurrencyType
        {
            /// <summary>
            /// </summary>
            Bill,

            /// <summary>
            /// </summary>
            Coin
        }

        // Ticket type
        private const string TicketType = "text";

        /// <summary>
        ///     Line separator
        /// </summary>
        public const string Dashes = TicketConstants.Dashes;

        // Currency, Date and time format strings
        /// <summary>
        /// </summary>
        public static readonly string TimeFormat = ApplicationConstants.DefaultTimeFormat;

        private readonly Dictionary<PaperStates, Func<string>> StateText;

        private readonly StringBuilder _centerField;
        private readonly StringBuilder _leftField;
        private readonly StringBuilder _rightField;

        /// <summary>
        /// </summary>
        public int ItemsPerPage;

        /// <summary>
        /// </summary>
        public int PageNumber;

        /// <summary>
        /// </summary>
        public IPropertiesManager PropertiesManager;

        /// <summary>
        /// </summary>
        public IServiceManager ServiceManager;

        /// <summary>
        /// </summary>
        public ITime Time;

        /// <summary>
        /// </summary>
        public bool RetailerOverride;

        /// <summary>
        ///     ticket title
        /// </summary>
        public string Title { get; protected set; }

        /// <summary>
        ///     Creates a text ticket
        /// </summary>
        /// <returns>A Ticket</returns>
        public virtual Ticket CreateTextTicket()
        {
            AddCasinoInfo();
            AddTicketHeader();
            AddTicketContent();

            var ticket = CreateTicket(Title);
            return ticket;
        }

        /// <summary>
        ///     Creates a extra page.
        /// </summary>
        /// <returns>A Ticket</returns>
        public virtual Ticket CreateSecondPageTextTicket()
        {
            AddTicketContent();

            var ticket = CreateTicket(Title);
            return ticket;
        }

        /// <summary>
        ///     If this is overridden, update the TicketHeaderLineCount
        /// </summary>
        public virtual void AddTicketHeader()
        {
            // NOTE: If additional lines are added here, update the TicketHeaderLineCount
            using (var scope = TicketLocalizer.NewScope())
            {
                // Transform and create fields needed by template regions
                if (ConfigWizardUtil.VisibleByConfig(PropertiesManager, ApplicationConstants.ConfigWizardIdentityPageZoneOverride))
                {
                    AddLine(
                        $"{(!RetailerOverride ? scope.GetString(ResourceKeys.LicenseText) : scope.GetString(ResourceKeys.RetailerText))}:",
                        null,
                        string.Format(
                            TicketLocalizer.CurrentCulture,
                            "{0}",
                            PropertiesManager.GetProperty(ApplicationConstants.Zone, scope.GetString(ResourceKeys.DataUnavailable))));
                }

                var now = ServiceManager.GetService<ITime>().GetLocationTime(DateTime.UtcNow);
                AddLine(
                    $"{scope.GetString(ResourceKeys.DateText)}: " + now.ToString(DateFormat),
                    null,
                    $"{scope.GetString(ResourceKeys.TimeText)}: " + now.ToString(TimeFormat)
                );
            }

            AddTicketHeaderCommonPart();

        }

        /// <summary>
        /// </summary>
        public void AddTicketHeaderCommonPart()
        {
            // NOTE: If additional lines are added here, update the TicketHeaderLineCount
            using (var scope = TicketLocalizer.NewScope())
            {
                AddLine(
                    $"{scope.GetString(ResourceKeys.MacAddressLabel)}:",
                    null,
                    string.Format(
                        TicketLocalizer.CurrentCulture,
                        "{0}",
                        MacAddressWithColon(NetworkInterfaceInfo.DefaultPhysicalAddress)));

                AddLine(
                    $"{scope.GetString(ResourceKeys.IPAddressesLabel)}:",
                    null,
                    string.Format(
                        TicketLocalizer.CurrentCulture,
                        "{0}",
                        NetworkInterfaceInfo.DefaultIpAddress));

                var ioService = ServiceManager.GetService<IIO>();

                AddLine(
                    $"{scope.GetString(ResourceKeys.ManufacturerLabel)}:",
                    null,
                    string.Format(
                        TicketLocalizer.CurrentCulture,
                        "{0}",
                        ioService.DeviceConfiguration.Manufacturer));

                AddLine(
                    $"{scope.GetString(ResourceKeys.ModelLabel)}:",
                    null,
                    string.Format(
                        TicketLocalizer.CurrentCulture,
                        "{0}",
                        ioService.DeviceConfiguration.Model));


                var machineId = PropertiesManager.GetValue(ApplicationConstants.MachineId, (uint)0);
                var assetNumber = string.Empty;
                if (machineId != 0)
                {
                    assetNumber = machineId.ToString();
                }

                AddLine(
                    $"{scope.GetString(ResourceKeys.AssetNumber)}:",
                    null,
                    string.Format(
                        TicketLocalizer.CurrentCulture,
                        "{0}",
                        assetNumber)); //VignetteText

                AddLine(
                    $"{scope.GetString(ResourceKeys.SerialNumberLabel)}:",
                    null,
                    string.Format(
                        TicketLocalizer.CurrentCulture,
                        "{0}",
                        PropertiesManager.GetValue(ApplicationConstants.SerialNumber, scope.GetString(ResourceKeys.DataUnavailable))));

                AddLine(
                    $"{scope.GetString(ResourceKeys.OSImageVersionLabel)}:",
                    null,
                    string.Format(
                        TicketLocalizer.CurrentCulture,
                        "{0}",
                        ServiceManager.TryGetService<IOSService>()?.OsImageVersion.ToString() ?? scope.GetString(ResourceKeys.DataUnavailable)));

                AddLine(
                    $"{scope.GetString(ResourceKeys.PlatformVersionText)}:",
                    null,
                    string.Format(
                        TicketLocalizer.CurrentCulture,
                        "{0}",
                        PropertiesManager.GetValue(KernelConstants.SystemVersion, scope.GetString(ResourceKeys.DataUnavailable))));

                AddLine(
                  $"{scope.GetString(ResourceKeys.Currency)}:",
                  null,
                  string.Format(
                      TicketLocalizer.CurrentCulture,
                      "{0}",
                      CurrencyExtensions.Currency.CurrencyName));
            }
        }

        /// <summary>
        /// </summary>
        public abstract void AddTicketContent();

        /// <summary>
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public virtual Ticket CreateTicket(string title)
        {
            var ticket = new Ticket
            {
                [TicketConstants.TicketType] = TicketType,
                [TicketConstants.Title] = string.IsNullOrEmpty(title) ? Title : title,
                [TicketConstants.Left] = _leftField.ToString(),
                [TicketConstants.Center] = _centerField.ToString(),
                [TicketConstants.Right] = _rightField.ToString()
            };
            return ticket;
        }

        /// <summary>
        /// </summary>
        public virtual void AddCasinoInfo()
        {
            // NOTE: If additional lines are added here, update the TicketCasinoInfoLineCount
            AddEmptyLines();
            AddLine(
                null,
                (string)PropertiesManager.GetProperty(PropertyKey.TicketTextLine1, string.Empty),
                null);
            AddLine(
                null,
                (string)PropertiesManager.GetProperty(PropertyKey.TicketTextLine2, string.Empty),
                null);
            AddLine(
                null,
                (string)PropertiesManager.GetProperty(PropertyKey.TicketTextLine3, string.Empty),
                null);
        }

        /// <summary>
        /// </summary>
        public void AddTicketFooter()
        {
            // NOTE: If additional lines are added here, update the TicketFooterLineCount
            var printer = ServiceManager.TryGetService<IPrinter>();
            var state = printer?.PaperState ?? PaperStates.Empty;
            StateText.TryGetValue(state, out var stateTextCb);
            AddLine(TicketLocalizer.GetString(ResourceKeys.PaperLevelText), null, stateTextCb());
        }

        /// <summary>
        ///     Add a line of dashes to the ticket center column
        /// </summary>
        public void AddDashesLine()
        {
            AddLine(null,
                string.Join(string.Empty, Dashes.Take(TicketCreatorHelper.MaxCharPerLine)),
                null);
        }

        /// <summary>
        ///     Add n empty lines
        /// </summary>
        /// <param name="number">Number of lines to add</param>
        public void AddEmptyLines(int number = 1)
        {
            for (var i = 0; i < number; i++)
            {
                AddLine(null, null, null);
            }
        }

        /// <summary>
        ///     Adds a "line" of data to the ticket
        /// </summary>
        /// <param name="left">The left column data</param>
        /// <param name="center">The center column data.</param>
        /// <param name="right">The right column data</param>
        public void AddLine(string left, string center, string right)
        {
            TicketCreatorHelper.AddLine(_leftField, _centerField, _rightField, left, center, right);
        }

        /// <summary>
        ///     Clears the ticket fields
        /// </summary>
        protected void ClearFields()
        {
            _leftField.Clear();
            _centerField.Clear();
            _rightField.Clear();
        }

        /// <summary>
        ///     Format Mac address like "xx:xx:xx:xx:xx:xx"
        /// </summary>
        /// <param name="address"></param>
        /// <returns>formatted Mac address</returns>
        private static string MacAddressWithColon(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return address;
            }

            var sb = new StringBuilder(address);
            var i = 2;
            while (i < sb.Length)
            {
                sb.Insert(i, ":");
                i += 3;
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Add a line with a label on the left and a value on the right
        /// </summary>
        /// <param name="label">Either use a ResourceKey or set localizeLabel to false</param>
        /// <param name="value">The value</param>
        /// <param name="localizeLabel">True if the label should be localized.</param>
        /// <param name="reformatLabelFirst">Whether or not to reformat the label part first in the event the line exceeds ticket width</param>
        public void AddLabeledLine(string label, string value, bool localizeLabel = true, bool reformatLabelFirst = false)
        {
            AddResolvedLine(FixWidth(localizeLabel ? TicketLocalizer.GetString(label) : label, value, reformatLabelFirst));
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
        /// <param name="labelFirst">Whether or not to break the label first </param>
        /// <returns>A tuple of string arrays</returns>
        private static (string[] labelMember, string[] valueMember) FixWidth(string label, string value, bool labelFirst = false)
        {
            var original = (labelMember: new[] { label }, valueMember: new[] { value });
            if (CheckStringsLength(original))
            {
                return original;
            }

            var check = labelFirst ? ReformatLabel(original.valueMember) : ReformatValue(original.labelMember);
            if (!CheckStringsLength(check))
            {
                return !labelFirst ? ReformatLabel(check.valueMember) : ReformatValue(check.labelMember);
            }
            return check;

            (string[] labelMember, string[] valueMember) ReformatLabel(string[] fixedString)
            {
                var (brokenList, fixedList) = BreakupStrings(label, fixedString);
                return (labelMember: brokenList, valueMember: fixedList);
            }

            (string[] labelMember, string[] valueMember) ReformatValue(string[] fixedString)
            {
                var (brokenList, fixedList) = BreakupStrings(value, fixedString);
                return (labelMember: fixedList, valueMember: brokenList);
            }
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
