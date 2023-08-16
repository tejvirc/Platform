namespace Aristocrat.Monaco.G2S.Common.PackageManager.CommandHandlers
{
    using System;
    using System.Linq;
    using Application.Contracts.Localization;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Exceptions;
    using Monaco.Common.Storage;
    using G2S.Data.Model;
    using Localization.Properties;
    using Storage;

    /// <summary>
    ///     Get package status command handler implementation.
    /// </summary>
    public class GetPackageStatusHandler : IParametersFuncHandler<string, GetPackageStatusResult>
    {
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IPackageErrorRepository _packageErrorRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly ITransferRepository _transferRepository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetPackageStatusHandler" /> class.
        /// </summary>
        /// <param name="packageRepository">Package repository instance.</param>
        /// <param name="transferRepository">Transfer service instance.</param>
        /// <param name="packageErrorRepository">Package errors repository.</param>
        /// <param name="contextFactory">Context factory.</param>
        public GetPackageStatusHandler(
            IPackageRepository packageRepository,
            ITransferRepository transferRepository,
            IPackageErrorRepository packageErrorRepository,
            IMonacoContextFactory contextFactory)
        {
            _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
            _transferRepository = transferRepository ?? throw new ArgumentNullException(nameof(transferRepository));
            _packageErrorRepository =
                packageErrorRepository ?? throw new ArgumentNullException(nameof(packageErrorRepository));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <inheritdoc />
        public GetPackageStatusResult Execute(string parameter)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                if (string.IsNullOrEmpty(parameter))
                {
                    throw new ArgumentNullException(nameof(parameter));
                }

                var result = new GetPackageStatusResult();

                var found = _packageRepository.GetPackageByPackageId(context, parameter);
                if (found == null)
                {
                    throw new CommandException(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PackageNotFoundErrorMessage));
                }

                if (found.State == PackageState.Error)
                {
                    result.PackageError = _packageErrorRepository.GetByPackageId(context, parameter).FirstOrDefault();
                }

                result.Transfer = _transferRepository.GetByPackageId(context, parameter);
                if (result.Transfer != null && result.Transfer.State != TransferState.InProgress)
                {
                    result.Transfer = null;
                }

                result.PackageState = found.State;

                return result;
            }
        }
    }
}