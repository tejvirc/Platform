namespace Aristocrat.Monaco.XMLValidation.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.Schema;

    internal static class Helper
    {
        // folder where we start up
        public static readonly string StartupPath = Environment.CurrentDirectory;

        // folder where jurisdiction data lies
        public static readonly string XmlPathDirectory = Path.GetFullPath(Path.Combine(StartupPath, @"..\..\..\..\bin\Debug\Platform\bin\jurisdiction"));

        // folders to filter out from testing
        public static readonly List<string> FilterDirectoryList = new List<string>() { Path.Combine(Helper.XmlPathDirectory, "DefaultAssets") };

        // Returns a list of all the files that matches the pattern by searching recursively
        // from the target directory passed
        public static string[] GetFilesList(string targetDirectory, string pattern)
        {
            // Process the list of files found in the directory.
            return Directory.GetFiles(targetDirectory, pattern, SearchOption.AllDirectories);
        }

        // Returns the subdirectories of this directory.
        public static string[] GetDirectoriesList(string targetDirectory, List<string>filterList)
        {
            var dirArr = Directory.GetDirectories(targetDirectory);
            dirArr = dirArr.Where(w => !filterList.Contains(w)).ToArray();
            return dirArr;
        }

        // Validates xml files using xsd schema - throws if validation fails
        public static void ValidateXmlFilesWithXsd(string[] fileList, string xsdFilePath)
        {
            var schemas = new XmlSchemaSet();
            schemas.Add("", xsdFilePath);
            foreach (var file in fileList)
            {
                var accountingXml = XDocument.Load(file);
                accountingXml.Validate(
                    schemas,
                    (o, e) =>
                    {
                        if (e.Severity == XmlSeverityType.Error)
                            throw new ArgumentException(
                                $"The {file} file does not match its {xsdFilePath} file schema.\n"
                                + $"The error message is {e.Message}\n"
                                + $"If the error message is not clear then some possible causes for the error could be as follows :\n"
                                + $"1) Mismatch in the order in which it's present in the {file} "
                                + $" and it's corresponding {xsdFilePath} schema\n"
                                + $"2) The new element is not present in the {xsdFilePath} schema");
                    });
            }
        }
    }
}