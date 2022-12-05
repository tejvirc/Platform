namespace Aristocrat.Monaco.Application.Monitors
{
    using Contracts.Localization;
    using Hardware.Contracts.Door;
    using Kernel;
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
                            () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MainDoorMismatch),
                            DisplayableMessageClassification.HardError,
                            DisplayableMessagePriority.Immediate,
                            typeof(OpenEvent));
                }
                else if (doorGuid == TopBoxOpticDoorGuid)
                {
                    DoorMismatchMessage = new DisplayableMessage(
                           () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TopBoxDoorMismatch),
                           DisplayableMessageClassification.HardError,
                           DisplayableMessagePriority.Immediate,
                           typeof(OpenEvent));
                }
                else if (doorGuid == MainOpticDoorGuid)
                {
                    DoorMismatchMessage = new DisplayableMessage(
                            () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MainDoorMismatch),
                            DisplayableMessageClassification.HardError,
                            DisplayableMessagePriority.Immediate,
                            typeof(OpenEvent));
                }
                else if (doorGuid == TopBoxDoorGuid)
                {
                    DoorMismatchMessage = new DisplayableMessage(
                            () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TopBoxDoorMismatch),
                            DisplayableMessageClassification.HardError,
                            DisplayableMessagePriority.Immediate,
                            typeof(OpenEvent));
                }
                else if (doorGuid == UniversalInterfaceBoxDoorGuid)
                {
                    DoorMismatchMessage = new DisplayableMessage(
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UniversalInterfaceBoxDoorMismatch),
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
            public DisplayableMessage DoorMismatchMessage { get; }
        }
    }
}