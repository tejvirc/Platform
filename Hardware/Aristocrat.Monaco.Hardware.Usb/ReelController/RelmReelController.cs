namespace Aristocrat.Monaco.Hardware.Usb.ReelController
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Contracts.Communicator;
    using Contracts.Gds.Reel;
    using Contracts.Reel;
    using Contracts.Reel.Events;
    using Contracts.Reel.ImplementationCapabilities;
    using Contracts.SharedDevice;
    using log4net;

    /// <summary>
    ///     The Relm Reel Controller control class
    /// </summary>
    public class RelmReelController : IReelControllerImplementation
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        ///     Instantiates and instance of the RelmReelController class
        /// </summary>
        public RelmReelController()
        {
            Logger.Debug("CSW - RelmReelController instantiated");
        }

#pragma warning disable 67
        /// <inheritdoc />
        public event EventHandler<ProgressEventArgs> DownloadProgressed;
        
        /// <inheritdoc />
        public event EventHandler<EventArgs> Initialized;
        
        /// <inheritdoc />
        public event EventHandler<EventArgs> InitializationFailed;
        
        /// <inheritdoc />
        public event EventHandler<EventArgs> Enabled;
        
        /// <inheritdoc />
        public event EventHandler<EventArgs> Disabled;
        
        /// <inheritdoc />
        public event EventHandler<EventArgs> Connected;
        
        /// <inheritdoc />
        public event EventHandler<EventArgs> Disconnected;
        
        /// <inheritdoc />
        public event EventHandler<EventArgs> ResetSucceeded;
        
        /// <inheritdoc />
        public event EventHandler<EventArgs> ResetFailed;
        
        /// <inheritdoc />
        public event EventHandler<ReelControllerFaultedEventArgs> ControllerFaultOccurred;
        
        /// <inheritdoc />
        public event EventHandler<ReelControllerFaultedEventArgs> ControllerFaultCleared;
        
        /// <inheritdoc />
        public event EventHandler<ReelFaultedEventArgs> FaultOccurred;
        
        /// <inheritdoc />
        public event EventHandler<ReelFaultedEventArgs> FaultCleared;
        
        /// <inheritdoc />
        public event EventHandler<ReelEventArgs> ReelStopping;
        
        /// <inheritdoc />
        public event EventHandler<ReelEventArgs> ReelStopped;
        
        /// <inheritdoc />
        public event EventHandler<ReelEventArgs> ReelSpinning;
        
        /// <inheritdoc />
        public event EventHandler<ReelEventArgs> ReelSlowSpinning;
        
        /// <inheritdoc />
        public event EventHandler<ReelEventArgs> ReelDisconnected;
        
        /// <inheritdoc />
        public event EventHandler<ReelEventArgs> ReelConnected;
        
        /// <inheritdoc />
        public event EventHandler HardwareInitialized;
#pragma warning restore 67
        
        /// <inheritdoc />
        public int VendorId { get; }
        
        /// <inheritdoc />
        public int ProductId { get; }
        
        /// <inheritdoc />
        public bool IsDfuCapable { get; }
        
        /// <inheritdoc />
        public bool IsDfuInProgress { get; }
        
        /// <inheritdoc />
        public bool IsConnected { get; }
        
        /// <inheritdoc />
        public bool IsInitialized { get; }
        
        /// <inheritdoc />
        public bool IsEnabled { get; }
        
        /// <inheritdoc />
        public int Crc { get; }
        
        /// <inheritdoc />
        public string Protocol { get; }
        
        /// <inheritdoc />
        public IReadOnlyCollection<int> ReelIds { get; }
        
        /// <inheritdoc />
        public ReelControllerFaults ReelControllerFaults { get; }
        
        /// <inheritdoc />
        public IReadOnlyDictionary<int, ReelFaults> Faults { get; }
        
        /// <inheritdoc />
        public IReadOnlyDictionary<int, ReelStatus> ReelsStatus { get; }
        
        /// <inheritdoc />
        public void Dispose()
        {
            Logger.Debug("CSW - Calling dispose.  This is doing to crash!");
            throw new NotImplementedException();
        }
        
        /// <inheritdoc />
        public Task<bool> Initialize(ICommunicator communicator)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc />
        public Task<bool> Detach()
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc />
        public Task<bool> Reconnect()
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc />
        public Task<DfuStatus> Download(Stream firmware)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc />
        public Task<DfuStatus> Upload(Stream firmware)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc />
        public void Abort()
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc />
        public bool Open()
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc />
        public bool Close()
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc />
        public Task<bool> Enable()
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc />
        public Task<bool> Disable()
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc />
        public Task<bool> SelfTest(bool nvm)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc />
        public Task<int> CalculateCrc(int seed)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc />
        public void UpdateConfiguration(IDeviceConfiguration internalConfiguration)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc />
        public Task<bool> HomeReels()
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc />
        public Task<bool> HomeReel(int reelId, int stop, bool resetStatus = true)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc />
        public Task<bool> SetReelOffsets(params int[] offsets)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc />
        public Task<bool> TiltReels()
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc />
        public IEnumerable<Type> GetCapabilities()
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc />
        public T GetCapability<T>() where T : class, IReelImplementationCapability
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc />
        public bool HasCapability<T>() where T : class, IReelImplementationCapability
        {
            throw new NotImplementedException();
        }
    }
}
