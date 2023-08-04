namespace Aristocrat.Monaco.Application.UI.Helpers
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Text;
    using Contracts.Extensions;
    using Contracts.Localization;
    using Hardware.Contracts.NoteAcceptor;
    using Monaco.Localization.Properties;

    [CLSCompliant(false)]
    public static class ViewModelHelper
    {
        /// <summary>
        ///     The maximum number of barcode digits to display.  If the barcode is longer,
        ///     all but this many digits will be obscured.
        /// </summary>
        private const int BarcodeDigitsToDisplay = 4;

        /// <summary>The default barcode size.</summary>
        private const int DefaultBarcodeSize = 18;

        /// <summary>
        ///     The number of barcode digits to group together, where groups are separated
        ///     by a logical separator, a hyphen for example.
        /// </summary>
        private const int BarcodeDigitGroupSize = 4;

        /// <summary>
        ///     Formats and returns the provided barcode in a manner acceptable
        ///     for displaying to users.
        /// </summary>
        /// <param name="barcode">The barcode to format</param>
        /// <returns>
        ///     The provided barcode in a manner acceptable for displaying to users.
        /// </returns>
        /// <remarks>
        ///     The displayed barcode should have the following properties
        ///     1) Separated into groups of digits
        ///     2) All but the last n digits are hidden
        ///     Example 18 digit barcode with group size=4 and 4 digits visible:
        ///     XX-XXXX-XXXX-XXXX-4852
        /// </remarks>
        public static string GetFormattedBarcode(this string barcode)
        {
            if (barcode == null)
            {
                return null;
            }

            var formattedBarcode = barcode.Replace("-", string.Empty); // Remove any "-"...will be added again below.

            // Is the formatted barcode less than the default barcode size?
            if (formattedBarcode.Length < DefaultBarcodeSize)
            {
                // Yes, prepend zeroes.
                var len = formattedBarcode.Length;
                for (var i = 0; i < DefaultBarcodeSize - len; i++)
                {
                    formattedBarcode = formattedBarcode.Insert(0, "0");
                }
            }

            if (formattedBarcode.Length > BarcodeDigitsToDisplay)
            {
                var digitsToObscure = formattedBarcode.Length - BarcodeDigitsToDisplay;
                var obscureString = new string('X', digitsToObscure);
                formattedBarcode = obscureString + formattedBarcode.Substring(digitsToObscure);
            }

            // Insert hyphens to separate the barcode
            for (var i = formattedBarcode.Length - BarcodeDigitGroupSize; i > 0; i = i - BarcodeDigitGroupSize)
            {
                formattedBarcode = formattedBarcode.Insert(i, "-");
            }

            return formattedBarcode;
        }

        /// <summary>
        ///     Compares the status with successful messages.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>
        ///     Boolean flag.
        /// </returns>
        public static bool CompareStatusWithMessages(this string status)
        {
            return (status.Contains(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidBarcodeText)) ||
                    status.Contains(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CheatDetectedText)) ||
                    status.Contains(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PausedText)) ||
                    status.Contains(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReadErrorText))) &&
                   !status.Contains(
                       Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CommunicationFailureText)) &&
                   !status.Contains(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DocumentEatenText)) &&
                   !status.Contains(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InspectionFailedText)) &&
                   !status.Contains(NoteAcceptorEventsDescriptor.FaultTexts[NoteAcceptorFaultTypes.OtherFault]) &&
                   !status.Contains(NoteAcceptorEventsDescriptor.FaultTexts[NoteAcceptorFaultTypes.StackerJammed]) &&
                   !status.Contains(NoteAcceptorEventsDescriptor.FaultTexts[NoteAcceptorFaultTypes.NoteJammed]) &&
                   !status.Contains(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.RejectedText)) &&
                   !status.Contains(NoteAcceptorEventsDescriptor.FaultTexts[NoteAcceptorFaultTypes.OtherFault]) &&
                   !status.Contains(NoteAcceptorEventsDescriptor.FaultTexts[NoteAcceptorFaultTypes.StackerFull]) &&
                   !status.Contains(
                       NoteAcceptorEventsDescriptor.FaultTexts[NoteAcceptorFaultTypes.StackerDisconnected]);
        }

        /// <summary>
        ///     Convert denomination text to digital format
        /// </summary>
        /// <param name="denominationsText">Text to convert</param>
        /// <param name="notes">A collection of notes</param>
        /// <returns>The dollar value, e.g. $1</returns>
        public static string GetDigitalFormatOfDenominations(this string denominationsText, Collection<int> notes, CultureInfo culture)
        {
            var st = new StringBuilder();
            foreach (var note in notes)
            {
                st.Append(note.FormattedCurrencyString("C0", culture) + " ");
            }

            return st.ToString();
        }
    }
}