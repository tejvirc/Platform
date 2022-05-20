namespace Aristocrat.Monaco.G2S.Common.CertificateManager.CommandHandlers
{
    using System;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Storage;
    using Storage;

    /// <summary>
    ///     Remove current certificate handler
    /// </summary>
    public class DeleteCertificateHandler : IParametersFuncHandler<X509Certificate2, bool>
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly IMonacoContextFactory _contextFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DeleteCertificateHandler" /> class.
        /// </summary>
        /// <param name="contextFactory">The context factory.</param>
        /// <param name="certificateRepository">The certificate repository.</param>
        public DeleteCertificateHandler(
            IMonacoContextFactory contextFactory,
            ICertificateRepository certificateRepository)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _certificateRepository =
                certificateRepository ?? throw new ArgumentNullException(nameof(certificateRepository));
        }

        /// <inheritdoc />
        public bool Execute(X509Certificate2 parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            using (var context = _contextFactory.Create())
            {
                var certificate =
                    _certificateRepository.Get(context, c => c.Thumbprint == parameter.Thumbprint).SingleOrDefault();

                if (certificate == null)
                {
                    return false;
                }

                _certificateRepository.Delete(context, certificate);
            }

            return true;
        }
    }
}