namespace Aristocrat.Monaco.Application.Localization
{
    using System;
    using System.Collections.Generic;
    using Contracts.Localization;
    using Kernel.Contracts.MessageDisplay;

    public class TranslationService : ITranslationService
    {
        public string Name => nameof(TranslationService);

        public ICollection<Type> ServiceTypes => new[] { typeof(ITranslationService) };

        public void Initialize()
        {
        }

        public string GetString(string key, CultureProviderType providerType = CultureProviderType.Operator) => Localizer.GetString(key, providerType);
    }
}