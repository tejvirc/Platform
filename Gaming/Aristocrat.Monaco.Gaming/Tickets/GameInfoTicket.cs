﻿namespace Aristocrat.Monaco.Gaming.Tickets
{
    using System.Collections.Generic;
    using Application.Contracts.Localization;
    using Application.Contracts.Tickets;
    using Common;
    using Contracts.Models;
    using Hardware.Contracts.Printer;
    using Localization.Properties;

    public class GameInfoTicket : TextTicket
    {
        private readonly List<GameOrderData> _games;

        public GameInfoTicket()
            : this(null, 0)
        {
        }

        public GameInfoTicket(List<GameOrderData> games, int page)
            : base(Localizer.For(CultureFor.PlayerTicket))
        {
            _games = games;

            Title = $"{TicketLocalizer.GetString(ResourceKeys.GameInfoTicketTitle)} - {TicketLocalizer.GetString(ResourceKeys.PageText)} {page}";
        }

        public override void AddTicketContent()
        {
            AddGamesInfo();
        }

        private void AddGamesInfo()
        {
            AddLine(null, Dashes, null);
            AddLine(TicketLocalizer.GetString(ResourceKeys.ComponentText), null, TicketLocalizer.GetString(ResourceKeys.VersionText));
            var lineLength = ServiceManager.GetService<IPrinter>().GetCharactersPerLine(false, 0);
            var gameNamePaddingLength = $"{TicketLocalizer.GetString(ResourceKeys.GameText)} - ".Length; // for padding the game name in case game name takes more then 1 line.

            foreach (var game in _games)
            {
                var availableSpaceForGameName = lineLength - ($"{game.Version}".Length + gameNamePaddingLength); //find available length for game name.
                var words = game.ThemeName.ConvertStringToWrappedWords(availableSpaceForGameName);

                if (string.IsNullOrEmpty(words[1]))
                {
                    AddLine($"{TicketLocalizer.GetString(ResourceKeys.GameText)} - {words[0]}", null, $"{game.Version}");
                }
                else
                {
                    var wordsCount = words.Count;
                    for (int i = 0; i <= wordsCount - 1; i++)
                    {
                        var gameNameLengthWithPadding = gameNamePaddingLength + words[i].Length;

                        if (i == 0)
                        {
                            AddLine($"{TicketLocalizer.GetString(ResourceKeys.GameText)} - {words[i]}", null, null);
                        }
                        else if (i == wordsCount - 1)
                        {
                            AddLine($"{words[i]}".PadLeft(gameNameLengthWithPadding, ' '), null, $"{game.Version}");
                        }
                        else
                        {
                            AddLine($"{words[i]}".PadLeft(gameNameLengthWithPadding, ' '), null, null);
                        }
                    }
                }
            }

            AddLine(null, Dashes, null);
            AddTicketFooter();
        }
    }
}
