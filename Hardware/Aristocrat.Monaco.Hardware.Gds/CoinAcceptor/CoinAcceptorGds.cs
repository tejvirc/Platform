namespace Aristocrat.Monaco.Hardware.Gds.CoinAcceptor
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Contracts.Gds.CoinAcceptor;
    using Contracts.CoinAcceptor;
    using Contracts.SharedDevice;
    using log4net;

    /// <summary>A GDS coin acceptor.</summary>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.GdsDeviceBase" />
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.CoinAcceptor.ICoinAcceptorImplementation" />
    public class CoinAcceptorGds : GdsDeviceBase, ICoinAcceptorImplementation
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
    
        /// <summary>
        ///     Initializes a new instance of the Aristocrat.Monaco.Hardware.Gds.CoinAcceptor.CoinAcceptorGds class.
        /// </summary>
        public CoinAcceptorGds()
        {
            DeviceType = DeviceType.CoinAcceptor;
            RegisterCallback<CoinInStatus>(StatusReported);
            RegisterCallback<CoinInFaultStatus>(CoinInFaultReported);
        }

        /// <inheritdoc />
        public CoinFaultTypes Faults { get; set; }

        /// <inheritdoc />
        public event EventHandler<CoinEventType> CoinInStatusReported;

        /// <inheritdoc />
        public event EventHandler<CoinFaultTypes> FaultOccurred;

        /// <inheritdoc />
        public void CoinRejectMechOff()
        {
            SendCommand(new RejectMode { RejectOnOff = true });
        }

        /// <inheritdoc />
        public void CoinRejectMechOn()
        {
            SendCommand(new RejectMode { RejectOnOff = false });
        }

        /// <inheritdoc />
        public void DivertToCashbox()
        {
            SendCommand(new DivertorMode { DivertorOnOff = false });
        }

        /// <inheritdoc />
        public void DivertToHopper()
        {
            SendCommand(new DivertorMode { DivertorOnOff = true });
        }

        /// <inheritdoc />
        public override Task<bool> SelfTest(bool nvm)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override Task<bool> Reset()
        {
            DeviceReset();
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public void DeviceReset()
        {
            SendCommand(new DeviceReset());
        }

        /// <summary>Called when a coin in report is received.</summary>
        /// <param name="report">The report.</param>
        protected virtual void StatusReported(CoinInStatus report)
        {
            Logger.Debug($"CoinInStatus: {report}");

            PublishReport(report);
            CoinInStatusReported(this, report.EventType);
        }

        /// <summary>Called when a failure status report is received.</summary>
        /// <param name="report">The report.</param>
        protected virtual void CoinInFaultReported(CoinInFaultStatus report)
        {
            Logger.Debug($"FailureStatus: {report}");
            PublishReport(report);
            FaultOccurred(this, report.FaultType);
        }
    }
}
