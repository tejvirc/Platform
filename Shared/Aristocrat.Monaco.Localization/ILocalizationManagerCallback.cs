namespace Aristocrat.Monaco.Localization
{
    using System.Globalization;

    internal interface ILocalizationManagerCallback
    {
        string[] GetOverrideKeys(string key);

        CultureInfo GetCultureFor(string name);
    }
}
