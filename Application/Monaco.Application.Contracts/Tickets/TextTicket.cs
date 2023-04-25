namespace Aristocrat.Monaco.Application.Contracts.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Aristocrat.Monaco.Hardware.Contracts;
    using ConfigWizard;
    using Extensions;
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
        public virtual int TicketCasinoInfoLineCount => 3;

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
            DateTimeFormatInfo = CultureInfo.CurrentCulture.DateTimeFormat;

            TicketLocalizer = PropertiesManager.GetValue(ApplicationConstants.LocalizationOperatorTicketLanguageSettingOperatorOverride, false) ?
                Localizer.For(CultureFor.Operator) :
                localizer;

            StateText = new Dictionary<PaperStates, string>
            {
                { PaperStates.Full, TicketLocalizer.GetString(ResourceKeys.PaperFullText)},
                { PaperStates.Low, TicketLocalizer.GetString(ResourceKeys.PaperLowText)},
                { PaperStates.Empty, TicketLocalizer.GetString(ResourceKeys.PaperOutText)},
                { PaperStates.Jammed, TicketLocalizer.GetString(ResourceKeys.PaperJamText)}
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

        /// <summary>
        /// </summary>
        public readonly Dictionary<PaperStates, string> StateText;

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
                        $"{(!RetailerOverride ? scope.GetString(ResourceKeys.LicenseText) : scope.GetString(ResourceKeys.RetailerText))}",
                        null,
                        string.Format(
                            CultureInfo.CurrentCulture,
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
                        CultureInfo.CurrentCulture,
                        "{0}",
                        MacAddressWithColon(NetworkInterfaceInfo.DefaultPhysicalAddress)));

                AddLine(
                    $"{scope.GetString(ResourceKeys.IPAddressesLabel)}:",
                    null,
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "{0}",
                        NetworkInterfaceInfo.DefaultIpAddress));

                var ioService = ServiceManager.GetService<IIO>();

                AddLine(
                    $"{scope.GetString(ResourceKeys.ManufacturerLabel)}:",
                    null,
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "{0}",
                        ioService.DeviceConfiguration.Manufacturer));

                AddLine(
                    $"{scope.GetString(ResourceKeys.ModelLabel)}:",
                    null,
                    string.Format(
                        CultureInfo.CurrentCulture,
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
                        CultureInfo.CurrentCulture,
                        "{0}",
                        assetNumber)); //VignetteText

                AddLine(
                    $"{scope.GetString(ResourceKeys.SerialNumberLabel)}:",
                    null,
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "{0}",
                        PropertiesManager.GetValue(ApplicationConstants.SerialNumber, scope.GetString(ResourceKeys.DataUnavailable))));

                AddLine(
                    $"{scope.GetString(ResourceKeys.OSImageVersionText)}:",
                    null,
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "{0}",
                        ServiceManager.TryGetService<IOSService>()?.OsImageVersion.ToString() ?? scope.GetString(ResourceKeys.DataUnavailable)));

                AddLine(
                    $"{scope.GetString(ResourceKeys.PlatformVersionText)}:",
                    null,
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "{0}",
                        PropertiesManager.GetValue(KernelConstants.SystemVersion, scope.GetString(ResourceKeys.DataUnavailable))));

                AddLine(
                  $"{scope.GetString(ResourceKeys.Currency)}:",
                  null,
                  string.Format(
                      CultureInfo.CurrentCulture,
                      "{0}",
                      CurrencyExtensions.CurrencyName));
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
            StateText.TryGetValue(state, out var stateText);
            AddLine(TicketLocalizer.GetString(ResourceKeys.PaperLevelText), null, stateText);
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
        ///     Clears the ticket fields
        /// </summary>
        protected void ClearFields()
        {
            _leftField.Clear();
            _centerField.Clear();
            _rightField.Clear();
        }
    }
}
