namespace Aristocrat.Monaco.Hardware.Reel.Capabilities
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Persistence;
    using Contracts.Reel.Capabilities;
    using Contracts.Reel.ImplementationCapabilities;

    internal class ReelBrightnessCapability : IReelBrightnessCapabilities,
        IStorageAccessor<ReelBrightnessCapabilityOptions>
    {
        private const string OptionsBlock = "Aristocrat.Monaco.Hardware.Reel.Capabilities.BrightnessCapability.Options";
        private const string ReelBrightnessOption = "ReelBrightness";
        private const int MinBrightness = 1;
        private const int MaxBrightness = 100;

        private readonly IReelBrightnessImplementation _implementation;
        private readonly ReelControllerStateManager _stateManager;

        private int _reelBrightness = MaxBrightness;

        public ReelBrightnessCapability(IReelBrightnessImplementation implementation, ReelControllerStateManager stateManager)
        {
            _implementation = implementation;
            _stateManager = stateManager;
        }

        public int DefaultReelBrightness
        {
            get => _reelBrightness;

            set
            {
                if (value is < MinBrightness or > MaxBrightness)
                {
                    return;
                }

                if (_reelBrightness == value)
                {
                    return;
                }

                _reelBrightness = value;
                this.ModifyBlock(
                    OptionsBlock,
                    (transaction, index) =>
                    {
                        transaction[index, ReelBrightnessOption] = _reelBrightness;
                        return true;
                    },
                    _stateManager.ControllerId - 1);
            }
        }

        public Task<bool> SetBrightness(IReadOnlyDictionary<int, int> brightness)
        {
            return _stateManager.CanSendCommand ? _implementation.SetBrightness(brightness) : Task.FromResult(false);
        }

        public Task<bool> SetBrightness(int brightness)
        {
            return _stateManager.CanSendCommand ? _implementation.SetBrightness(brightness) : Task.FromResult(false);
        }

        public bool TryAddBlock(IPersistentStorageAccessor accessor, int blockIndex, out ReelBrightnessCapabilityOptions block)
        {
            block = new ReelBrightnessCapabilityOptions { ReelBrightness = DefaultReelBrightness };

            using var transaction = accessor.StartTransaction();
            transaction[blockIndex, ReelBrightnessOption] = block.ReelBrightness;

            transaction.Commit();
            return true;
        }

        public bool TryGetBlock(IPersistentStorageAccessor accessor, int blockIndex, out ReelBrightnessCapabilityOptions block)
        {
            block = new ReelBrightnessCapabilityOptions
            {
                ReelBrightness = (int)accessor[blockIndex, ReelBrightnessOption],
            };

            return true;
        }
    }
}
