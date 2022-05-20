namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System;
    using Contracts;
    using Gaming.Commands;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using PRNGLib;

    [TestClass]
    public class GetRandomNumberCommandHandlerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenRngFactoryIsNullExpectException()
        {
            var handler = new GetRandomNumberCommandHandler(null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var factory = new Mock<IRandomFactory>();
            var rng = new Mock<IPRNG>();

            factory.Setup(f => f.Create(RandomType.Gaming)).Returns(rng.Object).Verifiable();

            var handler = new GetRandomNumberCommandHandler(factory.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void WhenHandleExpectRandom()
        {
            const ulong range = 500;
            const ulong notSoRandom = 7;

            var factory = new Mock<IRandomFactory>();
            var rng = new Mock<IPRNG>();

            rng.Setup(r => r.GetValue(range)).Returns(notSoRandom);

            factory.Setup(f => f.Create(RandomType.Gaming)).Returns(rng.Object);

            var handler = new GetRandomNumberCommandHandler(factory.Object);

            var command = new GetRandomNumber(range);
            handler.Handle(command);

            Assert.AreEqual(command.Value, notSoRandom);
        }
    }
}
