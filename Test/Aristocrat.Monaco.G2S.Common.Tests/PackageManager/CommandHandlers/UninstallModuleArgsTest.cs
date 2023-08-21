namespace Aristocrat.Monaco.G2S.Common.Tests.PackageManager.CommandHandlers
{
    using System;
    using Common.PackageManager.CommandHandlers;
    using Common.PackageManager.Storage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UninstallModuleArgsTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullModuleEntityExpectException()
        {
            var args = new UninstallModuleArgs(null, arg => { });

            Assert.IsNull(args);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectValidPropertiesSet()
        {
            var module = new Module();
            Action<UninstallModuleArgs> action = arg => { };

            var args = new UninstallModuleArgs(module, action);

            Assert.AreEqual(module, args.ModuleEntity);
            Assert.AreEqual(action, args.UninstallModuleCallback);
        }
    }
}