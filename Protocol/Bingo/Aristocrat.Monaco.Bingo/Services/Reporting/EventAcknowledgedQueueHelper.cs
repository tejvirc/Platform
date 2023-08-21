namespace Aristocrat.Monaco.Bingo.Services.Reporting
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Application.Contracts.Localization;
    using Aristocrat.Bingo.Client.Messages;
    using Common;
    using Common.Storage.Model;
    using Hardware.Contracts;
    using Kernel;
    using Localization.Properties;
    using Protocol.Common.Storage.Entity;

    public class EventAcknowledgedQueueHelper : IAcknowledgedQueueHelper<ReportEventMessage, long>
    {
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public EventAcknowledgedQueueHelper(
            IUnitOfWorkFactory unitOfWorkFactory,
            ISystemDisableManager systemDisableManager)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _systemDisableManager =
                systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
        }

        public long GetId(ReportEventMessage item)
        {
            return item.EventId;
        }

        public void WritePersistence(List<ReportEventMessage> list)
        {
            using var work = _unitOfWorkFactory.Create();
            work.BeginTransaction(IsolationLevel.Serializable);
            var repository = work.Repository<ReportEventModel>();
            var queue = repository.Queryable().SingleOrDefault() ?? new ReportEventModel();

            queue.Report = StorageUtilities.ToByteArray(list);
            repository.AddOrUpdate(queue);
            work.Commit();
        }

        public List<ReportEventMessage> ReadPersistence()
        {
            var queue = _unitOfWorkFactory.Invoke(
                x => x.Repository<ReportEventModel>().Queryable().FirstOrDefault());
            return queue is null ?
                new List<ReportEventMessage>() :
                StorageUtilities.GetListFromByteArray<ReportEventMessage>(queue.Report).ToList();
        }

        public void AlmostFullDisable()
        {
            _systemDisableManager.Disable(
                BingoConstants.EventQueueDisableKey,
                SystemDisablePriority.Normal,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReportEventQueueAlmostFull));
        }

        public void AlmostFullClear()
        {
            if (_systemDisableManager.IsDisabled)
            {
                _systemDisableManager.Enable(BingoConstants.EventQueueDisableKey);
            }
        }
    }
}