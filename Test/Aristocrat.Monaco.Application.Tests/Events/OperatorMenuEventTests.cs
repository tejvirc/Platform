namespace Aristocrat.Monaco.Application.Tests.Events
{
    #region Using

    using Contracts.OperatorMenu;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for OperatorMenuEventTests
    /// </summary>
    [TestClass]
    public class OperatorMenuEventTests
    {
        [TestMethod]
        public void OperatorMenuEnteredEventConstructorTest()
        {
            string role = "Test";
            string operatorId = "1234";
            string eventToString = "OperatorMenuEnteredEvent ";
            string eventToStringWithId = $"OperatorMenuEnteredEvent {role} ID: {operatorId}";
            var target = new OperatorMenuEnteredEvent(role, operatorId);

            Assert.IsNotNull(target);
            Assert.AreEqual(role, target.Role);
            Assert.AreEqual(operatorId, target.OperatorId);
            Assert.AreEqual(eventToStringWithId, target.ToString());

            target = new OperatorMenuEnteredEvent();

            Assert.IsNotNull(target);
            Assert.AreEqual("", target.Role);
            Assert.AreEqual(eventToString, target.ToString());
        }

        [TestMethod]
        public void OperatorMenuExitedEventDefaultConstructorTest()
        {
            string operatorId = "1234";
            string eventToString = "OperatorMenuExitedEvent";
            string eventToStringWithId = $"OperatorMenuExitedEvent ID: {operatorId}";

            var target = new OperatorMenuExitedEvent();
            Assert.IsNotNull(target);
            Assert.AreEqual(eventToString, target.ToString());

            target = new OperatorMenuExitedEvent(operatorId);
            Assert.IsNotNull(target);
            Assert.AreEqual(eventToStringWithId, target.ToString());

        }
    }
}
