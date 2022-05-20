namespace Aristocrat.Monaco.Application.ErrorMessage
{
    using System;
    using System.Collections.Generic;
    using Kernel;
    using Kernel.Contracts.ErrorMessage;

    public class ErrorMessageMapping : IErrorMessageMapping
    {
        private const string ErrorMessageConfigurationExtensionPath = "/ErrorMessage/Configuration";
        private readonly Dictionary<string, string> _mappingDictionary = new Dictionary<string, string>();

        public (bool errorMapped, string mappedText) MapError(Guid sourceId, string sourceText)
        {
            if (string.IsNullOrWhiteSpace(sourceText))
            {
                return (false, string.Empty);
            }

            var errorMapped = false;
            var mappedText = sourceText;

            if (_mappingDictionary.ContainsKey(sourceId.ToString("D")))
            {
                mappedText = _mappingDictionary[sourceId.ToString("D")];
                errorMapped = true;
            }
            else if (_mappingDictionary.ContainsKey(sourceText.ToUpper()))
            {
                mappedText = _mappingDictionary[sourceText.ToUpper()];
                errorMapped = true;
            }

            return (errorMapped, mappedText);
        }

        public bool Initialize()
        {
            var configuration = ConfigurationUtilities.GetConfiguration(
                ErrorMessageConfigurationExtensionPath,
                () => new ErrorMessageConfiguration());
            if (configuration?.Message == null || configuration.Message.Length == 0)
            {
                return false;
            }

            ParseConfiguration(configuration);

            return true;
        }

        private void ParseConfiguration(ErrorMessageConfiguration configuration)
        {
            foreach (var mapping in configuration.Message)
            {
                var key = Guid.TryParse(mapping.Source, out var id) ? id.ToString("D") : mapping.Source.ToUpper();

                _mappingDictionary.Add(key, mapping.Mapped);
            }
        }
    }
}