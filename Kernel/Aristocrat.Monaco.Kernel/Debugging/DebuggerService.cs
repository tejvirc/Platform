using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Aristocrat.Monaco.Kernel.Debugging
{
    /// <inheritdoc cref="IDebuggerService"/>
    public class DebuggerService : IDebuggerService
    {
        private IPropertiesManager _propertiesManager;

        /// <summary>
        /// Property key for use with the IPropertiesManager.
        /// </summary>
        public static readonly string DebuggerAttachPointPropertyKey = "debuggerAttachPoint";

        /// <summary>
        /// Service name.
        /// </summary>
        public string Name => nameof(DebuggerService);

        /// <summary>
        /// Collection of service types.
        /// </summary>
        public ICollection<Type> ServiceTypes => new[] { typeof(IDebuggerService) };

        /// <summary>
        /// Initializes the service.
        /// </summary>
        public void Initialize()
        {
            _propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
        }

        /// <inheritdoc />
        public bool AttachDebuggerIfRequestedForPoint(DebuggerAttachPoint debuggerAttachPoint)
        {
#if DEBUG
            if (IsDebuggerAttachPointRequested(debuggerAttachPoint))
            {
                return AttachDebugger();
            }
#endif
            return false;
        }

        /// <summary>
        /// Checks if the specified debugger attach point matches the defined attach point in the Properties Manager.<br />
        /// <b>ONLY FUNCTIONS WHEN USING THE DEBUG BUILD CONFIGURATION, NO-OPS OTHERWISE!</b>
        /// </summary>
        /// <param name="debuggerAttachPoint">The attach point to check if requested in the IPropertiesManager.</param>
        /// <returns></returns>
        private bool IsDebuggerAttachPointRequested(DebuggerAttachPoint debuggerAttachPoint)
        {
#if DEBUG
            return string.Equals(
                _propertiesManager.GetValue(DebuggerAttachPointPropertyKey, string.Empty),
                debuggerAttachPoint.ToString(),
                StringComparison.OrdinalIgnoreCase
            );
#else
            return false;
#endif
        }

        /// <summary>
        /// Attaches the debugger to the application. <br />
        /// <b>ONLY FUNCTIONS WHEN USING THE DEBUG BUILD CONFIGURATION, NO-OPS OTHERWISE!</b>
        /// </summary>
        /// <returns>See <see cref="Debugger.Launch"/>'s return info. Returns true if debugger startup/attach is successful, false otherwise.</returns>
        private bool AttachDebugger()
        {
#if DEBUG
            return Debugger.Launch();
#else
            return false;
#endif
        }
    }
}
