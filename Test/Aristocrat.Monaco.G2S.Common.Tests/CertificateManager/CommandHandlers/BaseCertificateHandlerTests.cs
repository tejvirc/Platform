namespace Aristocrat.Monaco.G2S.Common.Tests.CertificateManager.CommandHandlers
{
    using Common.CertificateManager.Models;
    using Common.CertificateManager.Storage;
    using Common.CertificateManager.Validators;
    using FluentValidation;
    using Monaco.Common.Storage;
    using Moq;

    public abstract class BaseCertificateHandlerTests
    {
        protected readonly IValidator<Certificate> CertificateEntityValidator = new CertificateValidator();

        protected readonly Mock<ICertificateRepository> CertificateRepositoryMock = new Mock<ICertificateRepository>();

        protected readonly Mock<IMonacoContextFactory> ContextFactoryMock = new Mock<IMonacoContextFactory>();
    }
}