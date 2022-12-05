namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using Asp.Client.DataSources;
    using Kernel.Contracts.MessageDisplay;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;

    [TestClass]
    public class ScreenMessageDataSourceTests
    {
        private const string Screen_Message_One = "Screen_Message_One";
        private const string Screen_Message_One_Value = "Screen_Message_One value";
        private const string Screen_Message_Two = "Screen_Message_Two";
        private const string Screen_Message_Two_Value = "Screen_Message_Two value";

        private Mock<IMessageDisplay> _messageDisplayService = new Mock<IMessageDisplay>(MockBehavior.Default);
        private ScreenMessageDataSource _dataSource;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _dataSource = new ScreenMessageDataSource(_messageDisplayService.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullContructorThrowsTest()
        {
            new ScreenMessageDataSource(null);
        }

        [TestMethod]
        public void DataSourceNameTest()
        {
            var expectedName = "ScreenMessages";
            Assert.AreEqual(expectedName, _dataSource.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InvalidMemberNameCallingSetMemberValueThrowsTest()
        {
            _dataSource.SetMemberValue("invalidMemberName", "Test");
        }

        [TestMethod]
        public void SetMemberValuePassedNonEmptyStringSendsDisplayMessageTest()
        {
            _dataSource.SetMemberValue(Screen_Message_One, Screen_Message_One_Value);
            _dataSource.SetMemberValue(Screen_Message_Two, Screen_Message_Two_Value);

            _messageDisplayService.Verify(v => v.DisplayMessage(It.Is<IDisplayableMessage>(b => b.Message == Screen_Message_One_Value &&
                                                                                                b.Priority == DisplayableMessagePriority.Immediate &&
                                                                                                b.Classification == DisplayableMessageClassification.Informative)
                                                                                            ),
                                                                                            Times.Once);

            _messageDisplayService.Verify(v => v.DisplayMessage(It.Is<IDisplayableMessage>(b => b.Message == Screen_Message_Two_Value &&
                                                                                                b.Priority == DisplayableMessagePriority.Immediate &&
                                                                                                b.Classification == DisplayableMessageClassification.Informative)
                                                                                            ),
                                                                                            Times.Once);

            _messageDisplayService.Verify(v => v.RemoveMessage(It.IsAny<Guid>()), Times.Never);
        }

        [TestMethod]
        public void SetMemberValuePassedEmptyStringClearsExistingDisplayMessageTest()
        {
            _dataSource.SetMemberValue(Screen_Message_One, Screen_Message_One_Value);
            _dataSource.SetMemberValue(Screen_Message_One, "");
            _dataSource.SetMemberValue(Screen_Message_One, Screen_Message_One_Value);

            _messageDisplayService.Verify(v => v.DisplayMessage(It.Is<IDisplayableMessage>(b => b.Message == Screen_Message_One_Value &&
                                                                                                b.Priority == DisplayableMessagePriority.Immediate &&
                                                                                                b.Classification == DisplayableMessageClassification.Informative)
                                                                                            ),
                                                                                            Times.Exactly(2));

            _messageDisplayService.Verify(v => v.DisplayMessage(It.Is<IDisplayableMessage>(b => b.Message == "" &&
                                                                                                b.Priority == DisplayableMessagePriority.Immediate &&
                                                                                                b.Classification == DisplayableMessageClassification.Informative)
                                                                                            ),
                                                                                            Times.Never);

            _messageDisplayService.Verify(v => v.RemoveMessage(It.IsAny<IDisplayableMessage>()), Times.Once);
        }

        [TestMethod]
        public void GetMemberValueReturnsCorrectMessagesTest()
        {
            _dataSource.SetMemberValue(Screen_Message_One, Screen_Message_One_Value);
            _dataSource.SetMemberValue(Screen_Message_Two, Screen_Message_Two_Value);

            var screenMessageOne = _dataSource.GetMemberValue(Screen_Message_One);
            var screenMessageTwo = _dataSource.GetMemberValue(Screen_Message_Two);

            Assert.AreEqual(screenMessageOne, Screen_Message_One_Value);
            Assert.AreEqual(screenMessageTwo, Screen_Message_Two_Value);
        }
    }
}