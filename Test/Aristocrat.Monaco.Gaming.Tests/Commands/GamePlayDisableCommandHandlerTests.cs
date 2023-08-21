namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System;
    using Contracts;
    using Gaming.Commands;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Vgt.Client12.Application.OperatorMenu;

    [TestClass]
    public class GamePlayDisabledCommandHandlerTests
    {
        private GamePlayDisabledCommandHandler _target;
        private Mock<IResponsibleGaming> _responsibleGaming;
        private Mock<IOperatorMenuLauncher> _operatorMenu;
        private Mock<IHandpayRuntimeFlagsHelper> _helper;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _responsibleGaming = new Mock<IResponsibleGaming>(MockBehavior.Default);
            _operatorMenu = new Mock<IOperatorMenuLauncher>(MockBehavior.Default);
            _helper = new Mock<IHandpayRuntimeFlagsHelper>(MockBehavior.Default);

            _target = CreateGamePlayDisabledCommandHandler();
        }

        [DataRow(true, false, false, DisplayName = "Null Responsible Gaming Test")]
        [DataRow(false, true, false, DisplayName = "Null Operator Menu Launcher Test")]
        [DataRow(false, false, true, DisplayName = "Null HandpayRuntimeFlagsHelper Test")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorTest(
            bool nullResponsibleGaming,
            bool nullOperatorMenu,
            bool nullHelper)
        {
            _target = CreateGamePlayDisabledCommandHandler(
                nullResponsibleGaming,
                nullOperatorMenu,
                nullHelper);
        }

        private GamePlayDisabledCommandHandler CreateGamePlayDisabledCommandHandler(
            bool nullResponsibleGaming = false,
            bool nullOperatorMenu = false,
            bool nullHelper = false)
        {
            return new GamePlayDisabledCommandHandler(
                nullResponsibleGaming ? null : _responsibleGaming.Object,
                nullOperatorMenu ? null : _operatorMenu.Object,
                nullHelper ? null : _helper.Object);
        }
    }
}
