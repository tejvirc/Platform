namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    using Kernel;

    /// <summary>
    ///     Defines the loading animation state.
    /// </summary>
    public enum LoadingAnimationState
    {
        /// <summary>
        ///     Indicates animation file is loading
        /// </summary>
        Loading,

        /// <summary>
        ///     Indicates animation files have completed loading
        /// </summary>
        Completed,

        /// <summary>
        ///     Indicates an error loading the current animation file
        /// </summary>
        Error
    }
}
