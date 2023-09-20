namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using ExpressMapper;

    /// <summary>
    ///     Extension methods for download-related commands
    /// </summary>
    public static class DownloadExtensions
    {
        private const long TransactionIdError = 99999999L;

        /// <summary>
        ///     Converts a <see cref="PackageLog" /> instance to a <see cref="packageLog" />
        /// </summary>
        /// <param name="this">The <see cref="PackageLog" /> instance to convert.</param>
        /// <returns>A <see cref="packageLog" /> instance.</returns>
        public static packageLog ToPackageLog(this PackageLog @this)
        {
            var command = new packageLog
            {
                pkgState = (t_pkgStates)@this.State,
                pkgException = @this.Exception,
                pkgId = @this.PackageId,
                transactionId = @this.TransactionId > 0 ? @this.TransactionId : TransactionIdError,
                logSequence = @this.Id,
                deviceId = @this.DeviceId
            };


            if (@this.ActivityType.HasValue)
            {
                var packageActivityItem = new packageActivityLog();
                command.Item = packageActivityItem;

                packageActivityItem.activityType = (t_pkgActivityTypes)@this.ActivityType.Value;

                packageActivityItem.overwrite = @this.Overwrite;
                packageActivityItem.reasonCode = @this.ReasonCode;

                if (@this.ActivityDateTime.HasValue)
                {
                    packageActivityItem.activityDateTimeSpecified = true;
                    packageActivityItem.activityDateTime = @this.ActivityDateTime.Value.UtcDateTime;
                }
            }
            else
            {
                var packageTransferItem = Mapper.Map<PackageLog, packageTransferLog>(@this);

                command.Item = packageTransferItem;
                command.pkgState = (t_pkgStates)@this.State;

                packageTransferItem.transferException = @this.ErrorCode;

                if (string.IsNullOrEmpty(packageTransferItem.transferLocation))
                {
                    packageTransferItem.transferLocation = "pending";
                }
            }

            if (command.pkgState == t_pkgStates.G2S_error)
            {
                if (command.pkgException == 0)
                {
                    command.pkgException = 1;
                }
            }

            return command;
        }
    }
}