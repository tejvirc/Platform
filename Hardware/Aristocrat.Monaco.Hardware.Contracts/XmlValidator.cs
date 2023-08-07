namespace Aristocrat.Monaco.Hardware.Contracts
{
    using System.Reflection;
    using System.Xml;
    using System.Xml.Schema;
    using log4net;

    /// <summary>
    ///     Validates XML files against XSD file.
    /// </summary>
    public class XmlValidator
    {
        /// <summary>
        ///     Create a logger for use in this class.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>Indicates whether the file is valid or not.</summary>
        private static bool _valid;

        /// <summary>Settings file for parsing object.</summary>
        private readonly XmlReaderSettings _settings;

        /// <summary>XML Reader object to parse file.</summary>
        private XmlReader _xmlReader;

        /// <summary>Initializes a new instance of the <see cref="XmlValidator" /> class.</summary>
        public XmlValidator()
        {
            _settings = new XmlReaderSettings { ValidationType = ValidationType.Schema };
            _settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            _settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            _settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

            _settings.ValidationEventHandler += ValidationHandler;
        }

        /// <summary>
        ///     Validates XML file passed into function against internally indicated schema file.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns><see langword="true" /> if XML file is valid, <see langword="false" /> otherwise.</returns>
        public bool Validate(string fileName)
        {
            // Initially true, any validation failures will change this.
            _valid = true;
            _xmlReader = XmlReader.Create(fileName, _settings);

            while (_xmlReader.Read())
            {
            }

            return _valid;
        }

        /// <summary>Handler for validation errors.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event args.</param>
        private static void ValidationHandler(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
            {
                Logger.Warn(sender, args.Exception);
            }
            else if (args.Severity == XmlSeverityType.Error)
            {
                Logger.Error(sender, args.Exception);
                _valid = false;
            }
        }
    }
}