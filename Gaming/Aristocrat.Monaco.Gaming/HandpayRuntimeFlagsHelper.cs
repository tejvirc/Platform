namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Contracts;
    using Kernel;
    using Runtime;
    using Runtime.Client;

    public class HandpayRuntimeFlagsHelper : IHandpayRuntimeFlagsHelper
    {
        private static readonly IReadOnlyList<Guid> DisableItems = new List<Guid>
        {
            ApplicationConstants.LiveAuthenticationDisableKey,
            ApplicationConstants.HandpayPendingDisableKey
        };

        private readonly ISystemDisableManager _systemDisableManager;
        private readonly IRuntime _runtime;
        private readonly IOverlayMessageStrategyController _overlayController;

        public HandpayRuntimeFlagsHelper(
            IRuntime runtime,
            ISystemDisableManager systemDisableManager,
            IOverlayMessageStrategyController overlayController)
        {
            _systemDisableManager = systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _overlayController = overlayController ?? throw new ArgumentNullException(nameof(overlayController));
        }

        public void SetHandpayRuntimeLockupFlags()
        {
            if (!_systemDisableManager.IsDisabled)
            {
                return;
            }

            if (InHandpayLockup && HandpayAnimationsEnabled())
            {
                // allow animating the overlay while in a lockup
                _runtime.UpdateFlag(RuntimeCondition.InOverlayLockup, true);
                _runtime.UpdateFlag(RuntimeCondition.InLockup, false);
            }
            else
            {
                // either not animating or another immediate lockup is stopping us
                _runtime.UpdateFlag(RuntimeCondition.InOverlayLockup, false);
                _runtime.UpdateFlag(RuntimeCondition.InLockup, true);
            }
        }

        private bool InHandpayLockup => _systemDisableManager.CurrentImmediateDisableKeys.All(x => DisableItems.Contains(x));

        private bool HandpayAnimationsEnabled()
        {
            var useOverride =
                _overlayController.RegisteredPresentations.Contains(PresentationOverrideTypes.JackpotHandpay);

            // Check for any other immediate lockups that are not Handpay or LiveAuthenticationDisable type.
            var haveLockUpThatIsNotHandpay =
                _systemDisableManager.CurrentImmediateDisableKeys.Except(DisableItems).Any();

            return useOverride && !haveLockUpThatIsNotHandpay;
        }
    }
}