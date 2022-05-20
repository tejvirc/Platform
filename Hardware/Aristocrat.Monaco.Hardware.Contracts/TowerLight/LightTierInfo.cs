namespace Aristocrat.Monaco.Hardware.Contracts.TowerLight
{
    using System;
    using System.Threading;

    /// <summary>Definition of the LightTierInfo class.</summary>
    public class LightTierInfo
    {
        /// <summary>Default Constructor</summary>
        public LightTierInfo()
        {
            FlashState = FlashState.Off;
            DeviceOn = false;
            Duration = Timeout.InfiniteTimeSpan;
            CanBeOverriddenByLowerIfOff = false;
        }

        /// <summary> Parameterized constructor </summary>
        public LightTierInfo(FlashState flashState, TimeSpan duration, bool deviceOn = false, bool canBeOverriddenByLowerIfOff = false)
        {
            FlashState = flashState;
            Duration = duration;
            DeviceOn = deviceOn;
            CanBeOverriddenByLowerIfOff = canBeOverriddenByLowerIfOff;
        }

        /// <summary> set the flash state of like on,off,slow flash etc</summary>
        public FlashState FlashState { get; set; }

        /// <summary>true if a tier needs to be on or off </summary>
        public bool DeviceOn { get; set; }

        /// <summary>Time Span for which tier should be on</summary>
        public TimeSpan Duration { get; set; }

        /// <summary>Indicates if it could be overridden by lower priority configuration if Off</summary>
        public bool CanBeOverriddenByLowerIfOff { get; }
    }
}