namespace Aristocrat.Bingo.Client.Messages
{
    using System;

    /// <inheritdoc />
    public class ActivityReportMessage : IMessage
    {
        /// <summary>
        ///     Creates an instance of <see cref="ActivityReportMessage"/>
        /// </summary>
        /// <param name="activityTime">The time for the activity</param>
        /// <param name="machineSerial">The machine serial for the activity</param>
        public ActivityReportMessage(DateTime activityTime, string machineSerial)
        {
            ActivityTime = activityTime;
            MachineSerial = machineSerial;
        }

        /// <summary>
        ///     Gets the activity time
        /// </summary>
        public DateTime ActivityTime { get; }

        /// <summary>
        ///     Gets the machine serial number for the activity message
        /// </summary>
        public string MachineSerial { get; }
    }
}