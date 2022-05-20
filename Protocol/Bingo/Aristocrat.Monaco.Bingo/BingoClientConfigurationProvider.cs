namespace Aristocrat.Monaco.Bingo
{
    using System;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using Aristocrat.Bingo.Client.Configuration;
    using Services.Security;
    using Common.Storage.Model;
    using Protocol.Common.Storage.Entity;

    public class BingoClientConfigurationProvider : IClientConfigurationProvider
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ICertificateService _certificateService;

        public BingoClientConfigurationProvider(IUnitOfWorkFactory unitOfWorkFactory,
            ICertificateService certificateService)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _certificateService = certificateService ?? throw new ArgumentNullException(nameof(certificateService));
        }

        public ClientConfigurationOptions Configuration =>
            _unitOfWorkFactory.Invoke(x => x.Repository<Host>().Queryable().Single())
            .ToConfigurationOptions(_certificateService.Get(
                StoreName.AuthRoot,
                StoreLocation.LocalMachine,
                X509FindType.FindByThumbprint));
    }
}