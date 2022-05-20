namespace Aristocrat.Monaco.Hhr.Storage.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Models;
    using Protocol.Common.Storage.Entity;

    public class ProgressiveUpdateEntityHelper  : IProgressiveUpdateEntityHelper
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private List<ProgressiveUpdateEntity> _progressiveUpdates;

        public ProgressiveUpdateEntityHelper(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                _progressiveUpdates = unitOfWork.Repository<ProgressiveUpdateEntity>().Queryable().ToList();
            }
        }

        public IEnumerable<ProgressiveUpdateEntity> ProgressiveUpdates
        {
            get => _progressiveUpdates;
            set
            {
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    unitOfWork.BeginTransaction(IsolationLevel.Serializable);
                    _progressiveUpdates = value.ToList();
                    var repo = unitOfWork.Repository<ProgressiveUpdateEntity>();
                    repo.ClearTable();
                    repo.AddRange(_progressiveUpdates);
                    unitOfWork.Commit();
                }
            }
        }
    }
}