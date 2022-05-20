namespace Aristocrat.Monaco.Bingo.Tests.Services.GamePlay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Bingo.Services.GamePlay;
    using Common;
    using Common.Storage.Model;
    using Gaming.Contracts.Central;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Protocol.Common.Storage.Entity;
    using Protocol.Common.Storage.Repositories;

    [TestClass]
    public class TotalWinValidatorTests
    {
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new();
        private readonly Mock<ISystemDisableManager> _systemDisableManager = new();

        private TotalWinValidator _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            var models = new List<WinResultModel> { new() { IsTotalWinMismatched = false } };

            var repository = new Mock<IRepository<WinResultModel>>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            _unitOfWorkFactory.Setup(x => x.Create())
                .Returns(mockUnitOfWork.Object);
            mockUnitOfWork.Setup(x => x.Repository<WinResultModel>()).Returns(repository.Object);
            repository.Setup(x => x.Queryable()).Returns(models.AsQueryable());

            _target = CreateTarget();
        }

        [DataRow(true, false)]
        [DataRow(false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorArgumentsTest(bool nullUnitOfWorkFactory, bool nullSystemDisableManager)
        {
            _target = CreateTarget(nullUnitOfWorkFactory, nullSystemDisableManager);
        }

        [TestMethod]
        public void ValidateTotalWinTest()
        {
            var transaction = new CentralTransaction
            {
                Outcomes = new List<Outcome>
                {
                    new(1, 2, 3, OutcomeReference.Direct, OutcomeType.Standard, 1000, 2, string.Empty),
                    new(2, 2, 3, OutcomeReference.Direct, OutcomeType.Standard, 2000, 2, string.Empty)
                }
            };

            var totalValidWin = transaction.Outcomes.Select(x => x.Value).Sum();

            _systemDisableManager.Verify(
                m => m.Disable(
                    BingoConstants.BingoWinMismatchKey,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    null),
                Times.Never);

            _target.ValidateTotalWin(totalValidWin, transaction);

            _unitOfWorkFactory.Verify();
        }

        [TestMethod]
        public void ValidateTotalWinErrorTest()
        {
            const long winDifference = 10;
            var transaction = new CentralTransaction
            {
                Outcomes = new List<Outcome>
                {
                    new(1, 2, 3, OutcomeReference.Direct, OutcomeType.Standard, 1000, 2, string.Empty),
                    new(2, 2, 3, OutcomeReference.Direct, OutcomeType.Standard, 2000, 2, string.Empty)
                }
            };

            var models = new List<WinResultModel> { new() { IsTotalWinMismatched = false } };

            var totalValidWin = transaction.Outcomes.Select(x => x.Value).Sum() - winDifference;

            var repository = new Mock<IRepository<WinResultModel>>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            _unitOfWorkFactory.Setup(x => x.Create())
                .Returns(mockUnitOfWork.Object);
            mockUnitOfWork.Setup(x => x.Repository<WinResultModel>()).Returns(repository.Object);
            repository.Setup(x => x.Queryable()).Returns(models.AsQueryable());

            _systemDisableManager.Setup(
                x => x.Disable(
                    BingoConstants.BingoWinMismatchKey,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    null)).Verifiable();

            _target.ValidateTotalWin(totalValidWin, transaction);

            _unitOfWorkFactory.Verify();
            _systemDisableManager.Verify();
        }

        [TestMethod]
        public void ConstructorDisableTest()
        {
            var models = new List<WinResultModel> { new() { IsTotalWinMismatched = true } };

            var repository = new Mock<IRepository<WinResultModel>>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            _unitOfWorkFactory.Setup(x => x.Create())
                .Returns(mockUnitOfWork.Object).Verifiable();
            mockUnitOfWork.Setup(x => x.Repository<WinResultModel>()).Returns(repository.Object).Verifiable();
            repository.Setup(x => x.Queryable()).Returns(models.AsQueryable()).Verifiable();

            _systemDisableManager.Setup(
                x => x.Disable(
                    BingoConstants.BingoWinMismatchKey,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    null)).Verifiable();

            _target = CreateTarget();

            repository.Verify();
            mockUnitOfWork.Verify();
            _unitOfWorkFactory.Verify();
            _systemDisableManager.Verify();
        }

        private TotalWinValidator CreateTarget(
            bool nullUnitOfWorkFactory = false,
            bool nullSystemDisableManager = false)
        {
            return new TotalWinValidator(
                nullUnitOfWorkFactory ? null : _unitOfWorkFactory.Object,
                nullSystemDisableManager ? null : _systemDisableManager.Object);
        }
    }
}