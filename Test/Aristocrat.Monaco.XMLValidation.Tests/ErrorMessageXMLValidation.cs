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
    public class ErrorMessageXmlValidationTest
    {
        private static readonly string XsdFilePath = Path.GetFullPath(
            Path.Combine(Helper.StartupPath, @"..\..\..\..\Application\Monaco.Application.Contracts\ErrorMessage\ErrorMessageConfiguration.xsd"));

        private string[] _directoriesList;
        private string[] _fileList;

        [TestInitialize]
        public void Setup()
        {
            _directoriesList = Helper.GetDirectoriesList(Helper.XmlPathDirectory, Helper.FilterDirectoryList);
            _fileList = Helper.GetFilesList(Helper.XmlPathDirectory, "ErrorMessage*.xml");
        }

        [TestMethod]
        public void ValidateErrorMessageXmlForEachJurisdiction()
        {
            Helper.ValidateXmlFilesWithXsd(_fileList, XsdFilePath);
        }
    }
}