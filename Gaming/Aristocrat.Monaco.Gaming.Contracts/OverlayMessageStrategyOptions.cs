namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System.ComponentModel;

    /// <summary>
    ///     Enumerates the possible overlay message strategies
    /// </summary>
    public enum OverlayMessageStrategyOptions
    {
        /// <summary>
        ///     Basic
        /// </summary>
        [Description("Basic")]
        Basic,

        /// <summary>
        ///     Enhanced
        /// </summary>
        [Description("Enhanced")]
        Enhanced,

        /// <summary>
        ///     Game-Driven
        /// </summary>
        [Description("Game-Driven")]
        GameDriven
    }
}
