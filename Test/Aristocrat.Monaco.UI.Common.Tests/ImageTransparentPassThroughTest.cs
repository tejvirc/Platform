namespace Aristocrat.Monaco.UI.Common.Tests
{
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Controls;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     Test for the ImageTransparentPassThrough class
    /// </summary>
    [TestClass]
    public class ImageTransparentPassThroughTest
    {
        private ImageTransparentPassThrough _target;

        [TestInitialize]
        public void Initialize()
        {
            _target = new ImageTransparentPassThrough();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [DataRow(0, 0)]
        [DataRow(0.9, 0.9)]
        [TestMethod]
        public void HitPointInsideImage(double x, double y)
        {
            var hitPoint = new Point(x, y);
            var renderSize = new Size(1, 1);
            var pixels = new byte[4] { 0, 0, 0, 255 };
            var source = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgr32, null, pixels, 4);
            var args = new object[3] { hitPoint, renderSize, source };
            var obj = new PrivateObject(_target);
            var hitTestResult = obj.Invoke("GetHitTestResult", args);
            Assert.IsInstanceOfType(hitTestResult, typeof(HitTestResult));
        }

        [DataRow(1, 1)]
        [DataRow(-1, -1)]
        [DataRow(0, 1)]
        [DataRow(1, 0)]
        [TestMethod]
        public void HitPointOutsideImage(double x, double y)
        {
            var hitPoint = new Point(x, y);
            var renderSize = new Size(1, 1);
            var pixels = new byte[4] { 0, 0, 0, 255 };
            var source = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgr32, null, pixels, 4);
            var args = new object[3] { hitPoint, renderSize, source };
            var obj = new PrivateObject(_target);
            var hitTestResult = obj.Invoke("GetHitTestResult", args);
            Assert.AreEqual(null, hitTestResult);
        }

        [DataRow(0,0)]
        [TestMethod]
        public void HitTransparentPoint(double x, double y)
        {
            var hitPoint = new Point(x, y);
            var renderSize = new Size(1, 1);
            var pixels = new byte[4] { 0, 0, 0, 0 };
            var source = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgr32, null, pixels, 4);
            var args = new object[3] { hitPoint, renderSize, source };
            var obj = new PrivateObject(_target);
            var hitTestResult = obj.Invoke("GetHitTestResult", args);
            Assert.AreEqual(null, hitTestResult);
        }
    }
}