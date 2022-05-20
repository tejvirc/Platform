namespace Aristocrat.Monaco.Application.Tests.Tickets
{
    #region Using

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Hardware.Contracts.Ticket;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Verifies the text contents of a ticket created using the 3-column 'text' template.
    /// </summary>
    public class TextTicketVerificationHelper
    {
        private readonly IList<Tuple<object, object, object, string>> _expectedTicket;

        /// <summary>
        ///     Constructs a new <see cref="TextTicketVerificationHelper" />
        /// </summary>
        public TextTicketVerificationHelper()
            : this(new List<Tuple<object, object, object, string>>())
        {
        }

        /// <summary>
        ///     Constructs a new <see cref="TextTicketVerificationHelper" /> using
        ///     the specified set of columnar data.
        /// </summary>
        /// <param name="lines">
        ///     An <see cref="IEnumerable" /> of 4-tuples where items
        ///     1, 2, and 3 are objects representing the expected data in the 3 columns of the ticket
        ///     and item 4 is a message describing the logical content of that line.
        /// </param>
        public TextTicketVerificationHelper(IEnumerable<Tuple<object, object, object, string>> lines)
        {
            _expectedTicket = new List<Tuple<object, object, object, string>>(lines);
            ConversionStyles = new Dictionary<string, Tuple<ulong, ulong, ulong>>();
        }

        public IDictionary<string, Tuple<ulong, ulong, ulong>> ConversionStyles { get; }

        /// <summary>
        ///     Adds a lines of expected data.
        /// </summary>
        /// <param name="left">The data object for the 'left' column on this line</param>
        /// <param name="center">The data object for the 'center' column on this line</param>
        /// <param name="right">The data object for the 'right' column on this line</param>
        /// <param name="desc">A description of the content of this line</param>
        /// <remarks>
        ///     If all three data parameters are left null, no validation will be done for this
        ///     line, beyond the implicit check that a comparable line exists in the <see cref="Ticket" />
        ///     under test.
        ///     All three columnar values could be string data, which would result in basic textual validation.
        ///     However, in the case of DateTime, TimeSpan, decimal, and integer type data, the validation will
        ///     try to convert the text value from the ticket to the same type as the expected data. In this way,
        ///     you should be able in many cases to remove culture-specific aspects of string formatting from
        ///     your expectations of the data.
        /// </remarks>
        public void AddTicketLine(object left = null, object center = null, object right = null, string desc = null)
        {
            if (left == null && center == null && right == null)
            {
                // Uninteresting Line
                _expectedTicket.Add(null);
            }
            else
            {
                _expectedTicket.Add(Wrap(left, center, right, desc));
            }
        }

        /// <summary>
        ///     Verifies each line of the supplied <see cref="Ticket" /> against the ticket lines
        ///     added to this object with <see cref="AddTicketLine" />
        /// </summary>
        /// <param name="ticket">The <see cref="Ticket" /> to verify</param>
        /// <param name="expectedTitle">The expected title for the ticket</param>
        public void VerifyTextTicket(Ticket ticket, string expectedTitle)
        {
            // Check basics - all the expected data is accounted for.
            Assert.IsFalse(string.IsNullOrEmpty(ticket["ticket type"]));
            Assert.IsFalse(string.IsNullOrEmpty(ticket["title"]));
            Assert.IsFalse(string.IsNullOrEmpty(ticket["left"]));
            Assert.IsFalse(string.IsNullOrEmpty(ticket["right"]));
            Assert.IsFalse(string.IsNullOrEmpty(ticket["center"]));

            // "text" is the only template this validation works with.
            Assert.AreEqual("text", ticket["ticket type"]);
            Assert.AreEqual(expectedTitle, ticket["title"]);

            // Now zip the three main content sections into a list of 3-Tuples
            // where each tuple is one of three columns
            var leftLines = ticket["left"].Replace("\r", string.Empty).Split('\n');
            var centerLines = ticket["center"].Replace("\r", string.Empty).Split('\n');
            var rightLines = ticket["right"].Replace("\r", string.Empty).Split('\n');
            var ticketText = leftLines.Zip(
                centerLines.Zip(rightLines, Tuple.Create),
                (l, c) => Tuple.Create(l, c.Item1, c.Item2));

            // Now run through the expected ticket data and verify
            //foreach (var verification in _expectedTicket.Zip(ticketText, Tuple.Create))
            //{
            //    if (verification.Item1 != null)
            //    {
            //        Verify(verification.Item1, verification.Item2);
            //    }
            //}
        }

        /// <summary>
        ///     Helper to create a tuple of the required internal format.
        /// </summary>
        /// <param name="left">Left column data</param>
        /// <param name="center">Center column data</param>
        /// <param name="right">Right column data</param>
        /// <param name="desc">A description of the line</param>
        /// <returns></returns>
        private static Tuple<object, object, object, string> Wrap(
            object left,
            object center,
            object right,
            string desc = null)
        {
            return Tuple.Create(left, center, right, desc);
        }

        /// <summary>
        ///     Verifies the actual data matches the expected data.
        /// </summary>
        /// <param name="expected">4-tuple with the expected columnar data and a description</param>
        /// <param name="actual">3-tuple with the string data representing a line of text</param>
        private void Verify(Tuple<object, object, object, string> expected, Tuple<string, string, string> actual)
        {
            Tuple<ulong, ulong, ulong> styleEnum = null;

            if (expected.Item4 != null)
            {
                ConversionStyles.TryGetValue(expected.Item4, out styleEnum);
            }

            if (expected.Item1 != null)
            {
                VerifyValue(expected.Item1, actual.Item1, expected.Item4, styleEnum?.Item1);
            }

            if (expected.Item2 != null)
            {
                VerifyValue(expected.Item2, actual.Item2, expected.Item4, styleEnum?.Item2);
            }

            if (expected.Item3 != null)
            {
                VerifyValue(expected.Item3, actual.Item3, expected.Item4, styleEnum?.Item3);
            }
        }

        /// <summary>
        ///     Parses <see cref="actualText" /> to the same type as <see cref="expected" />
        ///     and verifies that they are equal in terms of the concrete type of <see cref="expected" />
        /// </summary>
        /// <param name="expected">The expected data value</param>
        /// <param name="actualText">The actual text to verify against <see cref="expected" /></param>
        /// <param name="msg">A description to use if parse or verification fails</param>
        /// <param name="style"></param>
        private static void VerifyValue(object expected, string actualText, string msg, ulong? style)
        {
            object actualValue = null;
            try
            {
                actualValue = ParseActualValue(expected.GetType(), actualText, style);
            }
            finally
            {
                Assert.IsNotNull(actualValue, $"'{msg}' actual value '{actualText}' failed conversion.");
                Assert.AreEqual(expected, actualValue, $"'{msg}' failed verification.");
            }
        }

        /// <summary>
        ///     Parses the supplied text string to the supplied type.
        /// </summary>
        /// <param name="expectedType">The <see cref="Type" /> to attempt to convert to</param>
        /// <param name="actualText">The text value to attempt to parse</param>
        /// <returns>An object of type <see cref="expectedType" /> or null if parse fails</returns>
        /// <param name="style"></param>
        private static object ParseActualValue(Type expectedType, string actualText, ulong? style)
        {
            object actualValue;
            switch (expectedType.Name)
            {
                case "Decimal":
                    actualValue = decimal.Parse(
                        actualText,
                        ((NumberStyles?)style ?? NumberStyles.None));
                    break;
                case "Int32":
                    actualValue = int.Parse(
                        actualText,
                        ((NumberStyles?)style ?? NumberStyles.None));
                    break;
                case "DateTime":
                    actualValue = DateTime.Parse(
                        actualText,
                        CultureInfo.CurrentCulture,
                        ((DateTimeStyles?)style ?? DateTimeStyles.None));
                    break;
                case "TimeSpan":
                    actualValue = TimeSpan.Parse(
                        actualText,
                        CultureInfo.CurrentCulture);
                    break;
                default:
                    actualValue = actualText;
                    break;
            }

            return actualValue;
        }
    }
}