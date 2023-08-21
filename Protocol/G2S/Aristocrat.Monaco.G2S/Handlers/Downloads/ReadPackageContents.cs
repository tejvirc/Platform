namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.PackageManager;
    using Monaco.Common.Exceptions;
    using PackageManifest.Models;

    /// <summary>
    ///     Handles the v21.readPackageContents G2S message
    /// </summary>
    public class ReadPackageContents : ICommandHandler<download, readPackageContents>
    {
        private readonly IG2SEgm _egm;
        private readonly IPackageManager _packageManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReadPackageContents" /> class.
        ///     Creates a new instance of the ReadPackageContents handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="packageManager">Package Manager</param>
        public ReadPackageContents(IG2SEgm egm, IPackageManager packageManager)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, readPackageContents> command)
        {
            return await Sanction.OwnerAndGuests<IDownloadDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, readPackageContents> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var dl = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);
                if (dl == null)
                {
                    return;
                }

                var packageContents = GetPackageContents(command.Command.pkgId);
                if (packageContents != null)
                {
                    var response = command.GenerateResponse<packageContents>().Command;
                    response.moduleInfo = packageContents.moduleInfo;
                    response.pkgDescription = packageContents.pkgDescription;
                    response.pkgFormat = packageContents.pkgFormat;
                    response.pkgId = packageContents.pkgId;
                    response.pkgSize = packageContents.pkgSize;
                    response.pkgVerifyCode = packageContents.pkgVerifyCode;
                    response.releaseNum = packageContents.releaseNum;
                }
                else
                {
                    command.Error.Code = _packageManager.HasPackage(command.Command.pkgId)
                        ? ErrorCode.G2S_DLX011
                        : ErrorCode.G2S_DLX001;
                }
            }

            await Task.CompletedTask;
        }

        private packageContents GetPackageContents(string packageId)
        {
            var package = _packageManager.GetPackageEntity(packageId);
            if (package != null)
            {
                try
                {
                    var manifest = _packageManager.ReadManifest(packageId);
                    if (manifest == null)
                    {
                        return null;
                    }

                    return new packageContents
                    {
                        pkgId = packageId,
                        pkgFormat = t_pkgFormats.G2S_zip,
                        pkgSize = package.Size,
                        releaseNum = string.IsNullOrEmpty(manifest.Version) ? "1" : manifest.Version,
                        moduleInfo = new[]
                        {
                            new moduleInfo
                            {
                                modId = manifest.Name + ".iso",
                                modReleaseNum = string.IsNullOrEmpty(manifest.Version) ? "1" : manifest.Version,
                                modType = FromManifest(manifest)
                            }
                        }
                    };
                }
                catch (CommandException)
                {
                    return null;
                }
            }

            return null;
        }

        private static t_modTypes FromManifest(Image manifest)
        {
            switch (manifest.Type)
            {
                case "game":
                    return t_modTypes.G2S_game;
                case "os":
                    return t_modTypes.G2S_os;
                case "firmware":
                    return t_modTypes.G2S_firmware;
                default:
                    return t_modTypes.G2S_other;
            }
        }
    }
}
