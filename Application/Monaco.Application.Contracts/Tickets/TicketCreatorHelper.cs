namespace Aristocrat.Monaco.Application.Contracts.Tickets
{
    using System;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///     Helper methods for printed ticket creators
    /// </summary>
    public static class TicketCreatorHelper
    {
        private const int DefaultCharPerLine = 49;

        /// <summary>
        ///     Maximum number of characters per ticket line
        /// </summary>
        public static int MaxCharPerLine =>
            //ServiceManager.GetInstance().TryGetService<IPrinter>()?.GetCharactersPerLine(false, 0) ??
            DefaultCharPerLine;
        // TODO This method does not do what it is supposed to do apparently so for now just use the default const

        /// <summary>
        ///     Wrap printed line onto multiple lines if needed
        /// </summary>
        public static Tuple<string, int> WrapLine(string txt)
        {
            if (string.IsNullOrEmpty(txt) || MaxCharPerLine <= 0)
                return new Tuple<string, int>(txt, 0);

            var str = string.Empty;
            var temp = string.Empty;
            var addedLineFeeds = 0;
            var words = txt.Split(' ');

            foreach (string word in words)
            {
                temp += $"{word} ";
                if (temp.Length > MaxCharPerLine)
                {
                    temp = $"   {word} ";
                    str += "\n" + temp;
                    addedLineFeeds++;
                }
                else
                {
                    str += $"{word} ";
                }
            }

            return new Tuple<string, int>(str.Trim(), addedLineFeeds);
        }

        /// <summary>
        ///     Add ticket text to the string builders
        /// </summary>
        /// <param name="leftField">Left string builder to add to</param>
        /// <param name="centerField">Center string builder to add to</param>
        /// <param name="rightField">Right string builder to add to</param>
        /// <param name="left">Left text to add</param>
        /// <param name="center">Center text to add</param>
        /// <param name="right">Right text to add</param>
        public static void AddLine(StringBuilder leftField, StringBuilder centerField, StringBuilder rightField, string left, string center, string right)
        {
            if (centerField != null && !string.IsNullOrWhiteSpace(center) && center.Any(c => c != '-') &&
                left?.Length + center.Length + right?.Length + 2 > TicketCreatorHelper.MaxCharPerLine)
            {
                // If there is a center string that isn't only dashes and the full line (+2 to account for spaces between)
                // is too long, the center string should be on the top line and left/right below
                leftField?.AppendLine();
                leftField?.AppendLine(left);

                centerField.AppendLine(center);
                centerField.AppendLine();

                rightField?.AppendLine();
                rightField?.AppendLine(right);
            }
            else if (string.IsNullOrWhiteSpace(center) &&
                     left?.Length + right?.Length + 1 > TicketCreatorHelper.MaxCharPerLine)
            {
                // If left+right (+1 to account for space between) is too long, move right one line below left
                leftField?.AppendLine(left);
                leftField?.AppendLine();

                rightField?.AppendLine();
                rightField?.AppendLine(right);
            }
            else
            {
                leftField?.AppendLine(left);
                centerField?.AppendLine(center);
                rightField?.AppendLine(right);
            }
        }
    }
}
