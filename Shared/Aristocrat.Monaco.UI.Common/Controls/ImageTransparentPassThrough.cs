namespace Aristocrat.Monaco.UI.Common.Controls
{
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using log4net;

    /// <summary>
    ///     ImageTransparentPassThrough
    ///     Image Control that allows clicks on transparent areas to pass through
    /// </summary>
    public class ImageTransparentPassThrough : Image
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     HitTestCore
        /// </summary>
        /// <param name="hitTestParameters"></param>
        /// <returns></returns>
        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            var hitPoint = hitTestParameters.HitPoint;
            var renderSize = RenderSize;
            var source = (BitmapSource)Source;

            return GetHitTestResult(hitPoint, renderSize, source);
        }

        private HitTestResult GetHitTestResult(
            Point hitPoint,
            Size renderSize,
            BitmapSource source
        )
        {
            var x = (int)(hitPoint.X / renderSize.Width * source.PixelWidth);
            var y = (int)(hitPoint.Y / renderSize.Height * source.PixelHeight);

            // Copy the single pixel into a new byte array representing RGBA
            var pixel = new byte[4];
            if (x < 0 || x >= source.PixelWidth || y < 0 || y >= source.PixelHeight)
            {
                Logger.Error(
                    "Hit point outside of image source\n" +
                    $"source = {source}\n" +
                    $"x = {x} hitPointX = {hitPoint.X} Width = {renderSize.Width} PixelWidth = {source.PixelWidth}\n" +
                    $"y = {y} hitPointY = {hitPoint.Y} Height = {renderSize.Height} PixelHeight = {source.PixelHeight}");
                return null;
            }

            source.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, 4, 0);

            // Check the alpha (transparency) of the pixel
            // - threshold can be adjusted from 0 to 255
            if (pixel[3] < 10)
            {
                return null;
            }

            return new PointHitTestResult(this, hitPoint);
        }
    }
}