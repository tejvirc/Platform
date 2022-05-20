namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Interop;
    using System.Windows.Media.Imaging;

    /// <summary>
    ///     Converts from a bitmap type (bmp, png, jpg) to an ImageSource.
    ///     This would be used in xaml code to show an image from a resource file
    /// </summary>
    public class BitmapToImageSourceConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Bitmap bitmap))
            {
                return null;
            }

            BitmapSource bitmapSource;

            var bmpPtr = IntPtr.Zero;

            try
            {
                bmpPtr = bitmap.GetHbitmap();

                bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                    bmpPtr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                // freeze the bitmap to avoid hooking events to the bitmap
                bitmapSource.Freeze();
            }
            catch
            {
                bitmapSource = null;
            }
            finally
            {
                NativeMethods.DeleteObject(bmpPtr);
            }

            return bitmapSource;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (!(value is BitmapSource source))
            {
                return null;
            }

            var bitmap = new Bitmap(
                source.PixelWidth,
                source.PixelHeight,
                PixelFormat.Format32bppPArgb);

            var data = bitmap.LockBits(
                new Rectangle(System.Drawing.Point.Empty, bitmap.Size),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppPArgb);

            source.CopyPixels(
                Int32Rect.Empty,
                data.Scan0,
                data.Height * data.Stride,
                data.Stride);

            bitmap.UnlockBits(data);

            return bitmap;
        }
    }
}
