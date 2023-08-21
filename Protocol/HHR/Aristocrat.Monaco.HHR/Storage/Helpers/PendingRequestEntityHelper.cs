namespace Aristocrat.Monaco.Hhr.Storage.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Client.Messages;
    using Models;
    using Newtonsoft.Json;
    using Protocol.Common.Storage.Entity;

    /// <summary>
    ///     Entity helper for PendingRequestEntity
    /// </summary>
    public class PendingRequestEntityHelper : IPendingRequestEntityHelper
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private List<KeyValuePair<Request, Type>> _pendingRequests;

        public PendingRequestEntityHelper(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _jsonSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var repository = unitOfWork.Repository<PendingRequestEntity>();
                var entity = repository.Queryable().FirstOrDefault();
                if (entity is null)
                {
                    entity = new PendingRequestEntity
                    {
                        PendingRequests = JsonConvert.SerializeObject(
                            new List<KeyValuePair<Request, Type>>(),
                            _jsonSerializerSettings)
                    };

                    repository.Add(entity);
                    unitOfWork.SaveChanges();
                }

                _pendingRequests = JsonConvert.DeserializeObject<List<KeyValuePair<Request, Type>>>(
                    entity.PendingRequests,
                    _jsonSerializerSettings);
            }
        }

        public IEnumerable<KeyValuePair<Request, Type>> PendingRequests
        {
            get => _pendingRequests;
            set
            {
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    unitOfWork.BeginTransaction(IsolationLevel.Serializable);
                    _pendingRequests = value.ToList();
                    var repo = unitOfWork.Repository<PendingRequestEntity>();
                    var entity = repo.Queryable().First();

                    entity.PendingRequests = JsonConvert.SerializeObject(
                        _pendingRequests,
                        _jsonSerializerSettings);
                    repo.AddOrUpdate(entity);
                    unitOfWork.Commit();
                }
            }
        }
    }
}