namespace Aristocrat.Monaco.G2S.Tests.Extensions
{
    using System;
    using Data.Model;
    using G2S.Handlers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class G2SEnumExtensionsTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenApplyConditionToG2SStringIsNotInCaseExpectException()
        {
            var applyCondition = (ApplyCondition)1000;
            applyCondition.ToG2SString();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenDisableConditionToG2SStringIsNotInCaseExpectException()
        {
            var disableCondition = (DisableCondition)1000;
            disableCondition.ToG2SString();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenApplyConditionFromG2SStringIsNotInCaseExpectException()
        {
            var applyCondition = "G2S_notcased";
            applyCondition.ApplyConditionFromG2SString();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenTimeoutActionFromG2SStringIsNotInCaseExpectException()
        {
            var timeOutAction = "G2S_notcased";
            timeOutAction.TimeoutActionFromG2SString();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenDeviceClassToG2SStringIsNotInCaseExpectException()
        {
            var deviceClass = (DeviceClass)1000;
            deviceClass.DeviceClassToG2SString();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenDeviceClassFromG2SStringIsNotInCaseExpectException()
        {
            var deviceClass = "G2S_notcased";
            deviceClass.DeviceClassFromG2SString();
        }

        [TestMethod]
        public void ApplyConditionToG2SStringTest()
        {
            ExtensionsTestHelper.AssertEnumToG2SData<ApplyCondition, string>(G2SEnumExtensions.ToG2SString);
        }

        [TestMethod]
        public void DisableConditionToG2SStringTest()
        {
            ExtensionsTestHelper.AssertEnumToG2SData<DisableCondition, string>(G2SEnumExtensions.ToG2SString);
        }

        [TestMethod]
        public void DisableConditionFromG2SStringTest()
        {
            var strings = new[] { "G2S_none", "G2S_idle", "G2S_immediate", "G2S_zeroCredits" };

            ExtensionsTestHelper.AssertG2SStringsToEnum(G2SEnumExtensions.DisableConditionFromG2SString, strings);

            Assert.AreEqual(strings.Length, Enum.GetNames(typeof(DisableCondition)).Length);
        }

        [TestMethod]
        public void ApplyConditionFromG2SStringTest()
        {
            var strings = new[] { "G2S_immediate", "G2S_disable", "G2S_egmAction", "G2S_cancel" };

            ExtensionsTestHelper.AssertG2SStringsToEnum(G2SEnumExtensions.ApplyConditionFromG2SString, strings);

            Assert.AreEqual(strings.Length, Enum.GetNames(typeof(ApplyCondition)).Length);
        }

        [TestMethod]
        public void TimeoutActionFromG2SStringTest()
        {
            var strings = new[] { "G2S_abort", "G2S_ignore" };

            ExtensionsTestHelper.AssertG2SStringsToEnum(G2SEnumExtensions.TimeoutActionFromG2SString, strings);

            Assert.AreEqual(strings.Length, Enum.GetNames(typeof(TimeoutActionType)).Length);
        }

        [TestMethod]
        public void DeviceClassToG2SStringTest()
        {
            ExtensionsTestHelper.AssertEnumToG2SData(
                G2SEnumExtensions.DeviceClassToG2SString,
                new[] { DeviceClass.Note, DeviceClass.AuditMeters });
        }

        [TestMethod]
        public void DeviceClassFromG2SStringTest()
        {
            var strings = new[]
            {
                "G2S_communications",
                "G2S_cabinet",
                "G2S_eventHandler",
                "G2S_meters",
                "G2S_gamePlay",
                "G2S_deviceConfig",
                "G2S_commConfig",
                "G2S_optionConfig",
                "G2S_download",
                "G2S_handpay",
                "G2S_coinAcceptor",
                "G2S_noteAcceptor",
                "G2S_hopper",
                "G2S_noteDispenser",
                "G2S_printer",
                "G2S_progressive",
                "G2S_idReader",
                "G2S_bonus",
                "G2S_player",
                "G2S_voucher",
                "G2S_wat",
                "G2S_gat",
                "G2S_central",
                "G2S_all",
                "G2S_dft",
                "G2S_employee",
                "G2S_gameTheme",
                "G2S_hardware",
                "G2S_informedPlayer",
                "G2S_tournament",
                "G2S_spc",
                "GTK_cashout",
                "GTK_storage",
                "IGT_mediaDisplay",
                "IGT_smartCard"
            };

            ExtensionsTestHelper.AssertG2SStringsToEnum(G2SEnumExtensions.DeviceClassFromG2SString, strings);
        }
    }
}