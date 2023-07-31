namespace Aristocrat.Monaco.Kernel.MarketConfig;

using System;

/// <summary>
///     MarketConfigException is the exception raised when a method of IMarketConfigManager fails.
/// </summary>
public class MarketConfigException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MarketConfigException" /> class.  Also contains an error information
    ///     message.
    /// </summary>
    /// <param name="message">Associated error information for MarketConfigException.</param>
    public MarketConfigException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initialize a new instance of the <see cref="MarketConfigException" /> class from another exception.
    /// </summary>
    /// <param name="parentException">The exception that we are wrapping</param>
    public MarketConfigException(Exception parentException)
        : base("An exception occurred while attempting to load the market configuration.", parentException)
    {
    }
}
