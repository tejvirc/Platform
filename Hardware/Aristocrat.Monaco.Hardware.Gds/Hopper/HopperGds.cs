namespace Aristocrat.Monaco.Hardware.Gds.Hopper
{
    using Aristocrat.Monaco.Hardware.Contracts.Hopper;
    using log4net;
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Contracts.SharedDevice;
    using Aristocrat.Monaco.Hardware.Contracts.Gds.Hopper;
    using Aristocrat.Monaco.Hardware.Contracts.Gds.CoinAcceptor;
    using Aristocrat.Monaco.Hardware.Contracts.Gds;

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
            DeviceType = DeviceType.CoinAcceptor;

        }

        /// <inheritdoc/>
        public HopperFaultTypes Faults { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


        /// <inheritdoc/>
        public bool DisableHopper()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool EnableHopper()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public byte GetStatusReport()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool HopperTestDisconnection()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override Task<bool> SelfTest(bool nvm)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool SetHopperType(HopperType type)
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
        protected override Task<bool> Reset()
        {
            SendCommand(new GdsSerializableMessage(GdsConstants.ReportId.DeviceReset));
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public void DeviceReset()
        {
            //TBD : Need to move DeviceReset to common place
            SendCommand(new DeviceReset());
        }

    }
}
