namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.PackageManager;
    using Common.PackageManager.Storage;
    using ExpressMapper;

    /// <summary>
    ///     An implementation of <see cref="ICommandBuilder{IDownloadDevice, packageStatus}" />
    /// </summary>
    public class PackageStatusCommandBuilder : ICommandBuilder<IDownloadDevice, packageStatus>
    {
        private readonly IPackageManager _packageManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PackageStatusCommandBuilder" /> class.
        /// </summary>
        /// <param name="packageManager">Package manager instance.</param>
        public PackageStatusCommandBuilder(IPackageManager packageManager)
        {
            _packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
        }

        /// <inheritdoc />
        public async Task Build(IDownloadDevice device, packageStatus command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var pe = _packageManager.GetPackageLogEntity(command.pkgId);
            if (pe != null)
            {
                command.pkgState = (t_pkgStates)pe.State;
                command.pkgException = pe.Exception;
            }

            if (command.Item is packageActivityStatus packageActivityItem)
            {
                if (pe != null)
                {
                    if (pe.ActivityType.HasValue)
                    {
                        packageActivityItem.activityType = (t_pkgActivityTypes)pe.ActivityType.Value;
                    }

                    packageActivityItem.overwrite = pe.Overwrite;
                    packageActivityItem.reasonCode = pe.ReasonCode;

                    if (pe.ActivityDateTime.HasValue)
                    {
                        packageActivityItem.activityDateTimeSpecified = true;
                        packageActivityItem.activityDateTime = pe.ActivityDateTime.Value.UtcDateTime;
                    }
                }
            }
            else
            {
                var te = _packageManager.GetTransferEntity(command.pkgId);

                var packageTransferItem = te != null
                    ? Mapper.Map<TransferEntity, packageTransferStatus>(te)
                    : new packageTransferStatus();

                command.Item = packageTransferItem;

                if (te != null)
                {
                    if (pe == null)
                    {
                        command.pkgState = (t_pkgStates)te.State;
                    }
                }

                if (packageTransferItem.transferState == t_transferStates.G2S_error)
                {
                    var error = _packageManager.GetPackageErrorEntity(command.pkgId);
                    if (error != null)
                    {
                        packageTransferItem.transferException = error.ErrorCode;
                    }
                }

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

            await Task.CompletedTask;
        }
    }
}