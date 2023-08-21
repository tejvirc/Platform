namespace Aristocrat.G2S.Client.Devices
{
    using System;

    /// <summary>
    ///     Defines a contract for classes that support the noResponseTimer attribute , such as the communications class.
    /// </summary>
    public interface INoResponseTimer
    {
        /// <summary>
        ///     Gets specifies the interval used in the no-response timer.
        /// </summary>
        TimeSpan NoResponseTimer { get; }
    }
}