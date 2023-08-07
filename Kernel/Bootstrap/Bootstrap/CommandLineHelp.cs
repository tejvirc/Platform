namespace Aristocrat.Monaco.Bootstrap
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Kernel;
    using log4net;
    using Mono.Addins;

    /// <summary>
    ///     Definition of the CommandLineHelp class.
    /// </summary>
    public class CommandLineHelp
    {
        private const string CommandLineArgumentsExtensionPath = "/Kernel/CommandLineArguments";
        private const int ExpectedConsoleScreenWidth = 119;
        private const int OptionFieldDisplayPadding = 4;
        private const string HeaderFormatString = "{{0,{0}}}{{1,{1}}}{{2}}";
        private const string ArgumentFormatString = "{{0,{0}}}{{1,{1}}}{{2}}";
        private const string DescriptionOnlyFormatString = "{{0,{0}}}{{1}}";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private CommandLineHelp()
        {
        }

        /// <summary>
        ///     If the user chooses to see all of the available command line arguments, this method handles
        ///     discovering them via the extension point, formatting and displaying them
        /// </summary>
        public static void DiscoverAndDisplayCommandLineArguments()
        {
            Console.WriteLine();
            Console.WriteLine(@"Note: options are specified in a key=value pair arrangement - for example: ");
            Console.WriteLine();
            Console.WriteLine(@"Available options:");
            Console.WriteLine();

            var arguments = AddinManager.GetExtensionNodes<CommandLineArgumentExtensionNode>(CommandLineArgumentsExtensionPath);
            ProcessCommandLineArgumentNodes(arguments.ToList());
        }

        private static void ProcessCommandLineArgumentNodes(IReadOnlyCollection<CommandLineArgumentExtensionNode> list)
        {
            // Find the maximum length for the argument names and valid values to use when figuring out how big the
            // columns should be for displaying the help
            var maxNameLength = 0;
            var maxValueLength = 0;
            foreach (var node in list)
            {
                Logger.DebugFormat(CultureInfo.CurrentCulture, "Found node: {0}", node.Name);

                maxNameLength = node.Name.Length > maxNameLength ? node.Name.Length : maxNameLength;

                foreach (var valueNode in MonoAddinsHelper.GetChildNodes<ArgumentValueNode>(node))
                {
                    maxValueLength = valueNode.ValidValue.Length > maxValueLength
                        ? valueNode.ValidValue.Length
                        : maxValueLength;
                }
            }

            // The remaining space left for descriptions is the overall console width minus the maximum length of the present
            // argument name, maximum length of the argument value and the padding length for each column.
            var remainingForDescription =
                ExpectedConsoleScreenWidth - (maxNameLength + maxValueLength + OptionFieldDisplayPadding * 2);

            // Create variables representing the amount of space to be used by each field,
            // with each being negative to use left justified instead of right
            var nameFieldFormatLength = -(maxNameLength + OptionFieldDisplayPadding);
            var valueFieldFormatLength = -(maxValueLength + OptionFieldDisplayPadding);

            // The header format used for the column headers and the first line of each command line argument where a name, value
            // and description are present uses the padded field length values to create left justified text
            var headerFormat = string.Format(
                CultureInfo.InvariantCulture,
                HeaderFormatString,
                nameFieldFormatLength,
                valueFieldFormatLength);

            ////"{0," + nameFieldFormatLength + "}{1, " + valueFieldFormatLength + "}{2}";
            var argumentLineFormat = string.Format(
                CultureInfo.InvariantCulture,
                ArgumentFormatString,
                nameFieldFormatLength,
                valueFieldFormatLength);

            var descriptionOnlyLineFormat = string.Format(
                CultureInfo.InvariantCulture,
                DescriptionOnlyFormatString,
                nameFieldFormatLength + valueFieldFormatLength);

            var header = string.Format(CultureInfo.InvariantCulture, headerFormat, "Name", "Option", "Description");
            Console.WriteLine(header);
            Console.WriteLine();

            // For each CommandLineArgumentExtensionNode we have to do these things:
            //
            // 1. Print a line that has the name of the argument followed by the first valid value and the first line of its description
            // 2. Then we have to iterate over any parts of the description that didn't fit on that line and put them on lines below it,
            //      aligned with the other lines of the description
            // 3. Then we have to iterate over the rest of the valid values and print each name and first line of description so that it
            //      is aligned with the previous valid value and description
            // 4. Then we have to iterate over any possible remaining lines of description and align those with the previous line
            foreach (var node in list)
            {
                var argumentValueNodes =
                    MonoAddinsHelper.GetChildNodes<ArgumentValueNode>(node);

                DisplayArguments(
                    argumentValueNodes,
                    remainingForDescription,
                    argumentLineFormat,
                    descriptionOnlyLineFormat,
                    headerFormat,
                    node);
            }
        }

        private static void DisplayArguments(
            ICollection<ArgumentValueNode> argumentValueNodes,
            int remainingForDescription,
            string argumentLineFormat,
            string descriptionOnlyLineFormat,
            string headerFormat,
            CommandLineArgumentExtensionNode node)
        {
            if (argumentValueNodes.Count == 0)
            {
                return;
            }

            // Get the first argument and treat it differently because it will share the line
            // with the name of the option that can be set
            var argumentValueNode = argumentValueNodes.ElementAt(0);
            var descriptionParts = GetDescriptionParts(argumentValueNode.Description, remainingForDescription);
            Console.WriteLine(
                headerFormat,
                node.Name,
                argumentValueNode.ValidValue,
                descriptionParts[0]);

            for (var descriptionPartsIndex = 1; descriptionPartsIndex < descriptionParts.Count; descriptionPartsIndex++)
            {
                Console.WriteLine(
                    descriptionOnlyLineFormat,
                    string.Empty,
                    descriptionParts.ElementAt(descriptionPartsIndex));
            }

            Console.WriteLine();

            for (var argumentValueNodeIndex = 1;
                argumentValueNodeIndex < argumentValueNodes.Count;
                argumentValueNodeIndex++)
            {
                argumentValueNode = argumentValueNodes.ElementAt(argumentValueNodeIndex);
                descriptionParts = GetDescriptionParts(argumentValueNode.Description, remainingForDescription);
                Console.WriteLine(argumentLineFormat, string.Empty, argumentValueNode.ValidValue, descriptionParts[0]);

                for (var descriptionPartsIndex = 1;
                    descriptionPartsIndex < descriptionParts.Count;
                    descriptionPartsIndex++)
                {
                    Console.WriteLine(
                        descriptionOnlyLineFormat,
                        string.Empty,
                        descriptionParts.ElementAt(descriptionPartsIndex));
                }
            }

            Console.WriteLine();
        }

        private static List<string> GetDescriptionParts(string description, int chunkSize)
        {
            var descriptionParts = new List<string>();
            var correction = 0;
            var index = 0;

            while (index < description.Length)
            {
                // If the current index plus the chunk size is greater than
                // the length of the description we're at the end of the line
                // and we should just consume whatever is left
                string linePart;
                if (index + chunkSize > description.Length)
                {
                    linePart = description.Substring(index);
                }
                else
                {
                    // Otherwise consume up to the chunk size
                    linePart = description.Substring(index, chunkSize);

                    // But before just taking the line split at some arbitrary place,
                    // make a reasonable attempt to keep stuff together that belongs
                    // together by looking for a space. If one isn't the last character
                    // on the line, then back up until we find one and start from that spot
                    // for the next line.
                    if (linePart[linePart.Length - 1] != ' ')
                    {
                        var lastSpace = linePart.LastIndexOf(" ", StringComparison.Ordinal);
                        if (lastSpace != -1)
                        {
                            correction = linePart.Length - lastSpace - 1;
                            linePart = linePart.Substring(0, lastSpace);
                        }
                    }
                }

                descriptionParts.Add(linePart);

                // If we split a line and it was in the middle of the word, then we backed
                // up to find the previous space and split there instead, so we didn't actually
                // want to jump ahead the full chunk size and instead the chunk size minus
                // the correction to find the previous space
                index += chunkSize - correction;
                correction = 0;
            }

            return descriptionParts;
        }
    }
}
