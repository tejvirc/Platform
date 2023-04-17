namespace Aristocrat.Monaco.Hardware.Reel
{
    using System;
    using System.Collections.Generic;
    using Capabilities;
    using Contracts.Reel;
    using Contracts.Reel.Capabilities;
    using Contracts.Reel.ImplementationCapabilities;

    internal static class ReelCapabilitiesFactory
    {
        public static (Type CapabilityType, IReelControllerCapability Capability) Create(
            Type implementationType,
            IReelControllerImplementation controllerImplementation,
            ReelControllerStateManager stateManager)
        {
            if (implementationType == typeof(IReelBrightnessImplementation))
            {
                return (typeof(IReelBrightnessCapabilities),
                    new ReelBrightnessCapability(controllerImplementation.GetCapability<IReelBrightnessImplementation>(), stateManager));
            }

            if (implementationType == typeof(IReelLightingImplementation))
            {
                return (typeof(IReelLightingCapabilities),
                    new ReelLightingCapability(controllerImplementation.GetCapability<IReelLightingImplementation>(), stateManager));
            }

            if (implementationType == typeof(IReelSpinImplementation))
            {
                return (typeof(IReelSpinCapabilities),
                    new ReelSpinCapability(controllerImplementation.GetCapability<IReelSpinImplementation>(), stateManager));
            }

            return (null, null);
        }

        public static IEnumerable<KeyValuePair<Type, IReelControllerCapability>> CreateAll(
            IReelControllerImplementation implementation,
            ReelControllerStateManager stateManager)
        {
            List<KeyValuePair<Type, IReelControllerCapability>> capabilities = new();
            foreach (var implementationCapability in implementation.GetCapabilities())
            {
                var (capabilityType, capability) = Create(implementationCapability, implementation, stateManager);
                if (capabilityType is not null)
                {
                    capabilities.Add(new KeyValuePair<Type, IReelControllerCapability>(capabilityType, capability));
                }
            }

            return capabilities;
        }
    }
}
