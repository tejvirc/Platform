namespace Aristocrat.Monaco.Bingo
{
    using System;
    using System.Linq;
    using Aristocrat.Bingo.Client.Configuration;
    using Services.Security;
    using Common.Storage.Model;
    using Protocol.Common.Storage.Entity;

    public class BingoClientConfigurationProvider : IClientConfigurationProvider
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ICertificateService _certificateService;

        public BingoClientConfigurationProvider(
            IUnitOfWorkFactory unitOfWorkFactory,
            ICertificateService certificateService)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _certificateService = certificateService ?? throw new ArgumentNullException(nameof(certificateService));
        }

        public ClientConfigurationOptions CreateConfiguration() =>
            _unitOfWorkFactory.Invoke(x => x.Repository<Host>().Queryable().Single())
                .ToConfigurationOptions(_certificateService.GetCertificates());
    }
}