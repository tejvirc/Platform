namespace Aristocrat.Monaco.Sas.VoucherValidation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;

    /// <summary>
    ///     This SAS validation handler factory for getting the validation handler for the currently configured validation type
    /// </summary>
    public class SasValidationHandlerFactory
    {
        private readonly IPropertiesManager _propertiesManager;
        private readonly Dictionary<SasValidationType, IValidationHandler> _validationHandlers;

        /// <summary>
        ///     Create the SasValidationHandlerFactory instance
        /// </summary>
        /// <param name="propertiesManager">The properties manager</param>
        /// <param name="validationHandlers">The list of available validation handlers</param>
        public SasValidationHandlerFactory(IPropertiesManager propertiesManager, IEnumerable<IValidationHandler> validationHandlers)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _validationHandlers = validationHandlers?.ToDictionary(x => x.ValidationType) ??
                                  throw new ArgumentNullException(nameof(validationHandlers));
        }

        /// <summary>
        ///     Gets the validation handler for the current configured validation type
        /// </summary>
        /// <returns>The validation handler</returns>
        public IValidationHandler GetValidationHandler()
        {
            var validationType = _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures())
                .ValidationType;
            return _validationHandlers.TryGetValue(validationType, out var handler) ? handler : null;
        }
    }
}