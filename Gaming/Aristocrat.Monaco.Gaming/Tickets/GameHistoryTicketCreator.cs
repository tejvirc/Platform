namespace Aristocrat.Monaco.Gaming.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts.OperatorMenu;
    using Contracts.Models;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using log4net;

    /// <summary>
    ///     This class creates information ticket objects
    /// </summary>
    public class GameHistoryTicketCreator : IGameHistoryTicketCreator, IService
    {
        private const int Page1MaxLines = 5;
        private const int Page2MaxLines = 28;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public int EventsPerPage
        {
            get
            {
                var config = ServiceManager.GetInstance().GetService<IOperatorMenuConfiguration>();
                var showGameInfo = config.GetSetting(OperatorMenuSetting.GameHistoryViewModel, OperatorMenuSetting.PrintGameRoundInfo, false);
                return showGameInfo ? 1 : 2;
            }
        }

        public bool MultiPage(GameRoundHistoryItem item)
        {
            var formattedGameRoundDescriptionText = GetFormattedGameRoundDescriptionText(item);

            if (string.IsNullOrEmpty(formattedGameRoundDescriptionText))
            {
                return false;
            }

            var config = ServiceManager.GetInstance().GetService<IOperatorMenuConfiguration>();
            var showGameInfo = config.GetSetting(OperatorMenuSetting.GameHistoryViewModel, OperatorMenuSetting.PrintGameRoundInfo, false);

            return showGameInfo &&
                formattedGameRoundDescriptionText.Contains("\f") ||
                formattedGameRoundDescriptionText.Count(a => a == '\n') > Page1MaxLines;
        }

        public Ticket Create(int page, IList<GameRoundHistoryItem> items)
        {
            var numberOfPages = items.Count / EventsPerPage + (items.Count % EventsPerPage != 0 ? 1 : 0);

            if (page < 1 || page > numberOfPages)
            {
                Logger.Error(
                    $"Attempted to create ticket for invalid page number. Valid range for given event collection is 1 - {numberOfPages}.");
                return null;
            }

            var ticket = new GameHistoryTicket(items, EventsPerPage, page);

            return ticket.CreateTextTicket();
        }

        public List<Ticket> CreateMultiPage(GameRoundHistoryItem item)
        {
            var items = new List<GameRoundHistoryItem> { item };
            var tickets = new List<Ticket>();
            if (!MultiPage(item))
            {
                var ticket = new GameHistoryTicket(items, EventsPerPage, 1);

                ticket.CreateTextTicket();

                tickets.Add(ticket.CreateTextTicket());

                return tickets;
            }

            var completeGameRoundDescriptionText = GetFormattedGameRoundDescriptionText(item);
            var pages = completeGameRoundDescriptionText.Split('\f').ToList();
            var sectionedPages = new List<string>();

            // Make sections go to next page if too large for current page.
            // Section breaks are delimited with '\v'
            if (pages.Count > 0)
            {
                for (var i = 0; i < pages.Count; ++i)
                {
                    var sections = pages[i].Split('\v');

                    if (sections.Length > 1)
                    {
                        var thisPageSectioned = string.Empty;

                        foreach (var section in sections)
                        {
                            var sectionLengthIfAppended =
                                ((thisPageSectioned.Length > 0 ? thisPageSectioned + '\n' : "") + section)
                                .Count(c => c is '\n' or '\v');

                            var malformed = i == 0 && !sectionedPages.Any()
                                ? sectionLengthIfAppended >= Page1MaxLines - 1
                                : sectionLengthIfAppended >= Page2MaxLines - 1;

                            if (malformed)
                            {
                                sectionedPages.Add(thisPageSectioned);
                                thisPageSectioned = section;
                            }
                            else
                            {
                                if (thisPageSectioned.Length > 0)
                                {
                                    // Convert \v to \n
                                    thisPageSectioned += "\n";
                                }

                                thisPageSectioned += section;
                            }
                        }

                        sectionedPages.Add(thisPageSectioned);
                    }
                    else
                    {
                        sectionedPages.Add(pages[i]);
                    }
                }
            }
            
            var pageLines = new List<List<string>>();
            if (sectionedPages.Count > 1)
            {
                var malformed = false;
                for (var i = 0; i < sectionedPages.Count; ++i)
                {
                    var gameRoundLines = sectionedPages[i].Split('\n');

                    malformed = i == 0 ?
                        gameRoundLines.Length >= Page1MaxLines :
                        gameRoundLines.Length >= Page2MaxLines;

                    if (malformed)
                    {
                        break;
                    }

                    pageLines.Add(new List<string>(gameRoundLines));
                }

                if (!malformed)
                {
                    CreateTickets();
                    return tickets;
                }

                pageLines.Clear();
            }

            var lines = completeGameRoundDescriptionText.Split('\f', '\n', '\v');

            var page = new List<string>();

            foreach (var line in lines)
            {
                page.Add(line);

                if (page.Count >= Page1MaxLines && pageLines.Count == 0 || page.Count >= Page2MaxLines)
                {
                    pageLines.Add(page);
                    page = new List<string>();
                }
            }

            if (page.Count > 0)
            {
                pageLines.Add(page);
            }

            CreateTickets();

            void CreateTickets()
            {
                var ticket = new GameHistoryTicket(items, EventsPerPage, 1, pageLines.Count, pageLines[0]);

                tickets.Add(ticket.CreateTextTicket());

                for (var i = 1; i < pageLines.Count; ++i)
                {
                    ticket = new GameHistoryTicket(items, EventsPerPage, i + 1, pageLines.Count, pageLines[i]);

                    tickets.Add(ticket.CreateSecondPageTextTicket());
                }
            }

            return tickets;
        }

        /// <summary>
        ///     Gets the name of the service
        /// </summary>
        public string Name => "Game History Ticket Creator";

        /// <summary>
        ///     Gets the interface this service implements and exposes as a service
        /// </summary>
        public ICollection<Type> ServiceTypes => new[] { typeof(IGameHistoryTicketCreator) };

        /// <summary>
        ///     Initializes the service
        /// </summary>
        public void Initialize()
        {
        }

        private static string GetFormattedGameRoundDescriptionText(GameRoundHistoryItem item)
        {
            var formatter = ServiceManager.GetInstance().TryGetService<IGameRoundPrintFormatter>();
            return item is null
                ? string.Empty
                : item.GameRoundDescriptionText + formatter?.GetFormattedData(item.LogSequence);
        }
    }
}
