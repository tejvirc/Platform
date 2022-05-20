namespace Aristocrat.Monaco.Hardware.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Xml.Serialization;
    using Contracts;
    using Contracts.Printer;
    using Contracts.Ticket;
    using Contracts.TicketContent;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Ticket content resolving engine.
    ///     A print resolving engine is the one which prepares the instruction to print content using a specified template.
    ///     A print resolving engine will contain a reference to the template and the content (data) to be printed in each
    ///     printable region.
    ///     <para>
    ///         It gets the Template and corresponding regions.
    ///         After that it fills in the blanks i.e. basically replaces the variables with the actual printing data and
    ///         prepares
    ///         the pages.
    ///     </para>
    /// </summary>
    public class Resolver : IResolver
    {
        /// <summary>
        ///     Create a logger for use in this class.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>List of all regions available via configuration file.</summary>
        private readonly Dictionary<int, PrintableRegion> _printableRegions = new Dictionary<int, PrintableRegion>();

        /// <summary>List of all templates available via configuration file.</summary>
        private readonly Dictionary<string, PrintableTemplate> _printableTemplates =
            new Dictionary<string, PrintableTemplate>();

        /// <summary>Initializes a new instance of the <see cref="Resolver" /> class.</summary>
        public Resolver()
        {
            Pages = new Collection<List<PrintableRegion>>();
        }

        /// <summary>Gets the printable regions.</summary>
        public Dictionary<int, PrintableRegion> PrintableRegions => _printableRegions;

        /// <summary>Gets the printable regions.</summary>
        public Dictionary<string, PrintableTemplate> PrintableTemplates => _printableTemplates;

        /// <summary>Gets a list of a list of printable regions for each page.</summary>
        public Collection<List<PrintableRegion>> Pages { get; }

        /// <summary>Loads printable region information from printable_regions.xml configuration file.</summary>
        /// <param name="fileName">The file name.</param>
        public void LoadRegions(string fileName)
        {
            var xmlValidator = new XmlValidator();

            // Validate region settings against schema.
            if (!xmlValidator.Validate(fileName))
            {
                var message = fileName + " failed validation.";

                Logger.Error(message);

                throw new InvalidTicketConfigurationException(message);
            }

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var xmlSerializer = new XmlSerializer(typeof(dprcollectiontype));

                var contentsOfRegionsXml = (dprcollectiontype) xmlSerializer.Deserialize(fs);

                if (contentsOfRegionsXml == null)
                {
                    var message = fileName + " Deserialization failed.";

                    Logger.Error(message);

                    throw new InvalidTicketConfigurationException(message);
                }

                foreach (var dprElement in contentsOfRegionsXml.DPR)
                {
                    //// Copy XML entries into the region collection object.
                    var id = int.Parse(dprElement.id, CultureInfo.InvariantCulture);
                    var rotation = int.Parse(dprElement.rot, CultureInfo.InvariantCulture);
                    var defaultText = dprElement.D;

                    // make sure that there are no duplicate ids.
                    if (!_printableRegions.ContainsKey(id))
                    {
                        var region = new PrintableRegion(
                            dprElement.name,
                            dprElement.property,
                            id,
                            dprElement.x,
                            dprElement.px,
                            dprElement.y,
                            dprElement.py,
                            dprElement.dx,
                            dprElement.pdx,
                            dprElement.dy,
                            dprElement.pdy,
                            rotation,
                            dprElement.jst,
                            dprElement.type,
                            dprElement.m1,
                            dprElement.m2,
                            dprElement.attr,
                            defaultText);

                        _printableRegions.Add(id, region);
                    }
                }
            }
        }

        /// <summary>Loads printable template information from printable_templates.xml configuration.</summary>
        /// <param name="fileName">The file Name.</param>
        public void LoadTemplates(string fileName)
        {
            var xmlValidator = new XmlValidator();

            // Validate template settings against schema.
            if (!xmlValidator.Validate(fileName))
            {
                var message = fileName + " failed validation.";

                Logger.Error(message);

                throw new InvalidTicketConfigurationException(message);
            }

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var xmlSerializer = new XmlSerializer(typeof(dptcollectiontype));

                var contentsOfTemplatesXml = (dptcollectiontype) xmlSerializer.Deserialize(fs);

                if (contentsOfTemplatesXml == null)
                {
                    var message = fileName + " Deserialization failed.";

                    Logger.Error(message);

                    throw new InvalidTicketConfigurationException(message);
                }

                foreach (var dptElement in contentsOfTemplatesXml.DPT)
                {
                    var regionStrings = dptElement.Value.Trim().Split(' ');
                    var numericalRegions = new List<int>();

                    foreach (var str in regionStrings)
                    {
                        if (!string.IsNullOrEmpty(str))
                        {
                            numericalRegions.Add(int.Parse(str, CultureInfo.InvariantCulture));
                        }
                    }

                    _printableTemplates.Add(
                        dptElement.name,
                        new PrintableTemplate(
                            dptElement.name,
                            dptElement.id,
                            dptElement.t_dim_da,
                            dptElement.t_dim_pa,
                            numericalRegions));
                }
            } 
        }

        /// <summary>Resolves ticket content. It basically builds up the pages which needs to be printed.</summary>
        /// <param name="ticket">The ticket to resolve.</param>
        public void Resolve(Ticket ticket)
        {
            if (ticket == null)
            {
                const string errorMessage = "No ticket properties found.";
                Logger.Error(errorMessage);
                throw new PropertyNotFoundException(errorMessage);
            }

            var ticketType = ticket["ticket type"];

            // Make sure ticket type is set before anything else.
            if (string.IsNullOrEmpty(ticketType))
            {
                Logger.Error("Ticket Type was not set.");
                return;
            }

            // 1 - Get the template
            // 2 - retrieve all regions defined in this template
            // 3 - assign the value associated with one property if set
            try
            {
                BuildPages(ticket, _printableTemplates[ticketType]);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
                eventBus.Publish(new ResolverErrorEvent());
            }
        }

        /// <summary>
        ///     Adds the printable region.
        /// </summary>
        /// <param name="region">The region.</param>
        public void AddPrintableRegion(PrintableRegion region)
        {
            if (region == null)
            {
                throw new ArgumentNullException(nameof(region));
            }

            if (region.Id < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(region.Id));
            }

            if (_printableRegions.ContainsKey(region.Id))
            {
                _printableRegions[region.Id] = region;
            }
            else
            {
                _printableRegions.Add(region.Id, region);
            }
        }

        /// <summary>
        ///     Adds the printable template.
        /// </summary>
        /// <param name="template">The template.</param>
        public void AddPrintableTemplate(PrintableTemplate template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            if (string.IsNullOrEmpty(template.Name))
            {
                throw new ArgumentNullException(nameof(template.Name));
            }

            if (_printableTemplates.ContainsKey(template.Name))
            {
                Logger.Debug($"over writing existing template '{template.Name}' with regions '{template.Regions[0]},{template.Regions[1]},{template.Regions[2]},{template.Regions[3]}'");
                _printableTemplates[template.Name] = template;
            }
            else
            {
                Logger.Debug($"adding template '{template.Name}'");
                _printableTemplates.Add(template.Name, template);
            }
        }

        /// <summary>Associate template's regions with property values.</summary>
        /// <param name="ticket">The ticket.</param>
        /// <param name="template">The PrintableTemplate.</param>
        private void BuildPages(Ticket ticket, PrintableTemplate template)
        {
            var regions = template.Regions;
            var foundNextPage = false;
            var blankPage = true;
            if (regions == null)
            {
                const string errorMessage = "Template region list cannot be empty.";
                Logger.Error(errorMessage);
                throw new InvalidTicketPropertiesException(errorMessage);
            }

            var newPage = new List<PrintableRegion>();

            foreach (var i in regions)
            {
                if (_printableRegions[i] == null)
                {
                    var errorMessage = string.Format(CultureInfo.InvariantCulture, "Region {0} is not valid.", i);
                    Logger.Error(errorMessage);
                    throw new InvalidTicketConfigurationException(errorMessage);
                }

                var nextPage = _printableRegions[i].NextPage;
                var propertyName = _printableRegions[i].Property;
                var hasProperty = !string.IsNullOrEmpty(propertyName);

                var proceedNextPage = nextPage != -1 &&
                                      (!_printableRegions[i].IsOptionalNextPage() ||
                                       _printableRegions[i].IsOptionalNextPage() && hasProperty);

                if (proceedNextPage)
                {
                    // Add the current page (we will be updating contents as we go).
                    Pages.Add(newPage);

                    // Load the next page.
                    PrintableTemplate nextPrintableTemplate = null;

                    foreach (var printableTemplate in _printableTemplates)
                    {
                        if (printableTemplate.Value.Id == nextPage)
                        {
                            nextPrintableTemplate = printableTemplate.Value;
                            foundNextPage = true;
                        }
                    }

                    if (!foundNextPage)
                    {
                        // Could not find template needed (AND we looked through all of them now).
                        var errorMessage = string.Format(
                            CultureInfo.InvariantCulture,
                            "Template {0} could not be found.",
                            nextPage.ToString(CultureInfo.InvariantCulture));

                        Logger.Error(errorMessage);
                        throw new InvalidTicketConfigurationException(errorMessage);
                    }

                    try
                    {
                        BuildPages(ticket, nextPrintableTemplate);
                    }
                    catch (Exception)
                    {
                        Logger.Error("Failed building additional pages.");
                        throw;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(propertyName))
                    {
                        // A blank property name indicates a fixed value.  The ticket won't be blank.
                        blankPage = false;
                    }
                    else
                    {
                        // Check if region is flagged for optional print.
                        if (_printableRegions[i].IsFlagged())
                        {
                            if (ticket.IsPropertySet(propertyName))
                            {
                                bool.TryParse(ticket[propertyName], out var result);
                                if (!result)
                                {
                                    _printableRegions[i].DefaultText = string.Empty;
                                }
                                else
                                {
                                    blankPage = false;
                                }
                            }
                            else
                            {
                                _printableRegions[i].DefaultText = string.Empty;
                            }
                        }
                        else if (_printableRegions[i].GetContextValue('5', 3, out var contextValue))
                        {
                            var property = ticket[propertyName];

                            if (property.Length > contextValue)
                            {
                                _printableRegions[i].DefaultText = string.Empty;
                            }
                            else
                            {
                                _printableRegions[i].Attribute = "01";

                                if (string.IsNullOrEmpty(_printableRegions[i].DefaultText))
                                {
                                    _printableRegions[i].DefaultText = property;
                                    blankPage = false;
                                }
                            }
                        }
                        else if (_printableRegions[i].GetContextValue('4', 3, out contextValue))
                        {
                            var property = ticket[propertyName];

                            if (property.Length <= contextValue)
                            {
                                _printableRegions[i].Attribute =
                                    _printableRegions[i].DefaultText.Length != 0 ? "901" : "902";

                                _printableRegions[i].DefaultText = string.Empty;
                            }
                            else
                            {
                                _printableRegions[i].Attribute = "01";

                                if (string.IsNullOrEmpty(_printableRegions[i].DefaultText))
                                {
                                    _printableRegions[i].DefaultText = property;
                                    blankPage = false;
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(ticket[propertyName]))
                        {
                            // if the ticket[propertyName] is not null,
                            // it could be a static field.  Regardless,
                            // it contains a value we want to print.
                            _printableRegions[i].DefaultText = ticket[propertyName];
                            blankPage = false;
                        }
                        else if (_printableRegions[i].IsOptionalStaticRegion())
                        {
                            // if the printable region is optional (attr="901")
                            // or static optional(attr="902"),
                            // and the field is empty, print an empty field.
                            _printableRegions[i].DefaultText = string.Empty;
                        }
                        else
                        {
                            // This required field is empty, throw an exception.
                            var message = "Required field: " + _printableRegions[i].Name + " is empty";
                            Logger.Error(message);
                            //// this would break most current ticket printing.  Eventually we should use the throw.
                            //// For now, check the log for required fields that were left blank.
                            ////throw new InvalidTicketPropertiesException(message);
                        }
                    }

                    newPage.Add(_printableRegions[i]);
                }
            }

            if (!foundNextPage && !blankPage)
            {
                Pages.Add(newPage);
            }
        }
    }
}