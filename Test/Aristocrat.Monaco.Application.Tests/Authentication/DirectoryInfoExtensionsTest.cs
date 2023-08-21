namespace Aristocrat.Monaco.Application.Tests.Authentication
{
    using System;
    using System.IO;
    using Application.Authentication;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DirectoryInfoExtensionsTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenGetFilesByPatternDirectoryInfoIsNullExpectException()
        {
            var directoryInfo = (DirectoryInfo)null;
            directoryInfo.GetFilesByPattern("search pattern", SearchOption.AllDirectories);
        }
    }
}
