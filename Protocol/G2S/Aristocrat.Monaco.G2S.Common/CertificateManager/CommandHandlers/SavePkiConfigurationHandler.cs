namespace Aristocrat.Monaco.G2S.Common.CertificateManager.CommandHandlers
{
    using System;
    using ExpressMapper;
    using FluentValidation;
    using Models;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Models;
    using Monaco.Common.Storage;
    using Monaco.Common.Validation;
    using Storage;

    /// <summary>
    ///     Save certificate configuration handler
    /// </summary>
    public class SavePkiConfigurationHandler : IParametersFuncHandler<PkiConfiguration, SaveEntityResult>
    {
        private readonly IValidator<PkiConfiguration> _configurationValidator;

        private readonly IMonacoContextFactory _contextFactory;
        private readonly IPkiConfigurationRepository _pkiConfigurationRepository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SavePkiConfigurationHandler" /> class.
        /// </summary>
        /// <param name="contextFactory">The context factory.</param>
        /// <param name="pkiConfigurationRepository">The certificate configuration repository.</param>
        /// <param name="pkiConfigurationValidator">The certificate configuration validator.</param>
        public SavePkiConfigurationHandler(
            IMonacoContextFactory contextFactory,
            IPkiConfigurationRepository pkiConfigurationRepository,
            IValidator<PkiConfiguration> pkiConfigurationValidator)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _pkiConfigurationRepository = pkiConfigurationRepository ??
                                          throw new ArgumentNullException(nameof(pkiConfigurationRepository));
            _configurationValidator =
                pkiConfigurationValidator ?? throw new ArgumentNullException(nameof(pkiConfigurationValidator));
        }

        /// <summary>
        ///     Executes the specified certificate configuration entity.
        /// </summary>
        /// <param name="parameter">The certificate configuration entity.</param>
        /// <returns>The result</returns>
        public SaveEntityResult Execute(PkiConfiguration parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var validationResult = _configurationValidator.Validate(parameter);

            using (var context = _contextFactory.CreateDbContext())
            {
                if (validationResult.IsValid)
                {
                    var currentCertificateConfiguration = _pkiConfigurationRepository.GetSingle(context);

                    if (currentCertificateConfiguration == null)
                    {
                        currentCertificateConfiguration = new PkiConfiguration(0);
                        currentCertificateConfiguration = Mapper.Map(
                            parameter,
                            currentCertificateConfiguration);
                        _pkiConfigurationRepository.Add(context, currentCertificateConfiguration);
                    }
                    else
                    {
                        currentCertificateConfiguration = Mapper.Map(
                            parameter,
                            currentCertificateConfiguration);
                        _pkiConfigurationRepository.Update(context, currentCertificateConfiguration);
                    }

                    return new SaveEntityResult(true);
                }
            }

            return new SaveEntityResult(false, validationResult.ConvertToCommonValidationResult());
        }
    }
}