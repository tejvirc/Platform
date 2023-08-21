namespace Aristocrat.Monaco.Bingo.UI.Tests
{
    using System;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Bingo.UI.Converters;
    using Aristocrat.Monaco.Test.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ServerEnableDisableSettingsToCustomTextConverterTests
    {
        private const string EnabledKey = "this is the enabled key";
        private const string DisabledKey = "this guy is turned off";

        private enum MyEnum
        {
            Value
        }

        private Mock<ILocalizer> _localizer;
        private ServerEnableDisableSettingsToCustomTextConverter _target;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _localizer = MoqServiceManager.CreateAndAddService<ILocalizer>(MockBehavior.Strict);

            _target = new ServerEnableDisableSettingsToCustomTextConverter(_localizer.Object)
            {
                EnabledResourceKey = EnabledKey,
                DisabledResourceKey = DisabledKey
            };
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        private object Convert(object val)
        {
            return _target.Convert(val, null, null, null);
        }

        [TestMethod]
        public void ObjectInputShouldReturnEmptyString()
        {
            Assert.AreEqual(string.Empty, Convert(It.IsAny<object>()));
        }

        [DataTestMethod]
        [DataRow(42, DisplayName = "integer test")]
        [DataRow(MyEnum.Value, DisplayName = "enum test")]
        [DataRow(null, DisplayName ="null test")]
        public void NonStringInputShouldReturnEmptyString(object val)
        {
            Assert.AreEqual(string.Empty, Convert(val));
        }

        [TestMethod]
        public void UnsupportedStringsShouldBeReturned()
        {
            const string TestValue = "should be returned verbatim";
            _localizer
                .Setup(m => m.GetString(It.IsAny<string>()))
                .Throws(new Exception("shouldn't be called"));

            Assert.AreEqual(TestValue, Convert(TestValue));
        }

        [TestMethod]
        public void UnsupportedStringsShouldNotUseLocalizer()
        {
            const string TestValue = "unsupported value";
            _localizer
                .Setup(m => m.GetString(It.IsAny<string>()))
                .Throws(new Exception("shouldn't be called"));

            Convert(TestValue);
        }

        [DataTestMethod]
        [DataRow("enabled", DisplayName = "enabled test")]
        [DataRow("Enabled", DisplayName = "Enabled test")]
        [DataRow("enable", DisplayName = "enable test")]
        [DataRow("Enable", DisplayName = "Enable test")]
        [DataRow("on", DisplayName = "on test")]
        [DataRow("On", DisplayName = "On test")]
        [DataRow("true", DisplayName = "true test")]
        [DataRow("True", DisplayName = "True test")]
        [DataRow("1", DisplayName = "1 test")]
        public void VerifyEnabledKeyIsUsed(string val)
        {
            const string ExpectedResult = "expecting this guy";

            _localizer.Setup(m => m.GetString(EnabledKey)).Returns(ExpectedResult);
            Assert.AreEqual(ExpectedResult, Convert(val));
        }

        [DataTestMethod]
        [DataRow("disabled", DisplayName = "disabled test")]
        [DataRow("Disabled", DisplayName = "Disabled test")]
        [DataRow("disable", DisplayName = "disable test")]
        [DataRow("Disable", DisplayName = "Disable test")]
        [DataRow("off", DisplayName = "off test")]
        [DataRow("Off", DisplayName = "Off test")]
        [DataRow("false", DisplayName = "false test")]
        [DataRow("False", DisplayName = "False test")]
        [DataRow("0", DisplayName = "0 test")]
        public void VerifyDisabledKeyIsUsed(string val)
        {
            const string ExpectedResult = "expecting another guy";

            _localizer.Setup(m => m.GetString(DisabledKey)).Returns(ExpectedResult);
            Assert.AreEqual(ExpectedResult, Convert(val));
        }
    }
}
