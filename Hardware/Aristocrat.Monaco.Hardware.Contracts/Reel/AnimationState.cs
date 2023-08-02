namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    /// <summary>
    ///     Indicators for an animation's state 
    /// </summary>
    public enum AnimationState : byte
    {
        /// <summary>Indicates the animation was prepared</summary>
        Prepared,

        /// <summary>Indicates the animation was stopped</summary>
        Stopped,

        /// <summary>Indicates the animation was started</summary>
        Started,

        /// <summary>Indicates the animation was removed from the playing queue</summary>
        Removed,

        /// <summary>Indicates all animations were cleared</summary>
        AllAnimationsCleared
    }
}
