namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Application.Contracts.Authentication;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common;
    using Common.Storage;
    using Contracts.Client;
    using Kernel.Contracts.Components;
    using log4net;

    /// <summary>
    ///     Handles calculating the ROM signature
    /// </summary>
    public class RomSignatureVerificationHandler : ISasLongPollHandler<LongPollResponse, RomSignatureData>
    {
        private const string ManifestExtension = "manifest";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ISet<ComponentType> ManifestComponents = new HashSet<ComponentType>
        {
            ComponentType.Module,
            ComponentType.Package
        };

        private static readonly ISet<ComponentType> HashingComponents = new HashSet<ComponentType>
        {
            ComponentType.OS
        };

        private readonly ISasHost _sasHost;
        private readonly IComponentRegistry _componentRegistry;
        private readonly IFileSystemProvider _fileSystemProvider;
        private readonly IAuthenticationService _authenticationService;

        /// <summary>
        ///     Creates a new instance of the RomSignatureVerificationHandler
        /// </summary>
        /// <param name="sasHost">An instance of <see cref="ISasHost"/></param>
        /// <param name="componentRegistry">An instance of <see cref="IComponentRegistry"/></param>
        /// <param name="fileSystemProvider">An instance of <see cref="IFileSystemProvider"/></param>
        /// <param name="authenticationService">An instance of <see cref="IAuthenticationService"/></param>
        public RomSignatureVerificationHandler(
            ISasHost sasHost,
            IComponentRegistry componentRegistry,
            IFileSystemProvider fileSystemProvider,
            IAuthenticationService authenticationService)
        {
            _sasHost = sasHost ?? throw new ArgumentNullException(nameof(sasHost));
            _componentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
            _fileSystemProvider = fileSystemProvider ?? throw new ArgumentNullException(nameof(fileSystemProvider));
            _authenticationService =
                authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.RomSignatureVerification
        };

        /// <inheritdoc />
        public LongPollResponse Handle(RomSignatureData data)
        {
            CalculateRomHash(data.Seed, data.ClientNumber).FireAndForget();

            return new LongPollResponse();
        }

        private Task CalculateRomHash(ushort seed, byte clientId)
        {
            return Task.Run(
                () =>
                {
                    var computedHash = HashComponents(BitConverter.GetBytes(seed));
                    computedHash = HashManifestFiles(computedHash);

                    _sasHost.RomSignatureCalculated(BitConverter.ToUInt16(computedHash, 0), clientId);
                });
        }

        private byte[] HashManifestFiles(byte[] computedHash)
        {
            // StringComparer.Ordinal is required to match sorting done by Python script.
            foreach (var component in _componentRegistry.Components
                .Where(x => ManifestComponents.Contains(x.Type) && !string.IsNullOrEmpty(x.Path)).OrderBy(x => x.ComponentId, StringComparer.Ordinal))
            {
                var manifest = _fileSystemProvider.SearchFiles(
                    Path.GetDirectoryName(component.Path),
                    $"{Path.GetFileNameWithoutExtension(component.Path)}.{ManifestExtension}").FirstOrDefault();
                if (manifest is null)
                {
                    Logger.Warn($"Missing a manifest file for {component.ComponentId} of type {component.Type}");
                    continue;
                }

                Logger.Debug($"Hashing the manifest for component {component.ComponentId} of type {component.Type}");

                using (var fs = _fileSystemProvider.GetFileReadStream(manifest))
                {
                    computedHash = _authenticationService.ComputeHash(
                        fs,
                        AlgorithmType.Crc16.ToString().ToUpperInvariant(),
                        null,
                        computedHash);
                }
            }

            return computedHash;
        }

        private byte[] HashComponents(byte[] computedHash)
        {
            foreach (var component in _componentRegistry.Components
                .Where(x => HashingComponents.Contains(x.Type)).OrderBy(x => x.ComponentId))
            {
                Logger.Debug($"Hashing component {component.ComponentId} of type {component.Type}");

                var componentVerification = new ComponentVerification
                {
                    AlgorithmType = AlgorithmType.Crc16,
                    ComponentId = component.ComponentId,
                    EndOffset = (long)AuthenticationServiceConstants.EndOfStream,
                    StartOffset = 0,
                    Seed = computedHash
                };

                _authenticationService.CalculateHash(component, componentVerification);
                computedHash = componentVerification.Result;
            }

            return computedHash;
        }
    }
}