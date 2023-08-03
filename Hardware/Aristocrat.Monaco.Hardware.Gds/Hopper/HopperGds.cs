namespace Aristocrat.Monaco.Hardware.Gds.Hopper
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Contracts.Gds;
    using Contracts.Gds.CoinAcceptor;
    using Contracts.Gds.Hopper;
    using Contracts.Hopper;
    using Contracts.SharedDevice;
    using log4net;


    /// <summary>A GDS hopper.</summary>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.GdsDeviceBase" />
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.Hopper.IHopperImplementation" />
    public class HopperGds : GdsDeviceBase, IHopperImplementation
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        ///     Initializes a new instance of the Aristocrat.Monaco.Hardware.Gds.Hopper.HopperGds class.
        /// </summary>
        public HopperGds()
        {
            DeviceType = DeviceType.Hopper;
            RegisterCallback<CoinOutStatus>(StatusReported);
            RegisterCallback<HopperFaultStatus>(HopperFaultReported);

        }

        /// <inheritdoc/>
        public HopperFaultTypes Faults { get; set; }


        /// <inheritdoc />
        public event EventHandler<CoinOutEventType> CoinOutStatusReported;

        /// <inheritdoc />
        public event EventHandler<HopperFaultTypes> FaultOccurred;

        /// <inheritdoc/>
        public byte GetStatusReport()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override Task<bool> SelfTest(bool nvm)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SetMaxCoinoutAllowed(int amount)
        {
            SendCommand(new HopperMaxOutControl { Count = amount});
        }

        /// <inheritdoc/>
        public void StartHopperMotor()
        {
            SendCommand(new HopperMotorControl { OnOff = true });
        }

        /// <inheritdoc/>
        public void StopHopperMotor()
        {
            SendCommand(new HopperMotorControl { OnOff = false });
        }

        /// <inheritdoc />
        public void DeviceReset()
        {
            //TBD : Need to move DeviceReset to common place
            SendCommand(new DeviceReset());
        }

        /// <inheritdoc />
        protected override Task<bool> Reset()
        {
            SendCommand(new GdsSerializableMessage(GdsConstants.ReportId.DeviceReset));
            return Task.FromResult(true);
        }

        /// <summary>Called when a coin in report is received.</summary>
        /// <param name="report">The report.</param>
        protected virtual void StatusReported(CoinOutStatus report)
        {
            Logger.Debug($"CoinOutStatus: {report}");

            PublishReport(report);
            CoinOutStatusReported(this, report.Legal ?
                CoinOutEventType.LegalCoinOut :
                CoinOutEventType.IllegalCoinOut);
        }

        /// <summary>Called when a failure status report is received.</summary>
        /// <param name="report">The report.</param>
        protected virtual void HopperFaultReported(HopperFaultStatus report)
        {
            Logger.Debug($"FailureStatus: {report}");
            PublishReport(report);
            FaultOccurred(this, report.FaultType);
        }
    }
}
