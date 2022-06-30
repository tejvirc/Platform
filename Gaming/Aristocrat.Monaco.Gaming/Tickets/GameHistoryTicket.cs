namespace Aristocrat.Monaco.Gaming.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.ConfigWizard;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.Contracts.Tickets;
    using Common;
    using Contracts.Models;
    using Hardware.Contracts.Printer;
    using Localization.Properties;

    /// <summary>
    /// </summary>
    public class GameHistoryTicket : TextTicket
    {
        public const string SequenceDelimiter = "/";
        public const string EmptyEntry = " ";

        private readonly ICollection<GameRoundHistoryItem> _items;
        private readonly IList<string> _lines;
        private readonly int _page;
        private readonly int _totalPages;

        public GameHistoryTicket(IList<GameRoundHistoryItem> items, int eventsPerPage, int currentPage)
            : base(Localizer.For(CultureFor.OperatorTicket))
        {
            _items = items;
            ItemsPerPage = eventsPerPage;
            _page = currentPage;

            Title = TicketLocalizer.GetString(ResourceKeys.GameHistoryTicketTitleText);
        }

        public GameHistoryTicket(IList<GameRoundHistoryItem> items, int eventsPerPage, int page, int totalPages, IList<string> lines)
            : base(Localizer.For(CultureFor.OperatorTicket))
        {
            _items = items;
            ItemsPerPage = eventsPerPage;
            _page = page;
            _totalPages = totalPages;
            _lines = lines;

            Title = TicketLocalizer.GetString(ResourceKeys.GameHistoryTicketTitleText);
        }

        public override void AddTicketContent()
        {
            if (_items == null || _items.Count == 0)
            {
                return;
            }

            var config = ServiceManager.GetService<IOperatorMenuConfiguration>();
            var showGameInfo = config.GetSetting(OperatorMenuSetting.GameHistoryViewModel, OperatorMenuSetting.PrintGameRoundInfo, false);

            int endIndex;
            int startIndex;

            checked
            {
                startIndex = ItemsPerPage * ((_lines == null ? _page : 1) - 1);
                endIndex = Math.Min(startIndex + ItemsPerPage, _items.Count) - 1;
            }

            AddLine(EmptyEntry, Dashes, EmptyEntry);
            var lineLength = ServiceManager.GetService<IPrinter>().GetCharactersPerLine(false, 0);

            for (var i = startIndex; i <= endIndex; i++)
            {
                var entry = _items.ElementAt(i);
                if (entry == null)
                {
                    continue;
                }

                if (entry.IsTransactionItem)
                {
                    AddLine(TicketLocalizer.GetString(ResourceKeys.EventNameText), entry.GameName, null);
                }
                else
                {
                    if (!string.IsNullOrEmpty(entry.RefNoText))
                    {
                        var delimiterIndex = entry.RefNoText.IndexOf(SequenceDelimiter, StringComparison.Ordinal);
                        if (delimiterIndex >= 0)
                        {
                            var seq = entry.RefNoText.Substring(0, delimiterIndex).Trim();
                            AddLine("#", seq, _lines == null ? null : TicketLocalizer.FormatString(ResourceKeys.PageNumber, _page, _totalPages));
                        }
                    }

                    if (_totalPages == 0 || _page == 1)
                    {
                        //adjust the starting position for avoiding overlap with Game Name
                        if (entry.GameName.Length > 28 && entry.GameName.Length < 48)
                        {
                            var availableSpaceForGameName = lineLength - ($"{TicketLocalizer.GetString(ResourceKeys.GameNameText)}".Length + 1);
                            var words = entry.GameName.ConvertStringToWrappedWords(availableSpaceForGameName);

                            if (string.IsNullOrEmpty(words[1]))
                            {
                                AddLine($"{TicketLocalizer.GetString(ResourceKeys.GameNameText)}", $"{words[0]}", null);
                            }
                            else
                            {
                                var wordsCount = words.Count;

                                for (var j = 0; j <= wordsCount - 1; j++)
                                {
                                    AddLine(j == 0 ? $"{TicketLocalizer.GetString(ResourceKeys.GameNameText)}" : null, $"{words[j]}", null);
                                }
                            }
                        }
                        else
                        {
                            AddLine(TicketLocalizer.GetString(ResourceKeys.GameNameText), null, entry.GameName);
                        }
                    }
                }

                if (_totalPages == 0 || _page == 1)
                {
                    AddLine(
                    TicketLocalizer.GetString(ResourceKeys.StartTimeText),
                    null,
                    Time.GetLocationTime(entry.StartTime).ToString(DateAndTimeFormat));

                    if (entry.IsTransactionItem)
                    {
                        if (entry.EndTime != DateTime.MinValue)
                        {
                            AddLine(
                                TicketLocalizer.GetString(ResourceKeys.EndTimeText),
                                null,
                                Time.GetLocationTime(entry.EndTime).ToString(DateAndTimeFormat));
                        }
                    }
                    else
                    {
                        AddLine(
                            TicketLocalizer.GetString(ResourceKeys.EndTimeText),
                            null,
                            entry.EndTime == DateTime.MinValue
                                ? string.Empty
                                : Time.GetLocationTime(entry.EndTime).ToString(DateAndTimeFormat));
                    }


                    if (entry.AmountIn.HasValue)
                    {
                        AddLine(TicketLocalizer.GetString(ResourceKeys.CashInText), null, $"{ entry.AmountIn.Value.FormattedCurrencyString()}");
                    }

                    if (entry.AmountOut.HasValue)
                    {
                        AddLine(TicketLocalizer.GetString(ResourceKeys.CashOutText), null, $"{ entry.AmountOut.Value.FormattedCurrencyString()}");
                    }

                    AddLine(TicketLocalizer.GetString(ResourceKeys.StartCashText), null, $"{ entry.StartCredits.FormattedCurrencyString()}");

                    if (entry.CreditsWagered.HasValue)
                    {
                        AddLine(TicketLocalizer.GetString(ResourceKeys.CashWageredText), null, $"{ entry.CreditsWagered.Value.FormattedCurrencyString()}");
                    }

                    if (entry.CreditsWon.HasValue)
                    {
                        AddLine(TicketLocalizer.GetString(ResourceKeys.CashWonText), null, $"{ entry.CreditsWon.Value.FormattedCurrencyString()}");
                    }

                    AddLine(TicketLocalizer.GetString(ResourceKeys.EndCashText), null, $"{ entry.EndCredits.Value.FormattedCurrencyString()}");

                    if (!string.IsNullOrEmpty(entry.Status))
                    {
                        AddLine(TicketLocalizer.GetString(ResourceKeys.StatusText), null, entry.Status);
                    }
                }

                if (showGameInfo && !entry.IsTransactionItem)
                {
                    //print game round event info
                    AddLine(null, null, null);

                    if (_totalPages == 0 || _page == 1)
                    {
                        AddLine(TicketLocalizer.GetString(ResourceKeys.GameRoundEventInfoText), null, null);
                    }

                    if (_lines == null)
                    {
                        var details = entry.GameRoundDescriptionText;
                        if (!string.IsNullOrEmpty(details))
                        {
                            var lines = details.Split('\n');
                            foreach (var line in lines)
                            {
                                if (!string.IsNullOrEmpty(line.Trim()))
                                {
                                    AddLine(line);
                                }
                            }
                        }
                        else
                        {
                            AddLine($"<{TicketLocalizer.GetString(ResourceKeys.NoEventInfoText)}>", null, null);
                        }
                    }
                    else
                    {
                        foreach (var line in _lines)
                        {
                            AddLine(line);
                        }
                    }
                }

                AddLine(EmptyEntry, Dashes, EmptyEntry);
            }

            if (endIndex == _items.Count - 1)
            {
                AddTicketFooter();
            }
        }

        /// <summary>
        /// </summary>
        public override void AddTicketHeader()
        {
            if (ConfigWizardUtil.VisibleByConfig(PropertiesManager, ApplicationConstants.ConfigWizardIdentityPageZoneOverride))
            {
                AddLine(
                    $"{TicketLocalizer.GetString(ResourceKeys.RetailerNumber)}:",
                    null,
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "{0}",
                        PropertiesManager.GetProperty(ApplicationConstants.Zone, TicketLocalizer.GetString(ResourceKeys.DataUnavailable))));
            }

            var now = ServiceManager.GetService<ITime>().GetLocationTime(DateTime.UtcNow);
            AddLine(
                $"{TicketLocalizer.GetString(ResourceKeys.DateText)}: {now.ToString(DateFormat, CultureInfo.CurrentCulture)}",
                null,
                $"{TicketLocalizer.GetString(ResourceKeys.TimeText)}: {now.ToString(TimeFormat, CultureInfo.CurrentCulture)}");

            AddTicketHeaderCommonPart();
        }

        private void AddLine(string line)
        {
            if (line.Contains('¢'))
            {
                var line1 = line.Replace("¢", TicketLocalizer.GetString(ResourceKeys.CentText));
                AddLine(line1, null, null);
            }
            else
            {
                AddLine(line, null, null);
            }
        }
    }
}