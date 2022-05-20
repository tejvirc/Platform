namespace Aristocrat.Monaco.Application.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Contracts;
    using Contracts.ConfigWizard;
    using Contracts.Localization;
    using Contracts.Tickets;
    using Contracts.TiltLogger;
    using Kernel;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     Event log printed ticket formatter
	/// </summary>
	public class EventLogTicket : TextTicket
    {
        private ICollection<EventDescription> _events;

        /// <summary>
        ///     Constructor
        /// </summary>
        public EventLogTicket() : base(Localizer.For(CultureFor.OperatorTicket))
        {
            OperatorTicketDateFormat = PropertiesManager.GetValue(
                ApplicationConstants.LocalizationOperatorTicketDateFormat,
                ApplicationConstants.DefaultDateFormat);
        }

        private string OperatorTicketDateFormat { get; }

        private string OperatorTicketDateAndTimeFormat => $"{OperatorTicketDateFormat} {TimeFormat}";

        public void Initialize(ICollection<EventDescription> events, int eventsPerPage, int currentPage)
        {
            ClearFields();
            _events = events;
            ItemsPerPage = eventsPerPage;

            Title = $"{TicketLocalizer.GetString(ResourceKeys.EventLogTicketTitle)} - PAGE {currentPage}";
        }

        /// <summary>
        /// add ticket header
        /// </summary>
        public override void AddTicketHeader()
        {
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
                    $"{scope.GetString(ResourceKeys.DateText)}: " + now.ToString(OperatorTicketDateFormat),
                    null,
                    $"{scope.GetString(ResourceKeys.TimeText)}: " + now.ToString(TimeFormat)
                );
            }

            AddTicketHeaderCommonPart();
        }


        /// <summary>
        ///     Adds content to text ticket
        /// </summary>
        public override void AddTicketContent()
        {
            foreach (var eventDescription in _events)
            {
                var timestamp = eventDescription.Timestamp.Kind == DateTimeKind.Utc
                    ? TimeZoneInfo.ConvertTimeFromUtc(eventDescription.Timestamp, Time.TimeZoneInformation)
                    : eventDescription.Timestamp;

                AddLine(null, Dashes, null);
                AddLine(timestamp.ToString(OperatorTicketDateAndTimeFormat), null, null);
                AddDescription(eventDescription.Name);
            }

            AddLine(null, Dashes, null);
            AddTicketFooter();
        }

        private void AddDescription(string name)
        {
            if (!NeedSplitting(name))
            {
                AddLine(name, null, null);
            }
            else
            {
                var lines = SplitString(name);
                foreach (var line in lines)
                {
                    AddLine(line, null, null);
                }
            }
        }

        private static bool NeedSplitting(string description)
        {
            return description.Length > ApplicationConstants.AuditTicketMaxLineLength;
        }

        private static List<string> SplitString(string description)
        {
            var list = new List<string>();

            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            var words = description.Split(' ');

            foreach (var word in words)
            {
                if (sb1.Length + word.Length < ApplicationConstants.AuditTicketMaxLineLength)
                {
                    sb1.Append(sb1.Length < 1 ? word : " " + word);
                }
                else
                {
                    sb2.Append((sb2.Length < 1 ? word : " " + word));
                }
            }

            list.Add(sb1.ToString());
            list.Add("    " + sb2);  // indent continuation line

            return list;
        }
    }
}