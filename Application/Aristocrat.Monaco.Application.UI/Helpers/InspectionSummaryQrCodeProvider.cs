namespace Aristocrat.Monaco.Application.UI.Helpers
{
    using System;
    using System.Drawing;
    using System.Windows.Media;
    using QRCoder;
    using QRCoder.Xaml;

    [CLSCompliant(false)]
    public static class InspectionSummaryQrCodeProvider
    {
        private static readonly QRCodeGenerator _qrGenerator = new ();

        public static DrawingImage GetXamlImage(
            string text,
            System.Windows.Media.Color darkColor,
            System.Windows.Media.Color lightColor)
        {
            var qrCodeData = _qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var xamlQrCode = new XamlQRCode(qrCodeData);
            var darkColorHtml = ToHtmlColorString(darkColor);
            var lightColorHtml = ToHtmlColorString(lightColor);
            return xamlQrCode.GetGraphic(pixelsPerModule: 1, lightColorHtml, darkColorHtml);
        }

        public static Image GetPrinterImage(string text)
        {
            var qrCodeData = _qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(pixelsPerModule: 1);
            return qrCodeImage;
        }

        private static string ToHtmlColorString(System.Windows.Media.Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}
