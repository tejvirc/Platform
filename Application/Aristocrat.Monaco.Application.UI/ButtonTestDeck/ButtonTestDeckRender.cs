namespace Aristocrat.Monaco.Application.UI.ButtonTestDeck
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Hardware.Contracts.ButtonDeck;
    using Kernel;

    [CLSCompliant(false)]
    public class ButtonTestDeckRender
    {
        private const double DpiX = 96;
        private const double DpiY = 96;
        private readonly Visual _visual;
        private readonly RenderTargetBitmap _targetBitmap;
        private readonly IButtonDeckDisplay _display;
        private readonly byte[] _disabledImage;

        public ButtonTestDeckRender(Visual visual, int width, int height)
            : this(ServiceManager.GetInstance().GetService<IButtonDeckDisplay>(), visual, width, height)
        {
        }

        public ButtonTestDeckRender(IButtonDeckDisplay display, Visual visual, int width, int height)
        {
            _display = display;
            _visual = visual ?? throw new ArgumentNullException(nameof(visual));
            _targetBitmap = new RenderTargetBitmap(width, height, DpiX, DpiY, PixelFormats.Pbgra32);
            _disabledImage = new byte[width * height * 2];
        }

        public void Render(int screenId, bool enabled = true)
        {
            var rawData = _disabledImage;
            if (enabled)
            {
                using (var bitmap = RenderToBitmap(System.Drawing.Imaging.PixelFormat.Format16bppRgb565))
                {
                    rawData = Monaco.UI.Common.BitmapImage.GetRawBytes(bitmap, null);
                }
            }
            _display?.Draw(screenId, rawData);
        }

        private Bitmap RenderToBitmap(System.Drawing.Imaging.PixelFormat format)
        {
            _targetBitmap.Render(_visual);

            MemoryStream stream = null;
            Bitmap renderedBitmap = null;

            try
            {
                stream = new MemoryStream();

                var encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(_targetBitmap));
                encoder.Save(stream);

                using (var bitmap = new Bitmap(stream))
                {
                    stream = null;
                    renderedBitmap = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), format);
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }

            return renderedBitmap;
        }
    }
}
