namespace Aristocrat.Monaco.Hardware.Contracts.TowerLight
{
    using System;
    using System.ComponentModel;

    /// <summary>Tower Light flash state enumerations.</summary>
    public enum FlashState
    {
        /// <summary>Light is off.</summary>
        [Description("Off")]
        Off = 0,

        /// <summary>Light is on. (No flashing)</summary>
        [Description("On")]
        On,

        /// <summary>Light is flashing slow.</summary>
        [Description("Slow Flash")]
        SlowFlash,

        /// <summary>Light is flashing in medium speed.</summary>
        [Description("Medium Flash")]
        MediumFlash,

        /// <summary>Light is flashing in the same speed with MediumFlash but ON/OFF are reversed.</summary>
        [Description("Medium Flash Reversed")]
        MediumFlashReversed,

        /// <summary>Light is flashing fast.</summary>
        [Description("Fast Flash")]
        FastFlash,

        /// <summary>Light is flashing in the same speed with SlowFlash but ON/OFF are reversed.</summary>
        [Description("Slow Flash Reversed")]
        SlowFlashReversed,
    }

    /// <summary>Tower Light tier enumerations.</summary>
    public enum LightTier
    {
        /// <summary>Light Tier #1</summary>
        [Description("Tier 1")]
        Tier1 = 0,

        /// <summary>Light Tier #2</summary>
        [Description("Tier 2")]
        Tier2,

        /// <summary>Light Tier #3</summary>
        [Description("Tier 3")]
        Tier3,

        /// <summary>Light Tier #4</summary>
        [Description("Tier 4")]
        Tier4,

        /// <summary>Strobe Light</summary>
        [Description("Strobe")]
        Strobe
    }

    /// <summary>Definition of the ITowerLight interface. This interface provides the public access for the use of TowerLight component.</summary>
    public interface ITowerLight
    {
        /// <summary>
        ///     Gets a property indicating whether the tower light is on
        /// </summary>
        bool IsLit { get; }

        /// <summary>Turns off all the lights.</summary>
        void Reset();

        /// <summary>Sets the tower light flash state for the given light index.</summary>
        /// <param name="lightTier">Enumerated light tier value.</param>
        /// <param name="flashState">Enumerated light flash state to set.</param>
        /// <param name="test">True if this request is for testing purposes</param>
        /// <param name="duration">Time duration for which tier would be in particular state</param>
        void SetFlashState(LightTier lightTier, FlashState flashState, TimeSpan duration, bool test = false);

        /// <summary>
        /// Gets the current flash state for the given light tier
        /// </summary>
        FlashState GetFlashState(LightTier lightTier);
    }
}
