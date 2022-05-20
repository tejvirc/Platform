namespace Aristocrat.Monaco.Gaming.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Application.Contracts.MeterPage;
    using Application.Contracts.Tickets;
    using Contracts;
    using Contracts.Meters;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using Localization.Properties;

    /// <summary>
    ///     This class creates game meters ticket objects
    /// </summary>
    public class GameMetersTicketCreator : IGameMetersTicketCreator, IService
    {
        private IServiceManager _serviceManager;
        private IPropertiesManager _propertiesManager;

        // Ticket type
        private const string TicketType = "text";

        // Date format strings
        private static readonly string DateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

        /// <inheritdoc />
        public List<Ticket> CreateGameMetersTicket(
            int gameId,
            IList<MeterNode> meterNodes,
            bool useMasterValues,
            bool onlySelected)
        {
            var games = _propertiesManager.GetValues<IGameDetail>(GamingConstants.AllGames).ToList();

            if (_propertiesManager?.GetValue(ApplicationConstants.TicketModeAuditKey, TicketModeAuditBehavior.Audit) ==
                TicketModeAuditBehavior.Inspection)
            {
                var game = games.FirstOrDefault(g => g.Id == gameId);
                return game == null ? null : new GamePerformanceMetersTicket(game).CreateAuditTickets();
            }
            else if (onlySelected)
            {
                var tickets = new List<Ticket>();

                foreach (var game in games)
                {
                    tickets.Add(CreateSingleGameMetersTicket(game, meterNodes, useMasterValues));
                }

                return tickets;
            }
            else
            {
                var game = games.FirstOrDefault(g => g.Id == gameId);
                return game == null
                    ? null
                    : new List<Ticket> { CreateSingleGameMetersTicket(game, meterNodes, useMasterValues) };
            }
        }

        private Ticket CreateSingleGameMetersTicket(
            IGameDetail game,
            IList<MeterNode> meterNodes,
            bool useMasterValues)
        {
            var meterManager = _serviceManager.GetService<IGameMeterManager>();
            var meters = new List<Tuple<IMeter, string>>();
            foreach (var meter in meterNodes)
            {
                AddMeter(meterManager, meters, game.Id, meter.Name, meter.DisplayName);
            }

            // print region string builders
            var leftBuilder = new StringBuilder();
            var centerBuilder = new StringBuilder();
            var rightBuilder = new StringBuilder();

            // Header data
            var title = useMasterValues
                ? Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.MasterGameMetersTicketTitleText)
                : Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.PeriodGameMetersTicketTitleText);
            var dateTimeNow = _serviceManager.GetService<ITime>().GetLocationTime(DateTime.UtcNow);
            var retailerName =
                (string)_propertiesManager.GetProperty(PropertyKey.TicketTextLine1, string.Empty);
            var retailerId = (string)_propertiesManager.GetProperty(ApplicationConstants.Zone, Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.DataUnavailable));
            var jurisdiction = (string)_propertiesManager.GetProperty(
                ApplicationConstants.JurisdictionKey,
                Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.DataUnavailable));
            var serialNumber = _propertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty);
            var version = (string)_propertiesManager.GetProperty(KernelConstants.SystemVersion, Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.NotSet));

            // Theme
            var gameName = game.ThemeName;

            string leftHeader = Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.Meter);
            string rightHeader = Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.Value);

            leftBuilder.AppendLine(leftHeader + "\n\n\n");
            rightBuilder.AppendLine(rightHeader + "\n\n\n");

            // Meter values and name strings
            var meterValues = new List<string>();
            var meterNames = new List<string>();
            foreach (var m in meters)
            {
                var meter = m.Item1;
                var meterValue = useMasterValues ? meter.Lifetime : meter.Period;
                var meterValueText = meter.Classification.CreateValueString(meterValue);
                meterValueText = Regex.Replace(meterValueText, @"[^\u0000-\u007F]+", " ");
                var meterName = m.Item2;
                meterValues.Add(meterValueText);
                meterNames.Add(meterName);

                // build text for left and right regions
                leftBuilder.AppendLine(meterName);
                if (meterName.Length + meterValueText.Length + 1 > TicketCreatorHelper.MaxCharPerLine)
                {
                    // Move Value to lower line if combined is too long
                    leftBuilder.AppendLine();
                    rightBuilder.AppendLine();
                }
                rightBuilder.AppendLine(meterValueText);
            }

            // Fill in the ticket
            var ticket = new Ticket
            {
                ["ticket type"] = TicketType,
                ["title"] = title.ToUpper(CultureInfo.CurrentCulture),
                ["retailer_name"] = retailerName.ToUpper(CultureInfo.CurrentCulture),
                ["retailer_id"] = retailerId.ToUpper(CultureInfo.CurrentCulture),
                ["jurisdiction"] = jurisdiction.ToUpper(CultureInfo.CurrentCulture),
                ["serial_number_header"] = Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.SerialNumberText) + ":",
                ["serial_number"] = serialNumber,
                ["version"] = version,
                ["date_header"] = Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.DateText) + ":",
                ["date"] = dateTimeNow.ToString(DateFormat),
                ["time_header"] = Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.TimeText) + ":",
                ["time"] = dateTimeNow.ToString(ApplicationConstants.DefaultTimeFormat),
                ["game_name"] = gameName,
                ["game_id_header"] = Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.GameMetersTicketGameIdText) + ":",
                ["game_id"] = game.Id.ToString(CultureInfo.CurrentCulture)
            };

            ticket.AddFields("meter_name", meterNames);
            ticket.AddFields("meter_value", meterValues);

            // build header text for center region
            centerBuilder.AppendLine("\n\n");
            centerBuilder.AppendLine(ticket["retailer_name"]);
            centerBuilder.AppendLine(ticket["retailer_id"]);
            centerBuilder.AppendLine(ticket["jurisdiction"]);
            centerBuilder.AppendLine($"{ticket["serial_number_header"]} {ticket["serial_number"]}");
            centerBuilder.AppendLine(ticket["version"]);
            centerBuilder.AppendLine("\n\n");
            centerBuilder.AppendLine(ticket["date"]);
            centerBuilder.AppendLine(ticket["time"]);

            centerBuilder.AppendLine("\n");
            centerBuilder.AppendLine(new string('-', 87));
            centerBuilder.AppendLine(ticket["game_name"]);
            centerBuilder.AppendLine($"{ticket["game_id_header"]} {ticket["game_id"]}");

            var newlines = centerBuilder.ToString().Count(s => s == '\n') - 4;
            var lines = new string('\n', newlines);
            leftBuilder.Insert(0, lines);
            rightBuilder.Insert(0, lines);

            // fill fields needed by the printable template
            ticket["left"] = leftBuilder.ToString();
            ticket["right"] = rightBuilder.ToString();
            ticket["center"] = centerBuilder.ToString();

            return ticket;
        }

        /// <inheritdoc />
        public string Name => "Game Meters Ticket Creator";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IGameMetersTicketCreator) };

        /// <inheritdoc />
        public void Initialize()
        {
            _serviceManager = ServiceManager.GetInstance();
            _propertiesManager = _serviceManager.GetService<IPropertiesManager>();
        }

        private void AddMeter(
            IGameMeterManager meterManager,
            List<Tuple<IMeter, string>> meters,
            int gameId,
            string meterName,
            string displayName)
        {
            if (meterManager.IsMeterProvided(gameId, meterName))
            {
                meters.Add(new Tuple<IMeter, string>(meterManager.GetMeter(gameId, meterName), displayName));
            }
        }
    }
}
