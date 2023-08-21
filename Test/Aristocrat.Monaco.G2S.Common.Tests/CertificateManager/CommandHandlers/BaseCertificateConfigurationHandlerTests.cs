namespace Aristocrat.Monaco.G2S.Common.Tests.CertificateManager.CommandHandlers
{
    using Common.CertificateManager.Storage;
    using Monaco.Common.Storage;
    using Moq;

    public abstract class BaseCertificateConfigurationHandlerTests
    {
        protected readonly Mock<IMonacoContextFactory> ContextFactoryMock =
            new Mock<IMonacoContextFactory>();

        protected readonly Mock<IPkiConfigurationRepository> PkiConfigurationRepositoryMock =
            new Mock<IPkiConfigurationRepository>();
    }
}