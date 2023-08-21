namespace Aristocrat.Monaco.Hhr.Storage.Helpers
{
    using System;
    using System.Data;
    using System.Linq;
    using Models;
    using Protocol.Common.Storage.Entity;

    public class ManualHandicapEntityHelper : IManualHandicapEntityHelper
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private bool _isComplete;

        public ManualHandicapEntityHelper(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var entity = unitOfWork.Repository<ManualHandicapEntity>().Queryable().FirstOrDefault();

                if (entity is null)
                {
                    entity = new ManualHandicapEntity { IsCompleted = false };
                    unitOfWork.Repository<ManualHandicapEntity>().Add(entity);
                    unitOfWork.SaveChanges();
                }

                _isComplete = entity.IsCompleted;
            }
        }

        public bool IsCompleted
        {
            get => _isComplete;
            set
            {
                _isComplete = value;
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    unitOfWork.BeginTransaction(IsolationLevel.Serializable);
                    var repo = unitOfWork.Repository<ManualHandicapEntity>();
                    var entity = repo.Queryable().First();
                    entity.IsCompleted = value;
                    repo.AddOrUpdate(entity);
                    unitOfWork.Commit();
                }
            }
        }

    }
}
