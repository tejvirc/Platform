namespace Aristocrat.Monaco.Bingo.Services.GamePlay
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using Application.Contracts.Localization;
    using Common;
    using Common.Storage.Model;
    using Gaming.Contracts.Central;
    using Kernel;
    using Kernel.Contracts.MessageDisplay;
    using Localization.Properties;
    using Protocol.Common.Storage.Entity;

    public class TotalWinValidator : ITotalWinValidator
    {
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

            WinMismatchError = true;
            WinResultMismatchDisable();
        }

        private void WinResultMismatchDisable()
        {
            _systemDisableManager.Disable(
                BingoConstants.BingoWinMismatchKey,
                SystemDisablePriority.Immediate,
                () => Localizer.GetString(ResourceKeys.PresentationNotFound, CultureProviderType.Player));
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
