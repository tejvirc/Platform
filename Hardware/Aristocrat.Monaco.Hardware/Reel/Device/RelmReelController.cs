namespace Aristocrat.Monaco.Hardware.Reel.Device
{
    using Contracts.Communicator;
    using Contracts.Gds.Reel;
    using Contracts.Reel;
    using Contracts.Reel.Events;
    using Contracts.Reel.ImplementationCapabilities;
    using Contracts.SharedDevice;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    ///     The Relm Reel Controller control class
    /// </summary>
    public class RelmReelController : IReelControllerImplementation
    {
#pragma warning disable 67
        public event EventHandler<ProgressEventArgs> DownloadProgressed;

        public event EventHandler<EventArgs> Initialized;

        public event EventHandler<EventArgs> InitializationFailed;

        public event EventHandler<EventArgs> Enabled;

        public event EventHandler<EventArgs> Disabled;

        public event EventHandler<EventArgs> Connected;

        public event EventHandler<EventArgs> Disconnected;

        public event EventHandler<EventArgs> ResetSucceeded;

        public event EventHandler<EventArgs> ResetFailed;

        public event EventHandler<ReelControllerFaultedEventArgs> ControllerFaultOccurred;

        public event EventHandler<ReelControllerFaultedEventArgs> ControllerFaultCleared;

        public event EventHandler<ReelFaultedEventArgs> FaultOccurred;

        public event EventHandler<ReelFaultedEventArgs> FaultCleared;

        public event EventHandler<ReelEventArgs> ReelStopping;

        public event EventHandler<ReelEventArgs> ReelStopped;

        public event EventHandler<ReelEventArgs> ReelSpinning;

        public event EventHandler<ReelEventArgs> ReelSlowSpinning;

        public event EventHandler<ReelEventArgs> ReelDisconnected;

        public event EventHandler<ReelEventArgs> ReelConnected;

        public event EventHandler HardwareInitialized;
#pragma warning restore 67

        public int VendorId { get; }

        public int ProductId { get; }

        public bool IsDfuCapable { get; }

        public bool IsDfuInProgress { get; }

        public bool IsConnected { get; }

        public bool IsInitialized { get; }

        public bool IsEnabled { get; }

        public int Crc { get; }

        public string Protocol { get; }

        public IReadOnlyCollection<int> ReelIds { get; }

        public ReelControllerFaults ReelControllerFaults { get; }

        public IReadOnlyDictionary<int, ReelFaults> Faults { get; }

        public IReadOnlyDictionary<int, ReelStatus> ReelsStatus { get; }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Initialize(ICommunicator communicator)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Detach()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Reconnect()
        {
            throw new NotImplementedException();
        }

        public Task<DfuStatus> Download(Stream firmware)
        {
            throw new NotImplementedException();
        }

        public Task<DfuStatus> Upload(Stream firmware)
        {
            throw new NotImplementedException();
        }

        public void Abort()
        {
            throw new NotImplementedException();
        }

        public bool Open()
        {
            throw new NotImplementedException();
        }

        public bool Close()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Enable()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Disable()
        {
            throw new NotImplementedException();
        }

        public Task<bool> SelfTest(bool nvm)
        {
            throw new NotImplementedException();
        }

        public Task<int> CalculateCrc(int seed)
        {
            throw new NotImplementedException();
        }

        public void UpdateConfiguration(IDeviceConfiguration internalConfiguration)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HomeReels()
        {
            throw new NotImplementedException();
        }

        public Task<bool> HomeReel(int reelId, int stop, bool resetStatus = true)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetReelOffsets(params int[] offsets)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TiltReels()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Type> GetCapabilities()
        {
            throw new NotImplementedException();
        }

        public T GetCapability<T>() where T : class, IReelImplementationCapability
        {
            throw new NotImplementedException();
        }

        public bool HasCapability<T>() where T : class, IReelImplementationCapability
        {
            throw new NotImplementedException();
        }
    }
}
