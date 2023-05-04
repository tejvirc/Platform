namespace Aristocrat.Monaco.Hardware.Usb.ReelController.Relm
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Contracts.Communicator;
    using Contracts.Reel.ImplementationCapabilities;

    internal class RelmBrightness : IReelBrightnessImplementation
    {
        private readonly IRelmCommunicator _communicator;

        public RelmBrightness(IRelmCommunicator communicator)
        {
            _communicator = communicator;
        }

        public Task<bool> SetBrightness(IReadOnlyDictionary<int, int> brightness)
        {
            return _communicator.SetBrightness(brightness);
        }

        public Task<bool> SetBrightness(int brightness)
        {
            return _communicator.SetBrightness(brightness);
        }
    }
}
