namespace Aristocrat.Monaco.Gaming.Lobby.CommandHandlers;

using System;
using System.Linq;
using Aristocrat.LobbyRuntime.V1;
using Contracts;
using Kernel;

public class GetGamesCommandHandler : ICommandHandler<GetGames>
{
    private readonly IPropertiesManager _properties;

    public GetGamesCommandHandler(IPropertiesManager properties)
    {
        _properties = properties;
    }

    public void Handle(GetGames command)
    {
        var config = _properties.GetValue<LobbyConfiguration?>(GamingConstants.LobbyConfig, null);

        var games = _properties.GetValues<IGameDetail>(GamingConstants.Games).ToList();

        var gameCombos =
            from game in games
            from denom in game.ActiveDenominations
            where game.Enabled
            select new GameDetail
            {
                GameId = game.Id,
                Name = game.ThemeName,
                InstallDateTime = game.InstallDate,
                DllPath = game.GameDll,
                Denomination = denom,
                BetOption = game.Denominations.Single(d => d.Value == denom).BetOption,
                MinimumWagerCredits = game.MinimumWagerCredits,
                GameType = ConvertTo(game.GameType),
                GameSubtype = game.GameSubtype,
                Enabled = game.Enabled,
                ThemeId = game.ThemeId,
                Tags = game.GameTags.ToList(),
                Category = ConvertTo(game.Category),
                SubCategory = ConvertTo(game.SubCategory)
            };

        command.Games = gameCombos.ToList();
    }

    private static GameSubCategory ConvertTo(Contracts.Models.GameSubCategory subCategory) =>
        subCategory switch
        {
            Contracts.Models.GameSubCategory.BlackJack => GameSubCategory.BlackJack,
            Contracts.Models.GameSubCategory.Undefined => GameSubCategory.Undefined,
            Contracts.Models.GameSubCategory.OneHand => GameSubCategory.OneHand,
            Contracts.Models.GameSubCategory.ThreeHand => GameSubCategory.ThreeHand,
            Contracts.Models.GameSubCategory.FiveHand => GameSubCategory.FiveHand,
            Contracts.Models.GameSubCategory.TenHand => GameSubCategory.TenHand,
            Contracts.Models.GameSubCategory.SingleCard => GameSubCategory.SingleCard,
            Contracts.Models.GameSubCategory.FourCard => GameSubCategory.FourCard,
            Contracts.Models.GameSubCategory.MultiCard => GameSubCategory.MultiCard,
            Contracts.Models.GameSubCategory.Roulette => GameSubCategory.Roulette,
            _ => throw new ArgumentOutOfRangeException(nameof(subCategory))
        };

    private static GameCategory ConvertTo(Contracts.Models.GameCategory gameCategory) =>
        gameCategory switch
        {
            Contracts.Models.GameCategory.Undefined => GameCategory.Undefined,
            Contracts.Models.GameCategory.LightningLink => GameCategory.LightningLink,
            Contracts.Models.GameCategory.Slot => GameCategory.Slot,
            Contracts.Models.GameCategory.Keno => GameCategory.Keno,
            Contracts.Models.GameCategory.Poker => GameCategory.Poker,
            Contracts.Models.GameCategory.Table => GameCategory.Table,
            Contracts.Models.GameCategory.MultiDrawPoker => GameCategory.MultiDrawPoker,
            _ => throw new ArgumentOutOfRangeException(nameof(gameCategory))
        };

    private static GameType ConvertTo(Contracts.Models.GameType gameType) =>
        gameType switch
        {
            Contracts.Models.GameType.Undefined => GameType.Undefined,
            Contracts.Models.GameType.Slot => GameType.Slot,
            Contracts.Models.GameType.Poker => GameType.Poker,
            Contracts.Models.GameType.Keno => GameType.Keno,
            Contracts.Models.GameType.Blackjack => GameType.Blackjack,
            Contracts.Models.GameType.Roulette => GameType.Roulette,
            Contracts.Models.GameType.LightningLink => GameType.LightningLink,
            _ => throw new ArgumentOutOfRangeException(nameof(gameType))
        };
}
