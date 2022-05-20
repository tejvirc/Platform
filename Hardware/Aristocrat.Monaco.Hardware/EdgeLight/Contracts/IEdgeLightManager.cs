namespace Aristocrat.Monaco.Hardware.EdgeLight.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using Hardware.Contracts.EdgeLighting;
    using Strips;

    /// <summary>
    ///     Definition of the IEdgeLightManager interface.
    /// </summary>
    internal interface IEdgeLightManager : IDisposable
    {
        /// <summary>
        ///     Set's the low power mode.
        /// </summary>
        bool PowerMode { set; }

        /// <summary>
        ///     List of all the logical Strips attached
        /// </summary>
        IReadOnlyCollection<StripData> LogicalStrips { get; }

        IEnumerable<EdgeLightDeviceInfo> DevicesInfo { get; }

        /// <summary>
        ///     Clears the strip with that given priority. So that it doesn't take
        ///     part in rendering.
        /// </summary>
        /// <param name="stripId">The strip id.</param>
        /// <param name="priority">The priority of the component sending the data.</param>
        /// <returns></returns>
        bool ClearStripForPriority(
            int stripId,
            StripPriority priority);

        /// <summary>
        ///     Sets the whole strip with the Color and the data of given priority
        /// </summary>
        /// <param name="stripId">The strip id.</param>
        /// <param name="color">The color to set.</param>
        /// <param name="priority">The priority of the component sending the data.</param>
        /// <returns>
        ///     True if the action can be performed
        ///     (including parameter checking), false other wise
        /// </returns>
        bool SetStripColor(
            int stripId,
            Color color,
            StripPriority priority);

        /// <summary>
        ///     Sets the strip with the data starting at the given offset and the led count.
        /// </summary>
        /// <param name="stripId">The Strip Id.</param>
        /// <param name="priority">The priority of the component sending the data.</param>
        /// <param name="colorBuffer"></param>
        /// <param name="destinationLedIndex"></param>
        /// <returns>
        ///     True if the action can be performed
        ///     (including parameter checking), false other wise
        /// </returns>
        bool SetStripColors(
            int stripId,
            LedColorBuffer colorBuffer,
            int destinationLedIndex,
            StripPriority priority);

        /// <summary>
        ///     Renders all the data in the strip, based on priority.
        /// </summary>
        void RenderAllStripData();

        /// <summary>
        ///     Set's all the strips with the Particular color and the given priority.
        /// </summary>
        /// <param name="brightness">The brightness to set</param>
        /// <param name="priority">The priority of the component sending the data.</param>
        void SetBrightnessForPriority(
            int brightness,
            StripPriority priority);

        /// <summary>
        ///     Unset the brightness for all strips with a particular priority.
        /// </summary>
        /// <param name="priority">The priority of the subsystem that unset the brightness.</param>
        void ClearBrightnessForPriority(StripPriority priority);

        /// <summary>
        ///     Sets the individual edge light strip brightness of the given priority.
        /// </summary>
        /// <param name="stripId">Strip id for which to set brightness.</param>
        /// <param name="brightness"></param>
        /// <param name="priority"></param>
        void SetStripBrightnessForPriority(int stripId, int brightness, StripPriority priority);

        /// <summary>
        ///     Sets the individual edge light strip brightness of the given priority.
        /// </summary>
        /// <param name="stripId"></param>
        /// <param name="priority"></param>
        void ClearStripBrightnessForPriority(int stripId, StripPriority priority);

        /// <summary>
        ///     Sets the brightness limits for given priority.
        /// </summary>
        /// <param name="limits">New limits for priority.</param>
        /// <param name="forPriority">Strip priority for which to set limits.</param>
        void SetBrightnessLimits(EdgeLightingBrightnessLimits limits, StripPriority forPriority);

        /// <summary>
        ///     Gets the brightness limits for given priority.
        /// </summary>
        /// <param name="forPriority">Strip priority for which to set limits.</param>
        /// <returns>Limits for given priority.</returns>
        EdgeLightingBrightnessLimits GetBrightnessLimits(StripPriority forPriority);

        /// <summary>
        ///     Set's the new priority comparator and orders the strip priorities based on new comparer.
        /// </summary>
        /// <param name="comparer">New priority comparer to use for ordering priorities.</param>
        void SetPriorityComparer(IComparer<StripPriority> comparer);
    }
}