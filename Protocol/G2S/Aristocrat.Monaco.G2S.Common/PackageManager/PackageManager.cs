namespace Aristocrat.Monaco.G2S.Common.PackageManager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Xml.Serialization;
    using Application.Contracts.Authentication;
    using G2S.Data.Model;
    using G2S.Data.Packages;
    using CommandHandlers;
    using Kernel;
    using Kernel.Contracts.Components;
    using log4net;
    using Monaco.Common.Storage;
    using Protocol.Common.Installer;
    using PackageManifest.Models;
    using Storage;
    using Transfer;
    using Module = Storage.Module;
    using DbContext = Microsoft.EntityFrameworkCore.DbContext;

    /// <summary>
    ///     Package manager implementation that facades Game To System Message Protocol.
    /// </summary>
    public class PackageManager : IPackageManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IAuthenticationService _componentHash;
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IModuleRepository _moduleRepository;
        private readonly IPackageErrorRepository _packageErrorRepository;
        private readonly IPackageLogRepository _packageLogs;
        private readonly IPackageRepository _packageRepository;
        private readonly IInstallerService _installerService;
        private readonly IPathMapper _pathMapper;
        private readonly IFileSystemProvider _fileSystemProvider;
        private readonly Dictionary<string, CancellationTokenSource> _packageTaskAbortTokens =
            new Dictionary<string, CancellationTokenSource>();

        private readonly IScriptRepository _scriptRepository;
        private readonly ITransferRepository _transferRepository;
        private readonly ITransferService _transferService;
        private readonly IComponentRegistry _componentRegistry;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PackageManager" /> class.
        /// </summary>
        /// <param name="contextFactory">An <see cref="IMonacoContextFactory" /> instance.</param>
        /// <param name="packageRepository">Package repository instance.</param>
        /// <param name="transferRepository">Transfer repository instance.</param>
        /// <param name="transferService">Transfer service instance.</param>
        /// <param name="pathMapper">Path mapper instance.</param>
        /// <param name="packageErrorRepository">Package errors repository.</param>
        /// <param name="moduleRepository">Module repository.</param>
        /// <param name="scriptRepository">Script repository</param>
        /// <param name="componentHash">Hash calculator</param>
        /// <param name="componentRegistry">Component Registry</param>
        /// <param name="packageLogs">Package logs.</param>
        /// <param name="installerService">Software installer service.</param>
        /// <param name="fileSystemProvider">File system provider.</param>
        public PackageManager(
            IMonacoContextFactory contextFactory,
            IPackageRepository packageRepository,
            ITransferRepository transferRepository,
            ITransferService transferService,
            IPathMapper pathMapper,
            IPackageErrorRepository packageErrorRepository,
            IScriptRepository scriptRepository,
            IModuleRepository moduleRepository,
            IAuthenticationService componentHash,
            IComponentRegistry componentRegistry,
            IPackageLogRepository packageLogs,
            IInstallerService installerService,
            IFileSystemProvider fileSystemProvider)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
            _transferRepository = transferRepository ?? throw new ArgumentNullException(nameof(transferRepository));
            _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));
            _packageErrorRepository =
                packageErrorRepository ?? throw new ArgumentNullException(nameof(packageErrorRepository));
            _scriptRepository = scriptRepository ?? throw new ArgumentNullException(nameof(scriptRepository));
            _moduleRepository = moduleRepository ?? throw new ArgumentNullException(nameof(moduleRepository));
            _componentHash = componentHash ?? throw new ArgumentNullException(nameof(componentHash));
            _componentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
            _packageLogs = packageLogs ?? throw new ArgumentNullException(nameof(packageLogs));
            _installerService = installerService ?? throw new ArgumentNullException(nameof(installerService));
            _fileSystemProvider = fileSystemProvider ?? throw new ArgumentNullException(nameof(fileSystemProvider));
        }

        /// <inheritdoc />
        public IDictionary<string, CancellationTokenSource> PackageTaskAbortTokens => _packageTaskAbortTokens;

        /// <inheritdoc />
        public int PackageCount => _packageRepository?.Count(_contextFactory.CreateDbContext()) ?? 0;

        /// <inheritdoc />
        public int ScriptCount => _scriptRepository?.Count(_contextFactory.CreateDbContext()) ?? 0;
        
        /// <inheritdoc />
        public IReadOnlyCollection<Package> PackageEntityList =>
            new List<Package>(_packageRepository.GetAll(_contextFactory.CreateDbContext()));

        /// <inheritdoc />
        public IReadOnlyCollection<Script> ScriptEntityList =>
            new List<Script>(_scriptRepository.GetAll(_contextFactory.CreateDbContext()));

        /// <inheritdoc />
        public IReadOnlyCollection<TransferEntity> TransferEntityList =>
            new List<TransferEntity>(_transferRepository.GetAll(_contextFactory.CreateDbContext()));

        /// <inheritdoc />
        public IReadOnlyCollection<Module> ModuleEntityList =>
            new List<Module>(_moduleRepository.GetAll(_contextFactory.CreateDbContext()));

        /// <inheritdoc />
        public CreatePackageState CreatePackage(PackageLog packageEntity, Module module, bool overwrite, string format)
        {
            var handler = new CreatePackageHandler(
                _contextFactory,
                _packageErrorRepository,
                _installerService,
                UpdatePackageLog);

            return handler.Execute(new CreatePackageArgs(packageEntity, module, overwrite, format));
        }

        /// <inheritdoc />
        public void InstallPackage(
            Package package,
            Action<InstallPackageArgs> changeStatusCallback,
            bool deleteAfter)
        {
            var handler = new InstallPackageHandler(
                _contextFactory,
                _packageRepository,
                _packageErrorRepository,
                _installerService);

            handler.Execute(new InstallPackageArgs(package, changeStatusCallback, deleteAfter));
        }

        /// <inheritdoc />
        public void UninstallModule(Module module, Action<UninstallModuleArgs> changeStatusCallback)
        {
            var handler = new UninstallModuleHandler(
                _contextFactory,
                _moduleRepository,
                _packageErrorRepository,
                _installerService);

            handler.Execute(new UninstallModuleArgs(module, changeStatusCallback));
        }

        /// <inheritdoc />
        public void UninstallPackage(
            Package package,
            Action<InstallPackageArgs> changeStatusCallback,
            bool deleteAfter)
        {
            var handler = new UninstallPackageHandler(
                _contextFactory,
                _moduleRepository,
                _packageErrorRepository,
                _installerService);

            handler.Execute(new InstallPackageArgs(package, changeStatusCallback, deleteAfter));
        }

        /// <inheritdoc />
        public void UploadPackage(
            string packageId,
            TransferEntity transfer,
            Action<PackageTransferEventArgs> changeStatusCallback,
            CancellationToken ct,
            PackageLog packageLog)
        {
            var handler = new UploadPackageHandler(
                _contextFactory,
                _packageRepository,
                _transferRepository,
                _transferService,
                _packageErrorRepository,
                _installerService,
                _fileSystemProvider);

            handler.Execute(new TransferPackageArgs(packageId, transfer, changeStatusCallback, ct, packageLog));
        }

        /// <inheritdoc />
        public void AddPackageLog(PackageLog packageLog)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                AddPackageLog(packageLog, context);
            }
        }

        /// <inheritdoc />
        public void UpdatePackage(PackageLog packageLog)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                UpdatePackageLog(packageLog, context);
            }
        }

        /// <inheritdoc />
        public bool UpdateModuleEntity(Module module)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var me = _moduleRepository.GetModuleByModuleId(context, module.ModuleId);

                if (me != null)
                {
                    me.PackageId = module.PackageId;
                    me.Status = module.Status;
                    _moduleRepository.Update(context, me);
                }
                else
                {
                    _moduleRepository.Add(context, module);
                    return true;
                }
            }

            Logger.Info("Updated Module=" + module);
            return false;
        }

        /// <inheritdoc />
        public void UpdateTransferEntity(TransferEntity entity)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                UpdateTransferEntity(entity, context);
            }
        }

        /// <inheritdoc />
        public void UpdateScript(Script script)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var se = _scriptRepository.GetScriptByScriptId(context, script.ScriptId);

                if (se == null)
                {
                    _scriptRepository.Add(context, script);
                }
                else
                {
                    se.State = script.State;
                    se.AuthorizeDateTime = script.AuthorizeDateTime;
                    se.CommandData = script.CommandData;
                    se.ScriptException = script.ScriptException;
                    se.CompletedDateTime = script.CompletedDateTime;
                    se.ScriptCompleteHostAcknowledged = script.ScriptCompleteHostAcknowledged;

                    se.AuthorizeItems = script.AuthorizeItems;

                    _scriptRepository.Update(context, se);
                }
            }

            Logger.Info("Updated Script=" + script);
        }

        /// <inheritdoc />
        public void DeletePackage(DeletePackageArgs deletePackageArgs)
        {
            var handler = new DeletePackageHandler(
                _contextFactory,
                _packageErrorRepository,
                _packageLogs,
                _packageRepository,
                _installerService,
                UpdatePackageLog);

            handler.Execute(deletePackageArgs);
        }

        /// <inheritdoc />
        public bool ValidatePackage(string filePath)
        {
            var handler = new ValidatePackageHandler(
                _contextFactory,
                _packageErrorRepository,
                _installerService);

            return handler.Execute(filePath);
        }

        /// <inheritdoc />
        public PackageLog GetPackageLogEntity(string packageId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return _packageLogs.GetLastPackageLogeByPackageId(context, packageId);
            }
        }

        /// <inheritdoc />
        public Package GetPackageEntity(string packageId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return _packageRepository.GetPackageByPackageId(context, packageId);
            }
        }

        /// <inheritdoc />
        public PackageError GetPackageErrorEntity(string packageId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return _packageErrorRepository.GetByPackageId(context, packageId).OrderByDescending(a => a.Id)
                    .FirstOrDefault();
            }
        }

        /// <inheritdoc />
        public void VerifyPackages()
        {
            var handler = new VerifyPackagesHandler(
                _packageRepository,
                _componentHash,
                _pathMapper,
                _contextFactory,
                components =>
                {
                    foreach (var component in components)
                    {
                        _componentRegistry.Register(component);
                    }
                });

            handler.Execute();
        }

        /// <inheritdoc />
        public T ParseXml<T>(string xml)
            where T : class
        {
            using (TextReader reader = new StringReader(xml))
            {
                var theXmlRootAttribute = Attribute.GetCustomAttributes(typeof(T))
                    .FirstOrDefault(x => x is XmlRootAttribute) as XmlRootAttribute;
                var serializer = new XmlSerializer(typeof(T), theXmlRootAttribute ?? new XmlRootAttribute(nameof(T)));

                return (T)serializer.Deserialize(reader);
            }
        }

        /// <inheritdoc />
        public string ToXml<T>(T @class)
            where T : class
        {
            if (@class == null)
            {
                return string.Empty;
            }

            using (var writer = new StringWriter())
            {
                var theXmlRootAttribute = Attribute.GetCustomAttributes(typeof(T))
                    .FirstOrDefault(x => x is XmlRootAttribute) as XmlRootAttribute;
                var serializer = new XmlSerializer(typeof(T), theXmlRootAttribute ?? new XmlRootAttribute(nameof(T)));

                serializer.Serialize(writer, @class);
                return writer.ToString();
            }
        }

        /// <inheritdoc />
        public bool HasPackage(string packageId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var pe = _packageRepository.GetPackageByPackageId(context, packageId);
                if (pe != null)
                {
                    return pe.State != PackageState.Deleted && pe.State != PackageState.Error;
                }

                return IsTransferring(packageId);
            }
        }

        /// <inheritdoc />
        public bool HasModule(string moduleId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var me = _moduleRepository.GetModuleByModuleId(context, moduleId);

                return me != null;
            }
        }

        /// <inheritdoc />
        public bool HasScript(int scriptId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return _scriptRepository.GetScriptByScriptId(context, scriptId) != null;
            }
        }

        /// <inheritdoc />
        public Script GetScript(int scriptId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return _scriptRepository.GetScriptByScriptId(context, scriptId);
            }
        }

        /// <inheritdoc />
        public Module GetModuleEntity(string moduleId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return _moduleRepository.GetModuleByModuleId(context, moduleId);
            }
        }

        /// <inheritdoc />
        public bool IsTransferring(string packageId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var te = _transferRepository.GetByPackageId(context, packageId);
                if (te != null)
                {
                    return te.State == TransferState.Pending || te.State == TransferState.InProgress;
                }

                return false;
            }
        }

        /// <inheritdoc />
        public TransferEntity GetTransferEntity(string packageId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var te = _transferRepository.GetByPackageId(context, packageId);

                return te;
            }
        }

        /// <inheritdoc />
        public TransferEntity GetTransferEntity(long transferId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var te = _transferRepository.Get(context, transferId);

                return te;
            }
        }

        /// <inheritdoc />
        public void RetryTransfer(
            string packageId,
            Action<PackageTransferEventArgs> changeStatusCallback,
            PackageLog packageLog,
            CancellationToken ct)
        {
            var handler = new RetryTransferHandler(
                _contextFactory,
                _packageRepository,
                _transferRepository,
                _transferService,
                _packageErrorRepository,
                _componentHash,
                _installerService,
                _pathMapper,
                _fileSystemProvider,
                UpdatePackageLog);

            handler.Execute(new BaseTransferPackageArgs(packageId, changeStatusCallback, ct, packageLog));
        }

        /// <inheritdoc />
        public void DownloadPackage(
            string packageId,
            TransferEntity transfer,
            Action<PackageTransferEventArgs> changeStatusCallback,
            CancellationToken ct,
            PackageLog packageLog,
            int deviceId)
        {
            var handler = new DownloadPackageHandler(
                _contextFactory,
                _transferRepository,
                _transferService,
                _packageErrorRepository,
                _componentHash,
                _pathMapper,
                _fileSystemProvider,
                UpdatePackageLog);

            handler.Execute(new TransferPackageArgs(packageId, transfer, changeStatusCallback, ct, packageLog, deviceId));
        }

        /// <inheritdoc />
        public Image ReadManifest(string packageId)
        {
            return _installerService.ReadManifest(packageId);
        }

        /// <inheritdoc />
        public long GetScriptLastSequence()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return ScriptCount != 0 ? _scriptRepository.GetMaxLastSequence(context) : 0;
            }
        }

        private void AddPackageLog(PackageLog packageLog, DbContext context)
        {
            _packageLogs.Add(context, packageLog);

            UpdatePackage(packageLog, context);

            Logger.Info("Added PackageLog=" + packageLog);
        }

        private void UpdateTransferEntity(TransferEntity entity, DbContext context)
        {
            var log = _transferRepository.GetByPackageId(context, entity.PackageId);

            if (log != null)
            {
                log.State = entity.State;
                log.DeleteAfter = entity.DeleteAfter;
                log.Exception = entity.Exception;
                log.Location = entity.Location;
                log.PackageId = entity.PackageId;
                log.PackageValidateDateTime = entity.PackageValidateDateTime;
                log.Parameters = entity.Parameters;
                log.ReasonCode = entity.ReasonCode;
                log.Size = entity.Size;
                log.TransferCompletedDateTime = entity.TransferCompletedDateTime;
                log.TransferId = entity.TransferId;
                log.TransferPaused = entity.TransferPaused;
                log.TransferSize = entity.TransferSize;
                log.TransferType = entity.TransferType;

                _transferRepository.Update(context, log);
            }
            else
            {
                _transferRepository.Add(context, entity);
                log = entity;
            }

            UpdatePackageLog(log, context);

            Logger.Info("Updated TransferEntity=" + entity);
        }

        private void UpdatePackageLog(TransferEntity entity, DbContext context)
        {
            var log = _packageLogs.GetLastPackageLogeByPackageId(context, entity.PackageId);
            if (log != null)
            {
                log.TransferState = entity.State;
                log.DeleteAfter = entity.DeleteAfter;
                log.Exception = entity.Exception;
                log.Location = entity.Location;
                log.PackageId = entity.PackageId;
                log.PackageValidateDateTime = entity.PackageValidateDateTime;
                log.Parameters = entity.Parameters;
                log.ReasonCode = entity.ReasonCode;
                log.Size = entity.Size;
                log.TransferCompletedDateTime = entity.TransferCompletedDateTime;
                log.TransferId = entity.TransferId;
                log.TransferPaused = entity.TransferPaused;
                log.TransferSize = entity.TransferSize;
                log.TransferType = entity.TransferType;
                log.ErrorCode = entity.Exception;

                _packageLogs.Update(context, log);
            }
        }

        private void UpdatePackageLog(PackageLog packageLog, DbContext context)
        {
            var log = _packageLogs.GetLastPackageLogeByPackageId(context, packageLog.PackageId);

            if (log != null)
            {
                log.Exception = packageLog.Exception;
                log.ReasonCode = packageLog.ReasonCode;
                log.Size = packageLog.Size;
                log.State = packageLog.State;
                log.ActivityDateTime = packageLog.ActivityDateTime;
                log.ActivityType = packageLog.ActivityType;
                log.Overwrite = packageLog.Overwrite;
                log.DeviceId = packageLog.DeviceId;
                log.TransactionId = packageLog.TransactionId;
                log.ErrorCode = packageLog.ErrorCode;
                log.Hash = packageLog.Hash;
                log.TransferId = packageLog.TransferId;
                log.Location = packageLog.Location;
                log.Parameters = packageLog.Parameters;
                log.TransferState = packageLog.TransferState;
                log.TransferType = packageLog.TransferType;
                log.DeleteAfter = packageLog.DeleteAfter;
                log.TransferSize = packageLog.TransferSize;
                log.TransferPaused = packageLog.TransferPaused;
                log.TransferCompletedDateTime = packageLog.TransferCompletedDateTime;
                log.PackageValidateDateTime = packageLog.PackageValidateDateTime;

                _packageLogs.Update(context, log);
            }

            UpdatePackage(log, context);

            Logger.Info("Updated PackageLog=" + packageLog);
        }

        private void UpdatePackage(PackageLog packageLog, DbContext context)
        {
            var pe = _packageRepository.GetPackageByPackageId(context, packageLog.PackageId);

            if (pe == null)
            {
                var package = new Package() { PackageId = packageLog.PackageId, Size = packageLog.Size, Hash = packageLog.Hash };

                _packageRepository.Add(context, package);
            }
            else
            {
                pe.Size = packageLog.Size;
                pe.State = packageLog.State;
                pe.Hash = packageLog.Hash;

                _packageRepository.Update(context, pe);
            }
        }
    }
}
