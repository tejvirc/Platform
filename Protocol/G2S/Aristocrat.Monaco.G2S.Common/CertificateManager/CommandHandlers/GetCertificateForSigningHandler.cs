namespace Aristocrat.Monaco.G2S.Common.CertificateManager.CommandHandlers
{
    using System;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Storage;
    using Storage;

    /// <summary>
    ///     Get certificate for signing handler
    /// </summary>
    public class GetCertificateForSigningHandler : IFuncHandler<X509Certificate2>
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly IMonacoContextFactory _contextFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetCertificateForSigningHandler" /> class.
        /// </summary>
        /// <param name="contextFactory">The context factory.</param>
        /// <param name="certificateRepository">The certificate repository.</param>
        public GetCertificateForSigningHandler(
            IMonacoContextFactory contextFactory,
            ICertificateRepository certificateRepository)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _certificateRepository =
                certificateRepository ?? throw new ArgumentNullException(nameof(certificateRepository));
        }

        /// <inheritdoc />
        public X509Certificate2 Execute()
        {
            using (var context = _contextFactory.Create())
            {
                var certificate = _certificateRepository.Get(context, c => c.Default).SingleOrDefault();

                return certificate?.ToX509Certificate2();
            }
        }
    }
}