namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System.Linq;
    using Common.Storage.Model;
    using Gaming.Contracts;
    using Kernel;
    using Protocol.Common.Storage.Entity;

    public static class UnitOfWorkFactoryExtensions
    {
        public static BingoGameConfiguration GetSelectedGameConfiguration(
            this IUnitOfWorkFactory unitOfWorkFactory,
            IPropertiesManager propertiesManager)
        {
            var (game, denomination) = propertiesManager.GetActiveGame();
            if (game is null || denomination is null)
            {
                return null;
            }

            var gameConfigurations = unitOfWorkFactory.Invoke(
                x => x.Repository<BingoServerSettingsModel>().Queryable().SingleOrDefault())?.GamesConfigured;
            return gameConfigurations?.FirstOrDefault(
                x => x.PlatformGameId == game.Id && x.Denomination == denomination.Value);
        }
    }
}