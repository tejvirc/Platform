namespace Aristocrat.Monaco.G2S.Common.CertificateManager.CommandHandlers
{
    using System;
    using Models;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Storage;
    using Storage;

    /// <summary>
    ///     Get certificate configuration handler
    /// </summary>
    public class GetPkiConfigurationHandler : IFuncHandler<PkiConfiguration>
    {
        private readonly IPkiConfigurationRepository _certificateConfigurationRepository;
        private readonly IMonacoContextFactory _contextFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetPkiConfigurationHandler" /> class.
        /// </summary>
        /// <param name="contextFactory">The context factory.</param>
        /// <param name="certificateConfigurationRepository">The certificate configuration repository.</param>
        public GetPkiConfigurationHandler(
            IMonacoContextFactory contextFactory,
            IPkiConfigurationRepository certificateConfigurationRepository)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _certificateConfigurationRepository = certificateConfigurationRepository ??
                                                  throw new ArgumentNullException(
                                                      nameof(certificateConfigurationRepository));
        }

        /// <summary>
        ///     Executes this instance.
        /// </summary>
        /// <returns>Returns the configuration entity.</returns>
        public PkiConfiguration Execute()
        {
            using (var context = _contextFactory.Create())
            {
                return _certificateConfigurationRepository.GetSingle(context);
            }
        }
    }
}