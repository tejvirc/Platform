namespace Aristocrat.Monaco.Bingo.Services.GamePlay
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts.Localization;
    using Common;
    using Common.Storage.Model;
    using Gaming.Contracts.Central;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Protocol.Common.Storage.Entity;

    public class TotalWinValidator : ITotalWinValidator
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ISystemDisableManager _systemDisableManager;

        public TotalWinValidator(
            IUnitOfWorkFactory unitOfWorkFactory,
            ISystemDisableManager systemDisableManager)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _systemDisableManager = systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));

            SetupWinResults();
        }

        private bool WinMismatchError
        {
            get
            {
                using var unitOfWork = _unitOfWorkFactory.Create();
                var entity = unitOfWork.Repository<WinResultModel>().Queryable().FirstOrDefault();

                return entity?.IsTotalWinMismatched ?? false;
            }
            set
            {
                using var unitOfWork = _unitOfWorkFactory.Create();
                var repo = unitOfWork.Repository<WinResultModel>();
                var entity = repo.Queryable().AsNoTracking().First();

                entity.IsTotalWinMismatched = value;
                repo.Update(entity);

                unitOfWork.SaveChanges();
            }
        }

        public void ValidateTotalWin(long totalWinInMillicents, CentralTransaction transaction)
        {
            var currentGameTotalWin = transaction.Outcomes.Select(x => x.Value).Sum();
            if (totalWinInMillicents == currentGameTotalWin)
            {
                return;
            }

            Logger.Debug($"ValidateTotalWin failed, totalWin={totalWinInMillicents}, currentGameTotalWin={currentGameTotalWin}");

            WinMismatchError = true;
            WinResultMismatchDisable();
        }

        private void WinResultMismatchDisable()
        {
            _systemDisableManager.Disable(
                BingoConstants.BingoWinMismatchKey,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PresentationNotFound));
        }

        private void SetupWinResults()
        {
            if (WinMismatchError)
            {
                WinResultMismatchDisable();
            }

            using var unitOfWork = _unitOfWorkFactory.Create();
            var list = unitOfWork.Repository<WinResultModel>().Queryable();

            if (list.Any())
            {
                return;
            }

            unitOfWork.Repository<WinResultModel>().Add(
                new WinResultModel
                {
                    IsTotalWinMismatched = false
                });

            unitOfWork.SaveChanges();
        }
    }
}
