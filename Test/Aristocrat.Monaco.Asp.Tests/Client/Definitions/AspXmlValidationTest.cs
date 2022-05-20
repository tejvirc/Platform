namespace Aristocrat.Monaco.Asp.Tests.Client.Definitions
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using Aristocrat.Monaco.Asp.Client.Devices;

    [TestClass]
    public class AspXmlValidationTest
    {
        [TestMethod]
        public void ValidateAspDefintions()
        {
            var validationTestAssembly = Assembly.GetExecutingAssembly();

            var aspAssembly = Assembly.Load(validationTestAssembly.GetReferencedAssemblies().First(o => o.Name == "Aristocrat.Monaco.Asp"));
            string resourceNamePrefix = "Aristocrat.Monaco.Asp.Client.Definitions";
            string[] xmlDefinitionFileNames = new string[] { "Asp1000.xml", "Asp2000.xml" };
            string[] xmlDefinitionFileFullName = new string[xmlDefinitionFileNames.Length];
            for (int i = 0; i < xmlDefinitionFileNames.Length; i++)
            {
                xmlDefinitionFileFullName[i] = $"{resourceNamePrefix}.{xmlDefinitionFileNames[i]}";
            }

            string xsdFileFullName = $"{resourceNamePrefix}.Devices.xsd";

            string[] assemblyResourceNames = aspAssembly.GetManifestResourceNames();

            Assert.IsTrue(xmlDefinitionFileFullName.Where(o => assemblyResourceNames.Contains(o)).Count() == xmlDefinitionFileFullName.Length, "One or more expected Aristocrat.Monaco.Asp.Client.Definitions files are missing ");

            Assert.IsTrue(aspAssembly.GetManifestResourceNames().Contains(xsdFileFullName), $"{xsdFileFullName} file is missing");
            XmlReaderSettings xrs = new XmlReaderSettings();
            xrs.ConformanceLevel = ConformanceLevel.Document;
            xrs.CheckCharacters = true;
            xrs.IgnoreComments = true;
            xrs.ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings | XmlSchemaValidationFlags.ProcessIdentityConstraints | XmlSchemaValidationFlags.ProcessSchemaLocation;
            xrs.ValidationType = ValidationType.Schema;

            string resourceName = string.Empty;

            xrs.ValidationEventHandler += (object sender, ValidationEventArgs e) =>
            {
                if (e.Severity == XmlSeverityType.Error)
                {
                    throw new ArgumentException($"The {resourceName} file does not match its {xsdFileFullName} file schema {sender}. The error message is {e.Message}. The line number is {e.Exception.LineNumber}");
                }
            };

            Stream xsdStream = aspAssembly.GetManifestResourceStream(aspAssembly.GetManifestResourceNames().First(o => o == xsdFileFullName));
            XmlReader xsdReader = XmlReader.Create(xsdStream);

            xrs.Schemas.Add("", xsdReader);

            for (int i = 0; i < xmlDefinitionFileFullName.Length; i++)
            {
                resourceName = xmlDefinitionFileFullName[i];
                using (Stream stream = aspAssembly.GetManifestResourceStream(resourceName))
                {
                    using (XmlReader reader = XmlReader.Create(stream, xrs))
                    {
                        while (reader.Read()) ;
                    }
                }

                var xmlSerializer = new XmlSerializer(typeof(Devices));
                using (Stream stream = aspAssembly.GetManifestResourceStream(resourceName))
                {
                    using (XmlReader reader = XmlReader.Create(stream))
                    {
                        Assert.IsTrue(xmlSerializer.CanDeserialize(reader));
                    }
                }
            }
        }
    }
}