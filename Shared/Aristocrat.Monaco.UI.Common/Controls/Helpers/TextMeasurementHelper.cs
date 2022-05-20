namespace Aristocrat.Monaco.UI.Common.Controls.Helpers
{
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    ///     Helper for text measurement
    /// </summary>
    public static class TextMeasurementHelper
    {
        private static readonly Brush DefaultBrush = new SolidColorBrush(Colors.Black);

        /// <summary>
        ///     Measure the width of text rendering in a control (single-line).
        /// </summary>
        /// <param name="text">Text to use.</param>
        /// <param name="fontFamily">Font family.</param>
        /// <param name="fontStyle">Font style.</param>
        /// <param name="fontWeight">Font weight.</param>
        /// <param name="fontStretch">Font stretch.</param>
        /// <param name="fontSize">Font size.</param>
        /// <param name="pixelsPerDip">Pixels per DIP (such as from VisualTreeHelper.GetDpi(Visual).</param>
        /// <returns>Rendered width.</returns>
        public static double MeasureTextWidth(
            string text,
            FontFamily fontFamily,
            FontStyle fontStyle,
            FontWeight fontWeight,
            FontStretch fontStretch,
            double fontSize,
            double pixelsPerDip)
        {
            return GetControlFormattedText(text, fontFamily, fontStyle, fontWeight, fontStretch, fontSize, pixelsPerDip).Width;
        }

        /// <summary>
        ///     Measure the height of text rendering in a control (single-line).
        /// </summary>
        /// <param name="text">Text to use.</param>
        /// <param name="fontFamily">Font family.</param>
        /// <param name="fontStyle">Font style.</param>
        /// <param name="fontWeight">Font weight.</param>
        /// <param name="fontStretch">Font stretch.</param>
        /// <param name="fontSize">Font size.</param>
        /// <param name="pixelsPerDip">Pixels per DIP (such as from VisualTreeHelper.GetDpi(Visual).</param>
        /// <returns>Rendered size.</returns>
        public static double MeasureTextHeight(
            string text,
            FontFamily fontFamily,
            FontStyle fontStyle,
            FontWeight fontWeight,
            FontStretch fontStretch,
            double fontSize,
            double pixelsPerDip)
        {
            return GetControlFormattedText(text, fontFamily, fontStyle, fontWeight, fontStretch, fontSize, pixelsPerDip).Height;
        }

        private static FormattedText GetControlFormattedText(
            string text,
            FontFamily fontFamily,
            FontStyle fontStyle,
            FontWeight fontWeight,
            FontStretch fontStretch,
            double fontSize,
            double pixelsPerDip)
        {
            if (fontSize <= 0)
            {
                fontSize = 1;
            }

            return new FormattedText(
                text,
                Thread.CurrentThread.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    fontFamily,
                    fontStyle,
                    fontWeight,
                    fontStretch),
                fontSize,
                DefaultBrush,
                pixelsPerDip);
        }
    }
}
