namespace Aristocrat.Monaco.XMLValidation.Tests
{
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SasDefaultXmlValidationTest
    {
        private static readonly string XsdFilePath = Path.GetFullPath(
            Path.Combine(Helper.StartupPath, @"..\..\..\..\Protocol\SAS\Aristocrat.Monaco.Sas.Contracts\SASDefaultConfiguration.xsd"));

        private string[] _fileList;

        [TestInitialize]
        public void Setup()
        {
            _fileList = Helper.GetFilesList(Helper.XmlPathDirectory, "SASDefault*.xml");
        }

        [TestMethod]
        public void ValidateSasDefaultXmlForEachJurisdiction()
        {
            Helper.ValidateXmlFilesWithXsd(_fileList, XsdFilePath);
        }

        [TestMethod]
        public void ValidateSasDefaultXmlForEachJurisdictionRequiringSas()
        {
            var xmls = Helper.GetFilesList(Helper.XmlPathDirectory, "ConfigWizard.config.xml");
            foreach (var path in xmls)
            {
                var wizard = DeserializeConfig(path);
                if (wizard?.ProtocolConfiguration.ProtocolsAllowed?.Any(p => p.Name == CommsProtocol.SAS) == true)
                {
                    var directory = Path.GetDirectoryName(path);
                    Assert.IsTrue(_fileList.Any(f => f.Contains(directory)));
                }                
            }
        }

        private ConfigWizardConfiguration DeserializeConfig(string filename)
        {
            var serializer = new XmlSerializer(typeof(ConfigWizardConfiguration));

            using (Stream reader = new FileStream(filename, FileMode.Open))
            {
                return (ConfigWizardConfiguration)serializer.Deserialize(reader);
            }
        }
    }
}