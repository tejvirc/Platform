namespace Aristocrat.Monaco.Hhr.Storage.Helpers
{
    using System;
    using System.Data;
    using System.Linq;
    using Models;
    using Newtonsoft.Json;
    using Protocol.Common.Storage.Entity;
    using Services;

    public class PrizeInformationEntityHelper : IPrizeInformationEntityHelper
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private PrizeInformation _prizeInformation;

        public PrizeInformationEntityHelper(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var repository = unitOfWork.Repository<PrizeInformationEntity>();
                var entity = repository.Queryable().FirstOrDefault();

                if (entity is null)
                {
                    entity = new PrizeInformationEntity { PrizeInformationJson = string.Empty };
                    repository.Add(entity);
                    unitOfWork.SaveChanges();
                }

                _prizeInformation = JsonConvert.DeserializeObject<PrizeInformation>(entity.PrizeInformationJson);
            }
        }

        public PrizeInformation PrizeInformation
        {
            get => _prizeInformation;
            set
            {
                _prizeInformation = value;
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    unitOfWork.BeginTransaction(IsolationLevel.Serializable);
                    var repository = unitOfWork.Repository<PrizeInformationEntity>();
                    var entity = repository.Queryable().First();
                    entity.PrizeInformationJson = JsonConvert.SerializeObject(value);
                    repository.AddOrUpdate(entity);
                    unitOfWork.Commit();
                }
            }
        }
    }
}
