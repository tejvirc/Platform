namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System.Diagnostics;
    using System.Globalization;
    using Application.Contracts.Extensions;
    using Contracts.Models;
    using Models;
    using Toolkit.Mvvm.Extensions;

    /// <summary>
    ///     Defines the LobbyViewModel class
    /// </summary>
    public partial class LobbyViewModel
    {
        [Conditional("DESIGN")]
        private void WireDesignerData()
        {
            if (Execute.InDesigner)
            {
                GameList.Add(
                    new GameInfo
                    {
                        Name = "FlaminHits",
                        ImagePath = @"Assets/Images/GameIcons/1c_WildLeprecoins.png",
                        Denomination = 1000,
                        GameType = GameType.Slot,
                        PlatinumSeries = false
                    });
                GameList.Add(
                    new GameInfo
                    {
                        Name = "FlaminHits",
                        ImagePath = @"Assets/Images/GameIcons/5c_WelcomeTo_WelcomeToPlatinum.png",
                        Denomination = 5000,
                        GameType = GameType.Slot,
                        PlatinumSeries = true
                    });
                GameList.Add(
                    new GameInfo
                    {
                        Name = "FlaminHits",
                        ImagePath = @"Assets/Images/GameIcons/25c_WildFortune.png",
                        Denomination = 25000,
                        GameType = GameType.Slot,
                        PlatinumSeries = false
                    });
                GameList.Add(
                    new GameInfo
                    {
                        Name = "FlaminHits",
                        ImagePath = @"Assets/Images/GameIcons/1c_FlaminHit_FlaminHitsPlatinum.png",
                        ProgressiveOrBonusValue = string.Format(new CultureInfo(ActiveLocaleCode), "{0}", 1100.FormattedCurrencyString()),
                        Denomination = 1000,
                        GameType = GameType.Slot,
                        PlatinumSeries = true
                    });
                GameList.Add(
                    new GameInfo
                    {
                        Name = "WildLepreCoins",
                        ImagePath = @"Assets/Images/GameIcons/25c_FlushStreak.png",
                        Denomination = 25000,
                        GameType = GameType.Slot,
                        PlatinumSeries = false
                    });
                GameList.Add(
                    new GameInfo
                    {
                        Name = "WildLepreCoins",
                        ImagePath = @"Assets/Images/GameIcons/25c_Chili7s.png",
                        Denomination = 25000,
                        GameType = GameType.Slot,
                        PlatinumSeries = false,
                        IsNew = true
                    });
                GameList.Add(
                    new GameInfo
                    {
                        Name = "WildLepreCoins",
                        ImagePath = @"Assets/Images/GameIcons/5c_WelcomeTo_WelcomeToPlatinum.png",
                        Denomination = 5000,
                        GameType = GameType.Slot,
                        PlatinumSeries = false
                    });
                GameList.Add(
                    new GameInfo
                    {
                        Name = "WildLepreCoins",
                        ImagePath = @"Assets/Images/GameIcons/1c_WildSplash.png",
                        Denomination = 1000,
                        GameType = GameType.Slot,
                        PlatinumSeries = false
                    });
                GameList.Add(
                    new GameInfo
                    {
                        Name = "WildFortune",
                        ImagePath = @"Assets/Images/GameIcons/5c_FortunesOfTheNile.png",
                        Denomination = 5000,
                        GameType = GameType.Slot,
                        PlatinumSeries = false
                    });
                GameList.Add(
                    new GameInfo
                    {
                        Name = "WildFortune",
                        ImagePath = @"Assets/Images/GameIcons/5c_SunMoon.png",
                        Denomination = 5000,
                        GameType = GameType.Slot,
                        PlatinumSeries = false
                    });
                GameList.Add(
                    new GameInfo
                    {
                        Name = "WildFortune",
                        ImagePath = @"Assets/Images/GameIcons/1c_WheelGame.png",
                        Denomination = 1000,
                        GameType = GameType.Slot,
                        PlatinumSeries = false,
                        IsNew = true
                    });
                GameList.Add(
                    new GameInfo
                    {
                        Name = "WildFortune",
                        ImagePath = @"Assets/Images/GameIcons/1c_WhalesOfCash.png",
                        Denomination = 1000,
                        GameType = GameType.Slot,
                        PlatinumSeries = false,
                        IsNew = true
                    });
            }
        }
    }
}
