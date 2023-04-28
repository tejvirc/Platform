namespace Aristocrat.Monaco.Hardware.Usb.ReelController.Relm
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Contracts.Reel.ImplementationCapabilities;

    internal class ReelBrightnessImplementation : IReelBrightnessImplementation
    {
        public Task<bool> SetBrightness(IReadOnlyDictionary<int, int> brightness)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetBrightness(int brightness)
        {
            throw new NotImplementedException();
        }
    }
}
