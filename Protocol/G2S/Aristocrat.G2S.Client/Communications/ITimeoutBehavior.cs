namespace Aristocrat.G2S.Client.Communications
{
    using System;

    /// <summary>
    ///     Describes the timeout behavior for outbound communications
    /// </summary>
    public interface ITimeoutBehavior
    {
        /// <summary>
        ///     Gets or sets the duration of a session
        /// </summary>
        TimeSpan SessionTimeout { get; set; }
    }
}