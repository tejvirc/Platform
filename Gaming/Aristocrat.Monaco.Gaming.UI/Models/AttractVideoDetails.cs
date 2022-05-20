namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using System.Collections.Generic;
    using Contracts;

    public class AttractVideoDetails : IAttractDetails
    {
        public string TopperAttractVideoPath { get; set; }

        public string TopAttractVideoPath { get; set; }

        public string BottomAttractVideoPath { get; set; }

        public IDictionary<string, ILocaleGameGraphics> LocaleAttractGraphics { get; set; } =
            new Dictionary<string, ILocaleGameGraphics>();

        public string GetTopAttractVideoPathByLocaleCode(string localeCode)
        {
            return LocaleAttractGraphics.TryGetValue(localeCode, out var graphics) ? graphics.TopAttractVideo : null;
        }

        public string GetTopperAttractVideoPathByLocaleCode(string localeCode)
        {
            return LocaleAttractGraphics.TryGetValue(localeCode, out var graphics) ? graphics.TopperAttractVideo : null;
        }

        public string GetBottomAttractVideoPathByLocaleCode(string localeCode)
        {
            return LocaleAttractGraphics.TryGetValue(localeCode, out var graphics) ? graphics.BottomAttractVideo : null;
        }
    }
}