namespace Aristocrat.Monaco.Gaming.Tickets
{
    using Application.Tickets;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Localization.Properties;
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Creates a text ticket displaying game value meters
    /// </summary>
    public class GameValueMeterTicket : ValueMeterTicket
    {
        private readonly IGameDetail _game;

        /// <summary>
        ///     
        /// </summary>
        /// <param name="titleOverride"></param>
        /// <param name="meters"></param>
        /// <param name="useMasterValues"></param>
        /// <param name="game"></param>
        public GameValueMeterTicket(string titleOverride, IList<Tuple<IMeter, string>> meters, bool useMasterValues, IGameDetail game)
            : base(titleOverride, useMasterValues, meters)
        {
            _game = game;
        }

        protected override void AddTicketContentHeader()
        {
            var leftHeader = TicketLocalizer.GetString(ResourceKeys.Meter);
            var rightHeader = TicketLocalizer.GetString(ResourceKeys.Value);
            var gameName = _game.ThemeName;
            var gameIdText = $"{TicketLocalizer.GetString(ResourceKeys.GameMetersTicketGameIdText)}: {_game.Id}";

            AddLine(leftHeader, null, rightHeader);

            AddDashesLine();

            AddLine(null, gameName, null);
            AddLine(null, gameIdText, null);

            AddEmptyLines();
        }
    }
}
