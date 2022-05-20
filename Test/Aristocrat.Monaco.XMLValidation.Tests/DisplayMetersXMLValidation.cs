namespace Aristocrat.Monaco.XMLValidation.Tests
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    [TestClass]
    public class DisplayMetersXmlValidationTest
    {
        private static readonly string XsdFilePath = Path.GetFullPath(
            Path.Combine(Helper.StartupPath, @"..\..\..\..\Application\Monaco.Application.Contracts\MeterPage\DisplayMetersConfiguration.xsd"));

        private string[] _directoriesList;
        private string[] _fileList;

        [TestInitialize]
        public void Setup()
        {
            _directoriesList = Helper.GetDirectoriesList(Helper.XmlPathDirectory, Helper.FilterDirectoryList);
            _fileList = Helper.GetFilesList(Helper.XmlPathDirectory, "DisplayMeters*.xml");
            Assert.AreEqual(_directoriesList.Length, _fileList.Length);
        }

        [TestMethod]
        public void ValidateDisplayMetersXmlForEachJurisdiction()
        {
            Helper.ValidateXmlFilesWithXsd(_fileList, XsdFilePath);
        }
    }
}