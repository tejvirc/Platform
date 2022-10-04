namespace Aristocrat.Monaco.Hardware.Contracts.EdgeLighting
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    
    /// <summary>
    /// Defines runtime edge light parameters.
    /// </summary>
    public static class EdgeLightRuntimeParameters
    {
        /// <summary>
        /// Name of the shared memory.
        /// </summary>
        public const string EdgeLightSharedMemoryName = "EdgelightSharedMemory";

        /// <summary>
        /// Name of the synchronization mutex for the shared memory.
        /// </summary>
        public const string EdgeLightSharedMutexName = "EdgelightSharedMutex";

    }
    /// <summary>
    ///     Interface for controlling edge lights.
    /// </summary>
    public interface IEdgeLightingController
    {
        /// <summary>
        ///     Gets if edge lighting detected.
        /// </summary>
        bool IsDetected { get; }

        /// <summary>
        ///     Gets the available edge lighting device types
        /// </summary>
        IEnumerable<EdgeLightDeviceInfo> Devices { get; }

        /// <summary>
        /// </summary>
        bool DetectedOnStartup { get; }

        /// <summary>
        ///     Detected strip ids.
        /// </summary>
        IList<int> StripIds { get; }

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
        ///     Returns the led count for given strip Id. Zero if invalid strip id.
        /// </summary>
        /// <param name="stripId">Strip Id for which to return the Led count.</param>
        /// <returns></returns>
        int GetStripLedCount(int stripId);

        /// <summary>
        ///     Sets the global brightness of the edge light.
        /// </summary>
        /// <param name="brightness"></param>
        /// <param name="priority"></param>
        void SetBrightnessForPriority(int brightness, StripPriority priority);

        /// <summary>
        ///     Clears the global brightness of the edge light for given priority.
        /// </summary>
        /// <param name="priority"></param>
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
        ///     Adds a edge light pattern renderer to renderer list.
        /// </summary>
        /// <param name="forParameters"></param>
        /// <returns>An <c>IEdgeLightToken</c> that can be used to remove the state from the list.</returns>
        IEdgeLightToken AddEdgeLightRenderer(PatternParameters forParameters);

        /// <summary>
        ///     Removes a previously added renderer from the renderer list.
        /// </summary>
        /// <param name="token">The <c>IEdgeLightToken</c> received for setting the renderer using <c>AddEdgeLightRenderer.</c></param>
        void RemoveEdgeLightRenderer(IEdgeLightToken token);

        /// <summary>
        ///     Set's the new priority comparator and orders the strip priorities based on new comparer.
        /// </summary>
        /// <param name="comparer">New priority comparer to use for ordering priorities.</param>
        void SetPriorityComparer(IComparer<StripPriority> comparer);
    }

    /// <summary>
    ///     Base class for pattern parameters.
    /// </summary>
    public abstract class PatternParameters
    {
        /// <summary>
        ///     List instance that represent all strips.
        /// </summary>
        public static readonly IReadOnlyCollection<int> AllStrips = new List<int>();

        /// <summary>
        /// </summary>
        public StripPriority Priority { get; set; } = StripPriority.LowPriority;

        /// <summary>
        ///     Strips on which the pattern will be rendered.
        /// </summary>
        public IReadOnlyCollection<int> Strips { get; set; } = AllStrips;
    }

    /// <summary>
    ///     Solid color pattern. Displays a solid color on edge lights.
    /// </summary>
    public class SolidColorPatternParameters : PatternParameters
    {
        /// <summary>
        /// </summary>
        public Color Color { get; set; } = Color.Blue;
    }

    /// <summary>
    ///     Rainbow color pattern. Displays a rainbow color animation on edge lights.
    /// </summary>
    public class RainbowPatternParameters : PatternParameters
    {
        /// <summary>
        ///     The time when it has to render the next rainbow color
        /// </summary>
        public int Delay { get; set; } = 30;
    }

    /// <summary>
    /// </summary>
    public class ChaserPatternParameters : PatternParameters
    {
        /// <summary>
        /// </summary>
        public Color BackgroundColor { get; set; } = Color.Black;

        /// <summary>
        /// </summary>
        public Color ForegroundColor { get; set; } = Color.White;

        /// <summary>
        /// </summary>
        public int Delay { get; set; } = 100;
    }

    /// <summary>
    /// </summary>
    public class BlinkPatternParameters : PatternParameters
    {
        /// <summary>
        /// </summary>
        public Color OnColor { get; set; } = Color.Black;

        /// <summary>
        /// </summary>
        public Color OffColor { get; set; } = Color.White;

        /// <summary>
        /// </summary>
        public int OnTime { get; set; } = 100;

        /// <summary>
        /// </summary>
        public int OffTime { get; set; } = 100;
    }

    /// <summary>
    /// </summary>
    public class IndividualLedPatternParameters : PatternParameters
    {
        /// <summary>
        /// StripUpdateFunction takes strip id and led count and returns strip led colors.
        /// </summary>
        public Func<int, int, Color[]> StripUpdateFunction { get; set; }
    }

    /// <summary>
    /// </summary>
    public class IndividualLedBlinkPatternParameters : PatternParameters

    {
        /// <summary>
        ///     StripOnUpdateFunction takes strip id and led count and returns strip led colors for the off time of the cycle.
        /// </summary>
        public Func<int, int, Color[]> StripOnUpdateFunction { get; set; }
        
        /// <summary>
        ///     StripOffUpdateFunction takes strip id and led count and returns strip led colors for the off time of the cycle.
        /// </summary>
        public Func<int, int, Color[]> StripOffUpdateFunction { get; set; }

        /// <summary>
        ///     Gets or sets the on time
        /// </summary>
        public int OnTime { get; set; } = 100;

        /// <summary>
        ///     Gets or sets the off time
        /// </summary>
        public int OffTime { get; set; } = 100;
    }
}