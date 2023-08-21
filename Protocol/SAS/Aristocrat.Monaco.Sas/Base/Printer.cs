namespace Aristocrat.Monaco.Sas.Base
{
    using log4net;

    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SharedDevice;
    using Kernel;

    /// <inheritdoc />
    /// <remarks>
    /// The unmanaged code needs to check if the printer is available. It is unable to
    /// access ServiceManager for that. And letting Base do it will decrease the Client's
    /// dependency on Platform.  
    /// </remarks>>
    public class Printer : Contracts.Client.IPrinter
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <inheritdoc />
        public bool PrinterEnabled
        {
            get
            {
                
                IDeviceService printer = ServiceManager.GetInstance().TryGetService<IPrinter>();
                return printer?.Enabled ?? false;
            }

            set
            {
                IDeviceService printer = ServiceManager.GetInstance().TryGetService<IPrinter>();
                if (printer == null) return;

                // Only modify the enable state if it has changed.
                if (printer.Enabled != value)
                {
                    if (value)
                    {
                        Logger.Info("[CONFIG] Printer is enabled for " + EnabledReasons.Backend);
                        printer.Enable(EnabledReasons.Backend);
                    }
                    else
                    {
                        Logger.Info("[CONFIG] Printer is disabled for " + DisabledReasons.Backend);
                        printer.Disable(DisabledReasons.Backend);
                    }
                }
            }
        }
    }
}
