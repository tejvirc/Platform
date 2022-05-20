namespace Aristocrat.Monaco.UI.Common.Extensions
{
    using System;
    using System.Windows;

    /// <summary>
    ///     Extensions for Size
    /// </summary>
    public static class SizeExtensions
    {
        private const double Tolerance = 0.001;

        /// <summary>
        ///     Compare two Size objects
        /// </summary>
        /// <param name="size1"></param>
        /// <param name="size2"></param>
        /// <returns></returns>
        public static bool NearEquals(this Size size1, Size size2)
        {
            if (size1.IsEmpty && size2.IsEmpty) return true;

            return Math.Abs(size1.Width - size2.Width) < Tolerance && 
                   Math.Abs(size1.Height - size2.Height) < Tolerance;
        }
    }
}
