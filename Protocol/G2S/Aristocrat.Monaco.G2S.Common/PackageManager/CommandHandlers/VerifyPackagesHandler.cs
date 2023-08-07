namespace Aristocrat.Monaco.G2S.Common.PackageManager.CommandHandlers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts.Authentication;
    using Kernel;
    using Kernel.Contracts.Components;
    using log4net;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Storage;
    using Storage;
    using Data.Model;

    /// <summary>
    ///     Verifies packages
    /// </summary>
    public class VerifyPackagesHandler : IActionHandler
    {
        private const string TemporaryDirectoryPath = "/Downloads";
        private const string HashAlgorithm = @"SHA1";

        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IAuthenticationService _componentHash;

        private readonly IMonacoContextFactory _contextFactory;
        private readonly IPackageRepository _packageRepository;
        private readonly IPathMapper _pathMapper;
        private readonly Action<List<Component>> _componentResults;

        /// <summary>
        ///     Handler for package verification
        /// </summary>
        /// <param name="packageRepository">Package repository</param>
        /// <param name="componentHash">Component hash</param>
        /// <param name="pathMapper">Path mapper</param>
        /// <param name="contextFactory">Context Factory</param>
        /// <param name="componentResults">Component Results</param>
        public VerifyPackagesHandler(
            IPackageRepository packageRepository,
            IAuthenticationService componentHash,
            IPathMapper pathMapper,
            IMonacoContextFactory contextFactory,
            Action<List<Component>> componentResults)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));
            _componentHash = componentHash ?? throw new ArgumentNullException(nameof(componentHash));
            _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
            _componentResults = componentResults ?? throw new ArgumentException(nameof(componentResults));
        }

        /// <inheritdoc />
        public void Execute()
        {
            var results = new List<Component>();

            var temp = _pathMapper.GetDirectory(TemporaryDirectoryPath);

            using (var context = _contextFactory.CreateDbContext())
            {
                var packages = _packageRepository.GetAll(context).ToList();

                foreach (var file in temp.GetFiles())
                {
                    var package =
                        packages.FirstOrDefault(p => p.PackageId == Path.GetFileNameWithoutExtension(file.Name));
                    if (package?.State == PackageState.Pending)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(package?.Hash))
                    {
                        File.Delete(file.FullName);

                        Logger.Error(
                            $"Discovered and deleted a file that does not have a corresponding package - {file.Name}");
                    }
                    else
                    {
                        bool result;

                        using (var fileStream = new FileStream(file.FullName, FileMode.Open))
                        {
                            result = _componentHash.VerifyHash(
                                ConvertExtensions.FromPackedHexString(package.Hash),
                                fileStream,
                                HashAlgorithm);
                        }

                        if (!result)
                        {
                            File.Delete(file.FullName);

                            package.State = PackageState.Deleted;
                            _packageRepository.Update(context, package);

                            Logger.Error($"Discovered and deleted a file that failed verification - {file.Name}");
                        }
                        else
                        {
                            results.Add(new Component
                            {
                                ComponentId = package.PackageId,
                                Description = $"{package.PackageId} Download",
                                FileSystemType = FileSystemType.File,
                                Path = file.FullName,
                                Size = package.Size,
                                Type = ComponentType.Package
                            });
                        }
                    }
                }
            }

            Logger.Info("Verified packages");
            _componentResults.Invoke(results);
        }
    }
}
