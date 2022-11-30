namespace Aristocrat.Monaco.Common
{
    using JetBrains.Annotations;

    /// <summary>
    ///     A method delegate for logging messages for <see cref="ScopedMethodTimer"/>
    /// </summary>
    /// <param name="format">The message for the logger to use for formatting</param>
    /// <param name="caller">The caller for the function</param>
    /// <param name="elapsed">The elapsed time</param>
    [StringFormatMethod("format")]
    public delegate void MethodLogger(string format, string caller, double elapsed);
}