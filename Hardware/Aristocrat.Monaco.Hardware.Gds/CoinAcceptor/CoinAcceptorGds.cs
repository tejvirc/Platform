namespace Aristocrat.Monaco.Hardware.Gds.CoinAcceptor
{
    using Aristocrat.Monaco.Hardware.Contracts;
    using Aristocrat.Monaco.Kernel;
    using System;
    using System.Threading.Tasks;
    using Contracts.SharedDevice;
    using log4net;
    using System.Reflection;
    using Aristocrat.Monaco.Hardware.Contracts.PWM;

    /// <summary>
    /// 
    /// </summary>
    public class CoinAcceptorGds : GdsDeviceBase,
        ICoinAcceptorImplementation
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IPropertiesManager _propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<CoinEventType> CoinInStatusReported;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<CoinFaultTypes> FaultOccurred;

        /// <summary>
        /// 
        /// </summary>
        public CoinAcceptorGds()
        {
            DeviceType = DeviceType.CoinAcceptor;
            RegisterCallback<CoinInStatus>(StatusReported);
            RegisterCallback<CoinInFaultStatus>(CoinInFaultReported);    
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void CoinRejectMechOff()
        {
            SendCommand(new RejectMode { RejectOnOff = true });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void CoinRejectMechOn()
        {
            SendCommand(new RejectMode { RejectOnOff = false });
        }

        /// <summary>
        /// 
        /// </summary>
        public void DivertMechanismOnOff()
        {
            //TODO: implement hopper's properties with realtime values once hopper feature is available..
            bool isHopperInstalled = true;
            bool isHopperFull = false;

            if (_propertiesManager.GetValue(HardwareConstants.HopperEnabledKey, false) && isHopperInstalled && (!isHopperFull))
            {
                DivertToHopper();
            }
            else
            {
                DivertToCashbox();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void DivertToCashbox()
        {
            SendCommand(new DivertorMode { DivertorOnOff = false });

        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void DivertToHopper()
        {
            SendCommand(new DivertorMode { DivertorOnOff = true });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nvm"></param>
        /// <returns></returns>
        public override Task<bool> SelfTest(bool nvm)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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
