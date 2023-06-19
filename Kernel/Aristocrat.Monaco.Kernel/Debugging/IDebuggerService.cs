namespace Aristocrat.Monaco.Kernel.Debugging
{
    /// <summary>
    /// Handles the auto-attachment of the debugger (only for DEBUG configuration).<br />
    /// This allows developers to run through certain phases of the application lifecycle with full performance, and then auto-attach the debugger afterwards.<br />
    /// Use the following commandline argument as an example: <c>debuggerAttachPoint=OnSystemInitialized</c><br />
    /// See the <see cref="DebuggerAttachPoint">DebuggerAttachPoint</see> enum for valid values.<br />
    /// The commandline argument propagates to the IPropertiesManager at runtime.<br />
    /// <br />
    /// <b>ALL CONSUMERS OF THIS CLASS SHOULD WRAP ITS USAGE WITHIN A DEBUG PREPROCESSOR CONDITION BLOCK TO AVOID AFFECTING NON-DEBUG BUILDS!</b><br />
    /// <br />
    /// <b>End-user Instructions:</b><br />
    /// - Add the "debuggerAttachPoint=OnSystemInitialized" startup/commandline arg (substitute OnSystemInitialized with the desired attach point).<br />
    /// - Run Bootstrap without debugging, in the Debug build configuration.<br />
    /// - When the desired attach point is reached during runtime, a dialog will appear to begin attaching the debugger.<br />
    /// - When the "Choose Just-In-Time Debugger" dialog appears, select your Visual Studio instance,
    ///   and then check the "Manually choose the debugging engines." checkbox, then press OK.<br />
    /// - On the "Attach to Process" dialog, make sure that the "Managed" and "Native" options are checked, then press OK.<br />
    /// - The debugger will attach to the process.
    /// </summary>
    public interface IDebuggerService : IService
    {
        /// <summary>
        /// Attaches the debugger if the specified attach point was requested.<br />
        /// <b>ONLY FUNCTIONS WHEN USING THE DEBUG BUILD CONFIGURATION, NO-OPS OTHERWISE!</b>
        /// </summary>
        /// <param name="debuggerAttachPoint">The debugging attach point to check if requested.</param>
        /// <returns>True if the debugger is/was attached, false if failed to attach.</returns>
        bool AttachDebuggerIfRequestedForPoint(DebuggerAttachPoint debuggerAttachPoint);
    }
}