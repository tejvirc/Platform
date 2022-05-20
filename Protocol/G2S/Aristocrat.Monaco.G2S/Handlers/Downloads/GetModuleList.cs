namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.PackageManager;

    /// <summary>
    ///     Handles the v21.getModuleList G2S message
    /// </summary>
    public class GetModuleList : ICommandHandler<download, getModuleList>
    {
        private readonly IG2SEgm _egm;
        private readonly IPackageManager _packageManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetModuleList" /> class.
        ///     Creates a new instance of the GetModuleList handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="packageManager">Package Manager</param>
        public GetModuleList(IG2SEgm egm, IPackageManager packageManager)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, getModuleList> command)
        {
            return await Sanction.OwnerAndGuests<IDownloadDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, getModuleList> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var dl = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);
                if (dl == null)
                {
                    return;
                }

                /*
                 * moduleList Command
        10.41.1  Command Description
        This command is used by the EGM to report the list of modules to a host. The moduleList command is
        generated in response to the getModuleList command.
        An EGM manufacturer has the option to expose only the modules that it wants to. It is recommended that the
        EGM expose pertinent modules that may be updated or replaced via the download class. By doing so, the
        System Management Point has the option to evaluate what modules have outdated revisions (modReleaseNum),
        or to determine if the dependencies for a new package are already installed. If an EGM doesn't provide
        pertinent module information, then the System Management Point may decide to download and install
        packages that may already be installed on the EGM, which is inefficient use of network resources.
        The storageUsed element contains details about what storage mediums are used by the EGM for the
        associated module, with a relationship described that associates the storage medium to an actual G2S device.
        For example, games may reside upon a non-volatile storage medium owned by the EGM, which would be
        represented by the cabinet class and the associated cabinet device. On the other hand, modules that
        represent peripheral firmware might be associated to storage held on the peripheral itself, and therefore the
        associated storageUsed element may have values of deviceClass = "G2S_noteAcceptor" and deviceId =
        "1".
        
                TODO: 10.49.44 G2S_DLE305 Module Error 10.49.43 G2S_DLE304 Module Disabled
                 */

                var res = command.GenerateResponse<moduleList>();

                var list = new List<moduleStatus>();

                foreach (var me in _packageManager.ModuleEntityList)
                {
                    list.Add(_packageManager.ParseXml<moduleStatus>(me.Status));
                }

                res.Command.moduleStatus = list.ToArray();
            }

            await Task.CompletedTask;
        }
    }
}