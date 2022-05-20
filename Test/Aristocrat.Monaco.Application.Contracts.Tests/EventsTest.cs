namespace Aristocrat.Monaco.Application.Contracts.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Operations;
    using OperatorMenu;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     Tests the events in the Application.Contracts namespace
    /// </summary>
    [TestClass]
    public class EventsTest
    {
        [TestMethod]
        public void AddinConfigurationCompleteEventTest()
        {
            Assert.IsNotNull(new AddinConfigurationCompleteEvent());
        }

        [TestMethod]
        public void ButtonDeckNavigatorEndedEventTest()
        {
            Assert.IsNotNull(new ButtonDeckNavigatorEndedEvent());
        }

        [TestMethod]
        public void ButtonDeckNavigatorStartedEventTest()
        {
            Assert.IsNotNull(new ButtonDeckNavigatorStartedEvent());
        }

        [TestMethod]
        public void CheckRebootWhilePrintingEventTest()
        {
            Assert.IsNotNull(new CheckRebootWhilePrintingEvent());
        }

        [TestMethod]
        public void MetersOperatorMenuEnteredEventTest()
        {
            Assert.IsNotNull(new MetersOperatorMenuEnteredEvent());
        }

        [TestMethod]
        public void MetersOperatorMenuExitedEventTest()
        {
            Assert.IsNotNull(new MetersOperatorMenuExitedEvent());
        }

        [TestMethod]
        public void OperatingHoursEnabledEventTest()
        {
            Assert.IsNotNull(new OperatingHoursEnabledEvent());
        }

        [TestMethod]
        public void OperatingHoursExpiredEventTest()
        {
            Assert.IsNotNull(new OperatingHoursExpiredEvent());
        }

        [TestMethod]
        public void OperatorMenuEnteredEventTest()
        {
            string role = "Administrator";
            var target = new OperatorMenuEnteredEvent(role);

            Assert.IsNotNull(target);
            Assert.AreEqual(role, target.Role);
        }

        [TestMethod]
        public void PeriodMetersClearedEventTest()
        {
            Assert.IsNotNull(new PeriodMetersClearedEvent());
        }

        [TestMethod]
        public void PreConfigBootCompleteEventTest()
        {
            Assert.IsNotNull(new PreConfigBootCompleteEvent());
        }

        [TestMethod]
        public void PrintButtonStatusEventTest()
        {
            bool visible = true;
            var target = new PrintButtonStatusEvent(visible);

            Assert.IsNotNull(target);
            Assert.IsTrue(target.Enabled);
            }

        [TestMethod]
        public void PageTitleEventTest()
        {
            string content = "Test";
            var target = new PageTitleEvent(content);

            Assert.IsNotNull(target);
            Assert.AreEqual(content, target.Content);
        }

        [TestMethod]
        public void PrintButtonClickedEventTest()
        {
            bool cancel = true;
            var target = new PrintButtonClickedEvent(cancel);

            Assert.IsNotNull(target);
            Assert.IsTrue(target.Cancel);
        }

        [TestMethod]
        public void ReprintTicketEventTest1()
        {
            Assert.IsNotNull(new ReprintTicketEvent());
        }

        [TestMethod]
        public void ReprintTicketEventTest2()
        {
            var validationNumber = "1234";
            var target = new ReprintTicketEvent(validationNumber, 0);

            Assert.IsNotNull(target);
            Assert.AreEqual(validationNumber, target.ValidationNumber);
        }

        [TestMethod]
        public void SystemDisabledByOperatorEventTest()
        {
            SystemDisabledByOperatorEvent theEvent = new SystemDisabledByOperatorEvent();
            Assert.IsNotNull(theEvent);
        }

        [TestMethod]
        public void SystemEnabledByOperatorEventTest()
        {
            SystemEnabledByOperatorEvent theEvent = new SystemEnabledByOperatorEvent();
            Assert.IsNotNull(theEvent);
        }

        [TestMethod]
        public void TimeUpdatedEventTest()
        {
            var target = new TimeUpdatedEvent();

            Assert.IsNotNull(target);

            target = new TimeUpdatedEvent(TimeSpan.MaxValue);
            Assert.AreEqual(TimeSpan.MaxValue, target.TimeUpdate);
        }

        [TestMethod]
        public void PersistenceClearAuthorizationChangedEventTestNoArgs()
        {
            var target = new PersistenceClearAuthorizationChangedEvent();

            Assert.IsNotNull(target);
            Assert.IsFalse(target.PartialClearAllowed);
            Assert.IsFalse(target.FullClearAllowed);
            Assert.IsNull(target.PartialClearDeniedReasons);
            Assert.IsNull(target.FullClearDeniedReasons);
        }

        [TestMethod]
        public void PersistenceClearAuthorizationChangedEventTestWithArgs()
        {
            bool allowPartial = true;
            bool allowFull = true;
            string[] partialReasons = { "test" };
            string[] fullReasons = { "arbitrary" };

            var target = new PersistenceClearAuthorizationChangedEvent(
                allowPartial,
                allowFull,
                partialReasons,
                fullReasons);

            Assert.IsNotNull(target);
            Assert.AreEqual(allowPartial, target.PartialClearAllowed);
            Assert.AreEqual(allowFull, target.FullClearAllowed);
            Assert.AreEqual(partialReasons, target.PartialClearDeniedReasons);
            Assert.AreEqual(fullReasons, target.FullClearDeniedReasons);
        }
    }
}
