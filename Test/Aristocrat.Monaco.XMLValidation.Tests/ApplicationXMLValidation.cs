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
    public class ApplicationXmlValidationTest
    {
        private static readonly string XsdFilePath = Path.GetFullPath(
            Path.Combine(Helper.StartupPath, @"..\..\..\..\Application\Monaco.Application.Contracts\ApplicationConfiguration.xsd"));

        private string[] _directoriesList;
        private string[] _fileList;

        [TestInitialize]
        public void Setup()
        {
            _directoriesList = Helper.GetDirectoriesList(Helper.XmlPathDirectory, Helper.FilterDirectoryList);
            _directoriesList = _directoriesList.Where(x => (!x.Contains("Illinois-VLT"))).ToArray();
            _fileList = Helper.GetFilesList(Helper.XmlPathDirectory, "Application*.xml");
            _fileList = _fileList.Where(x => (!x.Contains("Illinois-VLT"))).ToArray();
            Assert.AreEqual(_directoriesList.Length, _fileList.Length);
        }

        [TestMethod]
        public void ValidateApplicationXmlForEachJurisdiction()
        {
            Helper.ValidateXmlFilesWithXsd(_fileList, XsdFilePath);
        }
    }
}