namespace Aristocrat.Monaco.Common
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    /// <summary>
    ///     A class that is used for measuring timing metrics for a particular scope
    /// </summary>
    /// <remarks>
    ///     Sample usage for tracking method timing:
    ///
    ///     public class MyTestClass
    ///     {
    ///         private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
    ///
    ///         public void MyTestMethod()
    ///         {
    ///             using (var _ = new ScopedMethodTimer(Logger.DebugMethodLogger))
    ///             {
    ///                 // Calls to trace timing metric for
    ///             }
    ///         }
    ///     }
    /// </remarks>
    public sealed class ScopedMethodTimer : IDisposable
    {
        private readonly MethodLogger _logger;
        private readonly string _message;
        private readonly Stopwatch _stopwatch;
        private bool _disposed;

        /// <summary>
        ///     Creates a <see cref="ScopedMethodTimer"/> for capturing timing methods
        /// </summary>
        /// <param name="logger">The logger to use to capture the timing metrics</param>
        /// <param name="caller">The calling method.  By default this will be populated with the calling class and method name</param>
        /// <exception cref="ArgumentException">Thrown when the caller is null or an empty string</exception>
        /// <exception cref="ArgumentNullException">Thrown when the logger is null</exception>
        public ScopedMethodTimer(MethodLogger logger, [CallerMemberName] string caller = "")
        {
            if (string.IsNullOrEmpty(caller))
            {
                throw new ArgumentException(@"Value cannot be null or empty.", nameof(caller));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _message = caller;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        /// <summary>
        ///     Creates a <see cref="ScopedMethodTimer"/> for capturing timing methods
        /// </summary>
        /// <param name="logger">The logger to use to capture the timing metrics</param>
        /// <param name="traceLogger">The logger to use to write a trace</param>
        /// <param name="startMessage">The message to be printed when the timer starts</param>
        /// <param name="endMessage">The message to be printed when the timer ends</param>
        /// <param name="prefix">Optional prefix to be added to the caller</param>
        /// <param name="caller">The calling method.  By default this will be populated with the calling class and method name</param>
        /// <exception cref="ArgumentException">Thrown when the caller is null or an empty string</exception>
        /// <exception cref="ArgumentNullException">Thrown when the logger is null</exception>
        public ScopedMethodTimer(MethodLogger logger, MethodTraceLogger traceLogger, string startMessage, string prefix, string endMessage, [CallerMemberName] string caller = "")
        {
            if (string.IsNullOrEmpty(caller))
            {
                throw new ArgumentException(@"Value cannot be null or empty.", nameof(caller));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            traceLogger = traceLogger ?? throw new ArgumentNullException(nameof(traceLogger));

            var prefixMessage= $"[{prefix} {caller}] : ";

            traceLogger($"[{prefix} {caller}]: {{0}}", startMessage);
            _message =  $"[{prefix} {caller}]: {endMessage}";
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _stopwatch.Stop();
            _logger("{0} - Elapsed Time: {1}ms.", _message, _stopwatch.Elapsed.TotalMilliseconds);

            _disposed = true;
        }
    }
}