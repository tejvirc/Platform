using System.Diagnostics.CodeAnalysis;

[module:
    SuppressMessage(
        "Microsoft.Naming",
        "CA1716:IdentifiersShouldNotMatchKeywords",
        Scope = "member",
        Target = "Aristocrat.Monaco.Kernel.IRunnable.#Stop()",
        Justification = "Other languages are not a concern at this time.")]

namespace Aristocrat.Monaco.Kernel
{
    using System;

    /// <summary>
    ///     IRunnable provides an interface for initializing, running, and stopping components and services.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <c>BaseRunnable</c> implements this interface  and provides the base implementation for the Runnable State
    ///         management and also the base implementation for <c>Run()</c> and <c>Stop</c> Methods.
    ///     </para>
    /// </remarks>
    public interface IRunnable
    {
        /// <summary>
        ///     Gets the property that represents the RunState of the Runnable
        /// </summary>
        RunnableState RunState { get; }

        /// <summary>
        ///     Maximum time to wait before the runnable is forcibly terminated
        /// </summary>
        TimeSpan Timeout { get; }

        /// <summary>
        ///     Initializes the runnable.
        /// </summary>
        /// <exception cref="RunnableException">Thrown when the runnable fails initialization.</exception>
        void Initialize();

        /// <summary>
        ///     Starts the runnable.
        /// </summary>
        void Run();

        /// <summary>
        ///     Stops the runnable.
        /// </summary>
        void Stop();
    }
}