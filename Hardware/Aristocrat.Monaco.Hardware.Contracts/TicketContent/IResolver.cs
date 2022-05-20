namespace Aristocrat.Monaco.Hardware.Contracts.TicketContent
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Ticket;

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
    public interface IResolver
    {
        /// <summary>Gets the printable regions.</summary>
        Dictionary<int, PrintableRegion> PrintableRegions { get; }

        /// <summary>Gets the printable regions.</summary>
        Dictionary<string, PrintableTemplate> PrintableTemplates { get; }

        /// <summary>Gets a list of a list of printable regions for each page.</summary>
        Collection<List<PrintableRegion>> Pages { get; }

        /// <summary>Loads printable region information from printable_regions.xml configuration file.</summary>
        /// <param name="fileName">The file name.</param>
        void LoadRegions(string fileName);

        /// <summary>Loads printable template information from printable_templates.xml configuration.</summary>
        /// <param name="fileName">The file Name.</param>
        void LoadTemplates(string fileName);

        /// <summary>Resolves ticket content. It basically builds up the pages which needs to be printed.</summary>
        /// <param name="ticket">The ticket to resolve.</param>
        void Resolve(Ticket ticket);

        /// <summary>
        ///     Adds the printable region.
        /// </summary>
        /// <param name="region">The region.</param>
        void AddPrintableRegion(PrintableRegion region);

        /// <summary>
        ///     Adds the printable template.
        /// </summary>
        /// <param name="template">The template.</param>
        void AddPrintableTemplate(PrintableTemplate template);
    }
}