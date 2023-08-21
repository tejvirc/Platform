namespace Aristocrat.Monaco.XMLValidation.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConfigWizardXmlValidationTest
    {
        private static readonly string XsdFilePath = Path.GetFullPath(
            Path.Combine(Helper.StartupPath, @"..\..\..\..\Application\Monaco.Application.Contracts\ConfigWizard\ConfigWizardConfiguration.xsd"));

        private string[] _directoriesList;
        private string[] _fileList;

        [TestInitialize]
        public void Setup()
        {
            _directoriesList = Helper.GetDirectoriesList(Helper.XmlPathDirectory, Helper.FilterDirectoryList);
            _fileList = Helper.GetFilesList(Helper.XmlPathDirectory, "ConfigWizard*.xml");
            Assert.AreEqual(_directoriesList.Length, _fileList.Length);
        }

        [TestMethod]
        public void ValidateConfigWizardXmlForEachJurisdiction()
        {
            var invalidFiles = new Dictionary<string, List<string>>();

            var schemas = new XmlSchemaSet();
            schemas.Add("", XsdFilePath);
            foreach (var file in _fileList)
            {
                var accountingXml = XDocument.Load(file);
                accountingXml.Validate(
                    schemas,
                    (o, e) =>
                    {
                        if (e.Severity == XmlSeverityType.Error)
                            if (invalidFiles.Any(a => a.Key == file)) invalidFiles[file].Add(e.Message);
                            else invalidFiles.Add(file, new List<string> { e.Message });
                    });
            }

            if (invalidFiles.Any())
            {
                var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine("ConfigWizard.config.xml files are invalid for the following jurisdictions:");
                sb.AppendLine();

                var padding = invalidFiles.Select(f => GetJurisdiction(f.Key).Length).Max() + 10;

                invalidFiles.Keys.ToList().ForEach(f =>
                {
                    var jurisdiction = GetJurisdiction(f);
                    var isFirst = true;

                    invalidFiles[f].ForEach(error =>
                    {
                        sb.AppendLine($"{(isFirst ? jurisdiction : "").PadRight(padding, ' ')} {error}");
                        isFirst = false;
                    });
                });

                Debug.WriteLine(sb.ToString());
                Debug.WriteLine("");

                Assert.Fail("ConfigWizard.config.xml files are invalid for one or more jurisdiction");
            }
        }

        private static string GetJurisdiction(string fileName)
        {
            var jurisdictionIndex = fileName.IndexOf(@"\jurisdiction\", StringComparison.InvariantCultureIgnoreCase);
            return fileName.Substring(jurisdictionIndex + @"\jurisdiction\".Length).Replace(@"\ConfigWizard.config.xml", "");
        }
    }
}