namespace Aristocrat.Monaco.Hardware.Contracts.EdgeLighting
{
    using System;

    /// <summary>
    ///     Defines parameters of Edge Lighting Brightness Limits.
    /// </summary>
    public struct EdgeLightingBrightnessLimits
    {
        private int _minimumAllowed;
        private int _maximumAllowed;

        /// <summary>
        ///     Truncates the brightness value between MinimumBrightness and MaximumBrightness.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int TruncateBrightnessValue(int value)
        {
            return Math.Min(Math.Max(MinimumBrightness, value), MaximumBrightness);
        }

        /// <summary>
        ///     Minimum brightness value supported by hardware.
        /// </summary>
        public const int MinimumBrightness = 0;

        /// <summary>
        ///     Maximum brightness value supported by hardware.
        /// </summary>
        public const int MaximumBrightness = 100;

        /// <summary>
        ///     get/set MinimumAllowed brightness. Value range (0-100). Any values outside the range are truncated.
        /// </summary>
        public int MinimumAllowed
        {
            get => _minimumAllowed;
            set => _minimumAllowed = TruncateBrightnessValue(value);
        }

        /// <summary>
        ///     get/set MaximumAllowed brightness. Value range (0-100). Any values outside the range are truncated.
        /// </summary>
        public int MaximumAllowed
        {
            get => _maximumAllowed;
            set => _maximumAllowed = TruncateBrightnessValue(value);
        }
    }
}