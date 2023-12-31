﻿namespace Aristocrat.Monaco.Bingo.Services.Reporting
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

    public class TransactionAcknowledgedQueueHelper : IAcknowledgedQueueHelper<ReportTransactionMessage, long>
    {
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public TransactionAcknowledgedQueueHelper(
            IUnitOfWorkFactory unitOfWorkFactory,
            ISystemDisableManager systemDisableManager)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _systemDisableManager =
                systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
        }

        public long GetId(ReportTransactionMessage item)
        {
            return item.TransactionId;
        }

        public void WritePersistence(List<ReportTransactionMessage> list)
        {
            using var work = _unitOfWorkFactory.Create();
            work.BeginTransaction(IsolationLevel.Serializable);
            var repository = work.Repository<ReportTransactionModel>();
            var queue = repository.Queryable().SingleOrDefault() ?? new ReportTransactionModel();

            queue.Report = StorageUtilities.ToByteArray(list);
            repository.AddOrUpdate(queue);
            work.Commit();
        }

        public List<ReportTransactionMessage> ReadPersistence()
        {
            var queue = _unitOfWorkFactory.Invoke(
                x => x.Repository<ReportTransactionModel>().Queryable().FirstOrDefault());
            if (queue is null)
            {
                return new List<ReportTransactionMessage>();
            }

            return StorageUtilities.GetListFromByteArray<ReportTransactionMessage>(queue.Report).ToList();
        }

        public void AlmostFullDisable()
        {
            _systemDisableManager.Disable(
                BingoConstants.TransactionQueueDisableKey,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TransactionReportingQueueAlmostFull),
                true,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TransactionReportingQueueAlmostFullHelp));
        }

        public void AlmostFullClear()
        {
            if (_systemDisableManager.IsDisabled)
            {
                _systemDisableManager.Enable(BingoConstants.TransactionQueueDisableKey);
            }
        }
    }
}