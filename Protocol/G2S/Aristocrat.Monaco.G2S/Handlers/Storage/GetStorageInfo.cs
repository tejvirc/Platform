namespace Aristocrat.Monaco.G2S.Handlers.Storage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using NativeOS.Services.OS;

    public class GetStorageInfo : ICommandHandler<storage, getStorageInfo>
    {
        private const string RamDescription = @"RAM";

        private readonly IG2SEgm _egm;

        private readonly Dictionary<string, storagePurpose[]> _purposes = new()
        {
            { RamDescription, new[] { new storagePurpose { application = t_storageApplication.G2S_os } } },
            {
                "Windows", new[]
                {
                    new storagePurpose { application = t_storageApplication.G2S_boot },
                    new storagePurpose { application = t_storageApplication.G2S_os }
                }
            },
            {
                "Data", new[]
                {
                    new storagePurpose { application = t_storageApplication.G2S_game },
                    new storagePurpose { application = t_storageApplication.G2S_download },
                    new storagePurpose { application = t_storageApplication.G2S_configuration }
                }
            }
        };

        public GetStorageInfo(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        public async Task<Error> Verify(ClassCommand<storage, getStorageInfo> command)
        {
            var error = await Sanction.OwnerAndGuests<IStorageDevice>(_egm, command);
            if (error != null && error.IsError)
            {
                return error;
            }

            var device = _egm.GetDevice<ICabinetDevice>();
            if (!device.IsMatching(command.Command.deviceClass) || !device.IsMatching(command.Command.deviceId))
            {
                return new Error(ErrorCode.GTK_STX002);
            }

            return null;
        }

        public async Task Handle(ClassCommand<storage, getStorageInfo> command)
        {
            var response = command.GenerateResponse<storageInfoList>();

            var cabinet = _egm.GetDevice<ICabinetDevice>();

            var storageInfo = new List<storageItem>();

            var storageId = 1;

            var info = SystemPerformanceProvider.GetSystemPerformance();
            storageInfo.Add(
                new storageItem
                {
                    deviceClass = cabinet.PrefixedDeviceClass(),
                    deviceId = cabinet.Id,
                    storageId = storageId,
                    persistence = false,
                    description = RamDescription,
                    maxFileSize = 0,
                    used = info.PhysicalTotal - info.PhysicalAvailable,
                    free = info.PhysicalAvailable,
                    storageType = "GTK_ram",
                    storagePurpose = _purposes[RamDescription]
                });

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive == null || !drive.IsReady || drive.DriveType != DriveType.Fixed)
                {
                    continue;
                }

                if (!_purposes.TryGetValue(drive.VolumeLabel, out var purpose))
                {
                    purpose = new[] { new storagePurpose { application = t_storageApplication.G2S_other } };
                }

                storageInfo.Add(
                    new storageItem
                    {
                        deviceClass = cabinet.PrefixedDeviceClass(),
                        deviceId = cabinet.Id,
                        storageId = ++storageId,
                        persistence = true,
                        description = string.IsNullOrEmpty(drive.VolumeLabel) ? drive.Name : drive.VolumeLabel,
                        maxFileSize = long.MaxValue,
                        used = drive.TotalSize - drive.AvailableFreeSpace,
                        free = drive.AvailableFreeSpace,
                        storageType = "GTK_hdd",
                        storagePurpose = purpose
                    });
            }

            if (command.Command.storageId != DeviceId.All &&
                storageInfo.All(s => s.storageId != command.Command.storageId))
            {
                command.Error.SetErrorCode(ErrorCode.GTK_STX001);
                return;
            }

            response.Command.storageItem = storageInfo.Where(item => Filter(command.Command, item)).ToArray();

            await Task.CompletedTask;
        }

        private bool Filter(c_getStorageInfoType command, c_storageInfoItem item)
        {
            var device = _egm.GetDevice(item.deviceClass.TrimmedDeviceClass(), item.deviceId);

            if (!device.IsMatching(command.deviceClass))
            {
                return false;
            }

            if (!device.IsMatching(command.deviceId))
            {
                return false;
            }

            return command.storageId == DeviceId.All || command.storageId == item.storageId;
        }
    }
}