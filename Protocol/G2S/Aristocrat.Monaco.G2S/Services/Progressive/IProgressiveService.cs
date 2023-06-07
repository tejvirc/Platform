namespace Aristocrat.Monaco.G2S.Services.Progressive
{ using Aristocrat.G2S.Client.Devices;
    using Kernel;

    public interface IProgressiveService : IService, IProgressiveDeviceManager, IProgressiveLevelManager { }
}
