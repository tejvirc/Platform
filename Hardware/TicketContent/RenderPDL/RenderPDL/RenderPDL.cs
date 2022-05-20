namespace Vgt.Client12.Hardware.TicketContent
{
    using System;
    using System.Reflection;
    using Aristocrat.Monaco.Hardware.Contracts.Ticket;
    using Aristocrat.Monaco.Hardware.Contracts.TicketContent;
    using log4net;

    /// <summary>
    ///     GDS printer rendering engine.
    ///     Handles specific functionality (aligning the text and translating the position) related to
    ///     the GDS printer which uses PDL.
    /// </summary>
    public class RenderPdl : IRenderFactory, ITemplateRenderer
    {
        private const int LineWidth = 45;

        /// <summary>Create a logger for use in this class.</summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>Initializes a new instance of the <see cref="RenderPdl" /> class. </summary>
        public RenderPdl()
        {
            // Set factory key for this implementation.
            FactoryProductKey = "PDL";
        }

        /// <summary>Gets or sets the factory product key.</summary>
        /// <returns>Factory product key object.</returns>
        public object FactoryProductKey { get; protected set; }

        /// <inheritdoc />
        public bool RenderTicket(Ticket ticket, IResolver resolver, out string printCommand, bool adjustTextTicketTitle = false)
        {
            if (ticket == null || resolver?.PrintableRegions == null || resolver.PrintableTemplates == null)
            {
                printCommand = string.Empty;
                return false;
            }

            var template = resolver.PrintableTemplates[ticket["ticket type"]];
            if (template == null)
            {
                printCommand = string.Empty;
                return false;
            }

            var templateId = template.Id.ToString();

            var data = "<PT id=\"" + templateId + "\">";
            var fieldOfInterestSet = false;
            // add content for each dynamic region
            foreach (var regionId in template.Regions)
            {
                var region = resolver.PrintableRegions[regionId];
                var fieldName = region.Property;

                // TODO: if later xml schema supports transform, then support that here
                if (string.IsNullOrEmpty(fieldName))
                {
                    // Add empty string for static content that's already defined in the region
                    data += "<D></D>";
                }
                else
                {
                    if (!fieldOfInterestSet && (fieldName.Equals("validation number") || fieldName.Equals("barcode") || fieldName.Equals("validation number alt")))
                    {
                        // Add content for this ticket field name with field of interest "1" identifier.
                        data += "<D pRoI=\"1\">" + ticket[fieldName] + "</D>";
                        fieldOfInterestSet = true;
                    }
                    else
                    {
                        var ticketField = ticket[fieldName];
                        if (string.IsNullOrEmpty(ticketField))
                        {
                            data += "<D></D>";
                        }
                        else
                        {
                            if (adjustTextTicketTitle && fieldName.Equals("title"))
                            {
                                var titleLen = LineWidth - ticketField.Length;
                                ticketField = Environment.NewLine + ticketField.PadLeft(
                                                  ticketField.Length + (titleLen > 2 ? titleLen / 2 : 0),
                                                  ' ');
                            }

                            // Add content for this ticket field name.
                            var field = ticketField.Replace("<", "-").Replace(">", "-");
                            data += "<D>" + field + "</D>";
                        }
                    }
                }
            }

            // Add the final delimiter
            data += "</PT>";

            printCommand = data;

            Logger.Debug($"Rendering print command: {printCommand}");

            return true;
        }
    }
}