namespace Aristocrat.Monaco.G2S.Common.CertificateManager.CommandHandlers
{
    using System;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using FluentValidation;
    using Models;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Storage;
    using Monaco.Common.Validation;
    using Storage;

    /// <summary>
    ///     Save current certificate handler
    /// </summary>
    public class AddCertificateHandler : IParametersFuncHandler<X509Certificate2, Certificate>
    {
        private readonly ICertificateRepository _certificateRepository;

        private readonly IValidator<Certificate> _certificateValidator;

        private readonly IMonacoContextFactory _contextFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AddCertificateHandler" /> class.
        /// </summary>
        /// <param name="contextFactory">The context factory.</param>
        /// ///
        /// <param name="certificateRepository">The certificate repository.</param>
        /// <param name="certificateValidator">The certificate validator.</param>
        public AddCertificateHandler(
            IMonacoContextFactory contextFactory,
            ICertificateRepository certificateRepository,
            IValidator<Certificate> certificateValidator)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _certificateRepository =
                certificateRepository ?? throw new ArgumentNullException(nameof(certificateRepository));
            _certificateValidator =
                certificateValidator ?? throw new ArgumentNullException(nameof(certificateValidator));
        }

        /// <summary>
        ///     Executes the specified save certificate arguments.
        /// </summary>
        /// <param name="parameter">The certificate.</param>
        /// <returns>A SaveEntityResult object.</returns>
        public Certificate Execute(X509Certificate2 parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            using (var context = _contextFactory.Create())
            {
                var existing = _certificateRepository.Get(context, c => c.Thumbprint == parameter.Thumbprint)
                    .FirstOrDefault();
                if (existing != null)
                {
                    return existing;
                }
            }

            var password = string.Empty;
            byte[] data;

            if (parameter.HasPrivateKey)
            {
                password = CertificateUtilities.GeneratePassword();
                data = parameter.Export(X509ContentType.Pkcs12, password);
            }
            else
            {
                data = parameter.Export(X509ContentType.Cert);
            }

            var certificate = new Certificate
            {
                Thumbprint = parameter.Thumbprint,
                Data = data,
                Password = password,
                VerificationDate = DateTime.UtcNow
            };

            var validationResult = _certificateValidator
                .Validate(certificate, ruleSet: $"{ValidationRuleType.Common},{ValidationRuleType.AddEntity}");

            if (!validationResult.IsValid)
            {
                return null;
            }

            using (var context = _contextFactory.Create())
            {
                _certificateRepository.Add(context, certificate);
            }

            return certificate;
        }
    }
}