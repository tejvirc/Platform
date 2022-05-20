namespace Aristocrat.Monaco.Gaming.Barkeeper
{
    using System.Collections.Generic;
    using Contracts.Barkeeper;
    using Hardware.Contracts.EdgeLighting;

    /// <summary>
    ///     Extensions for the <see cref="IEdgeLightingController" />
    /// </summary>
    public static class EdgeLightingControllerExtensions
    {
        /// <summary>
        ///     Sets the edge light render for the given reward type with the provided brightness
        /// </summary>
        /// <param name="this">The <see cref="IEdgeLightingController" /> instance</param>
        /// <param name="reward">The reward to set</param>
        /// <param name="brightness">The brightness to set for this reward</param>
        /// <returns>The edge light token</returns>
        public static IEdgeLightToken AddEdgeLightRenderer(this IEdgeLightingController @this, RewardLevel reward, int brightness)
        {
            @this.SetBrightness(new List<int> { (int)reward.Led.ToStripIds() }, StripPriority.PlatformControlled, brightness);

            return @this.AddEdgeLightRenderer(BarkeeperRewardLevelHelper.GetPattern(reward.Color, reward.Led, reward.Alert));
        }

        /// <summary>
        ///     Sets the edge light render for the given pattern parameters type with the provided brightness
        /// </summary>
        /// <param name="this">The <see cref="IEdgeLightingController" /> instance</param>
        /// <param name="parameters">The pattern parameters to set</param>
        /// <param name="brightness">The brightness to set for this pattern parameters</param>
        /// <returns>The edge light token</returns>
        public static IEdgeLightToken AddEdgeLightRenderer(this IEdgeLightingController @this, PatternParameters parameters, int brightness)
        {
            @this.SetBrightness(parameters.Strips, parameters.Priority, brightness);

            return @this.AddEdgeLightRenderer(parameters);
        }

        private static void SetBrightness(
            this IEdgeLightingController @this,
            IEnumerable<int> lights,
            StripPriority priority,
            int brightness)
        {
            foreach (var light in lights)
            {
                @this.SetStripBrightnessForPriority(light, brightness, priority);
            }
        }
    }
}