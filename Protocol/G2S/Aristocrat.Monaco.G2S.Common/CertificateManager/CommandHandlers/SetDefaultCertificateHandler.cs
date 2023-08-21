namespace Aristocrat.Monaco.G2S.Common.CertificateManager.CommandHandlers
{
    using System;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using Models;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Storage;
    using Storage;

    /// <summary>
    ///     Sets the specified certificate as the default certificate in the certificate store
    /// </summary>
    public class SetDefaultCertificateHandler : IParametersFuncHandler<X509Certificate2, Certificate>
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly IMonacoContextFactory _contextFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetDefaultCertificateHandler" /> class.
        /// </summary>
        /// <param name="contextFactory">The context factory.</param>
        /// <param name="certificateRepository">An <see cref="ICertificateRepository" /> instance.</param>
        public SetDefaultCertificateHandler(
            IMonacoContextFactory contextFactory,
            ICertificateRepository certificateRepository)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _certificateRepository =
                certificateRepository ?? throw new ArgumentNullException(nameof(certificateRepository));
        }

        /// <inheritdoc />
        public Certificate Execute(X509Certificate2 parameter)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var currentDefault = _certificateRepository.Get(context, c => c.Default).SingleOrDefault();

                if (currentDefault?.Thumbprint == parameter.Thumbprint)
                {
                    return currentDefault;
                }

                if (currentDefault != null)
                {
                    currentDefault.Default = false;

                    _certificateRepository.Update(context, currentDefault);
                }

                var certificate = _certificateRepository.Get(context, c => c.Thumbprint == parameter.Thumbprint)
                    .Single();

                certificate.Default = true;

                _certificateRepository.Update(context, certificate);

                return certificate;
            }
        }
    }
}