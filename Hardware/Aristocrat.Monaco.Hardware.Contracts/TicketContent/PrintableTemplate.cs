////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="PrintableTemplate.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 1996-2017 Video Gaming Technologies, Inc.  All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Hardware.Contracts.TicketContent
{
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    ///     <para>
    ///         PrintableTemplate is a defined page format for printer output,
    ///         consisting of associations of one or more defined printable regions.
    ///     </para>
    ///     <para>
    ///         One or more printable regions are associated with a printable template (template hereafter)
    ///         to form a complete printed output of a particular type.
    ///     </para>
    ///     <para>
    ///         A template may include text regions for a name and location, the current date and expiration,
    ///         a validation number, and a currency value. A template may also include a barcode region and graphic regions.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         PDL Command Descriptions used for the PrintableTemplate is mentioned below.
    ///         The following is the structure of the DPT command.
    ///     </para>
    ///     <see>.\Hardware\Aristocrat.Monaco.Hardware\printable_templates.xml</see>
    ///     <code><DPT id="TemplateId">idvalue1 idvalue2 ... idvaluen</DPT></code>
    ///     <para>"TemplateId" would be replaced with the appropriate printable template identifier.</para>
    ///     <para>"idvalue" values would be replaced with the associated printable region identifiers.</para>
    ///     <para>
    ///         <list type="bullet">
    ///             <item>
    ///                 <term>id</term>
    ///                 <description>
    ///                     <para>Three digit printable template identifier.</para>
    ///                     <para>Value ranges from 000 to 999.</para>
    ///                     <para>000 – 099 are reserved for predefined regions.</para>
    ///                     <para>100 – 999 reserved for downloadable use.</para>
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <term>idvalue1 idvalue2 ... idvaluen</term>
    ///                 <description>
    ///                     <para>Three digit resident printable region identifiers used in the format of this ticket.</para>
    ///                     <para>
    ///                         These fields are the method by which all printable regions used on a ticket are linked together
    ///                         to
    ///                         define the Printable Template.
    ///                     </para>
    ///                     <para>
    ///                         The order of the printable region in this list is important because print data sent with the
    ///                         DPT command
    ///                         must be in the order of printable region identifiers in this list.
    ///                     </para>
    ///                     <para>Value ranges from 000 to 999.</para>
    ///                     <para>
    ///                         The printable regions with identifiers used to define a given template must have been already
    ///                         defined via
    ///                         appropriate DPR command. If the DPT command contains unknown printable regions identifiers, the
    ///                         printer
    ///                         will report an error.
    ///                     </para>
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    public class PrintableTemplate
    {
        /// <summary>Initializes a new instance of the <see cref="PrintableTemplate" /> class.</summary>
        /// <param name="name">The template name (ticket type).</param>
        /// <param name="id">The template id.</param>
        /// <param name="da">The template dimension dot line axis.</param>
        /// <param name="pa">The template dimension paper axis.</param>
        /// <param name="regions">The list of regions.</param>
        public PrintableTemplate(string name, int id, int da, int pa, IList<int> regions)
        {
            Name = name;
            Id = id;
            Da = da;
            Pa = pa;
            Regions = regions;
        }

        /// <summary>Gets template Name.</summary>
        public string Name { get; }

        /// <summary>Gets template Id.</summary>
        public int Id { get; }

        /// <summary>Gets template template dimension dot line axis.</summary>
        public int Da { get; }

        /// <summary>Gets template dimension paper axis.</summary>
        public int Pa { get; }

        /// <summary>Gets Printable Regions.</summary>
        public IList<int> Regions { get; }

        /// <summary>
        ///     Returns the string representation of the PDL DPT command.
        /// </summary>
        /// <returns>The string representation of the PDL DPT command.</returns>
        public string ToPDL()
        {
            var part1 = string.Format(CultureInfo.InvariantCulture, "<DPT id=\"{0}\">", Id);
            var part2 = string.Empty;
            foreach (var region in Regions)
            {
                part2 += string.Format(CultureInfo.InvariantCulture, "{0} ", region);
            }

            part2 = part2.TrimEnd();
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}</DPT>", part1, part2);
        }
    }
}