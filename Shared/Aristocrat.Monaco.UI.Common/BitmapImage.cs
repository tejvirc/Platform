namespace Aristocrat.Monaco.UI.Common
{
    using System;
    using System.Drawing;

    /// <summary>Definition of the Image class</summary>
    public class BitmapImage : IDisposable
    {
        private bool _disposed;
        private readonly Bitmap _imageBitmap;
        private readonly byte[] _rawBytes;

        /// <summary>Constructor of Image</summary>
        /// <param name="w">The width of the image</param>
        /// <param name="h">The height of the image</param>
        public BitmapImage(int w, int h)
        : this(w, h, System.Drawing.Imaging.PixelFormat.Format16bppRgb565)
        {
        }

        /// <summary>Constructor of Image</summary>
        /// <param name="w">The width of the image</param>
        /// <param name="h">The height of the image</param>
        /// <param name="format">The pixel format of the bitmap</param>
        public BitmapImage(int w, int h, System.Drawing.Imaging.PixelFormat format)
        {
            _disposed = false;
            _imageBitmap = new Bitmap(w, h, format);
            _rawBytes = new byte[w * h * 2];
            Canvas = Graphics.FromImage(_imageBitmap);
        }

        /// <summary>Destructor of Image </summary>
        ~BitmapImage()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>The Graphics object to be drawn on</summary>
        public Graphics Canvas { get; }

        /// <summary>The width of the image</summary>
        public int Width => _imageBitmap.Width;

        /// <summary>The height of the image</summary>
        public int Height => _imageBitmap.Height;

        /// <summary>The raw rgb bytes of the drawn image</summary>
        public byte[] GetRawBytes()
        {
            return GetRawBytes(_imageBitmap, _rawBytes);
        }

        /// <summary>The raw rgb bytes of the drawn image</summary>
        /// <param name="bitmap">The image</param>
        /// <param name="rawBytes">The raw rgb bytes</param>
        /// <returns>The raw rgb bytes</returns>
        public static byte[] GetRawBytes(Bitmap bitmap, byte[] rawBytes)
        {
            // Lock the bitmap bits.  
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            int bytes = Math.Abs(bmpData.Stride) * bitmap.Height;
            rawBytes = rawBytes ?? new byte[bytes];

            if (rawBytes.Length == bytes)
            {
                // Copy the RGB values into the array.
                System.Runtime.InteropServices.Marshal.Copy(ptr, rawBytes, 0, bytes);
            }

            // Unlock the bits.
            bitmap.UnlockBits(bmpData);
            return rawBytes;
        }

        /// <summary>Disposes the object</summary>
        /// <param name="disposing">Whether or not managed resources should be disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _imageBitmap.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
