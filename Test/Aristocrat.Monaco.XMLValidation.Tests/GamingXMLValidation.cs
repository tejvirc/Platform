namespace Aristocrat.Monaco.XMLValidation.Tests
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    [TestClass]
    public class GamingXmlValidationTest
    {
        private static readonly string XsdFilePath = Path.GetFullPath(
            Path.Combine(Helper.StartupPath, @"..\..\..\..\Gaming\Aristocrat.Monaco.Gaming.Contracts\GamingConfiguration.xsd"));

        private string[] _directoriesList;
        private string[] _fileList;

        [TestInitialize]
        public void Setup()
        {
            _directoriesList = Helper.GetDirectoriesList(Helper.XmlPathDirectory, Helper.FilterDirectoryList);
            _fileList = Helper.GetFilesList(Helper.XmlPathDirectory, "Gaming*.xml");
            Assert.AreEqual(_directoriesList.Length, _fileList.Length);
        }

        [TestMethod]
        public void ValidateGamingXmlForEachJurisdiction()
        {
            Helper.ValidateXmlFilesWithXsd(_fileList, XsdFilePath);
        }
    }
}