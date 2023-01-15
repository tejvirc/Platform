namespace Aristocrat.Monaco.Application.Monitors
{
    using Contracts.Localization;
    using Hardware.Contracts.Door;
    using Kernel.MessageDisplay;
    using Kernel.Contracts.MessageDisplay;
    using Monaco.Localization.Properties;

    public partial class DoorMonitor
    {
        internal class DoorInfoWithMismatch : DoorInfo
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="DoorInfoWithMismatch" /> class.
            /// </summary>
            /// <param name="doorGuid">The string used to instantiate the guid of the door disable.</param>
            /// <param name="doorMeterName">The string name of the door meter.</param>
            /// <param name="powerOffMeter">The name of the power off door meter.</param>
            public DoorInfoWithMismatch(string doorGuid, string doorMeterName, string powerOffMeter)
                : base(doorGuid, doorMeterName, powerOffMeter)
            {
                if (doorGuid == MainDoorGuid)
                {
                    DoorMismatchMessage = new DisplayableMessage(
                            ResourceKeys.MainDoorMismatch,
                            CultureProviderType.Player,
                            DisplayableMessageClassification.HardError,
                            DisplayableMessagePriority.Immediate,
                            typeof(OpenEvent));
                }
                else if (doorGuid == TopBoxOpticDoorGuid)
                {
                    DoorMismatchMessage = new DisplayableMessage(
                           ResourceKeys.TopBoxDoorMismatch,
                           CultureProviderType.Player,
                           DisplayableMessageClassification.HardError,
                           DisplayableMessagePriority.Immediate,
                           typeof(OpenEvent));
                }
                else if (doorGuid == MainOpticDoorGuid)
                {
                    DoorMismatchMessage = new DisplayableMessage(
                            ResourceKeys.MainDoorMismatch,
                            CultureProviderType.Player,
                            DisplayableMessageClassification.HardError,
                            DisplayableMessagePriority.Immediate,
                            typeof(OpenEvent));
                }
                else if (doorGuid == TopBoxDoorGuid)
                {
                    DoorMismatchMessage = new DisplayableMessage(
                            ResourceKeys.TopBoxDoorMismatch,
                            CultureProviderType.Player,
                            DisplayableMessageClassification.HardError,
                            DisplayableMessagePriority.Immediate,
                            typeof(OpenEvent));
                }
                else if (doorGuid == UniversalInterfaceBoxDoorGuid)
                {
                    DoorMismatchMessage = new DisplayableMessage(
                        ResourceKeys.UniversalInterfaceBoxDoorMismatch,
                        CultureProviderType.Player,
                        DisplayableMessageClassification.HardError,
                        DisplayableMessagePriority.Immediate,
                        typeof(OpenEvent));
                }
                else
                {
                    DoorMismatchMessage = null;
                }
            }

            /// <summary>
            ///     The displayable message for door mismatch.
            /// </summary>
            public IDisplayableMessage DoorMismatchMessage { get; }
        }
    }
}