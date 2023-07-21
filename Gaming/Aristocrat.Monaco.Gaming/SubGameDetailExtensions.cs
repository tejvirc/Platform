namespace Aristocrat.Monaco.Gaming
{
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Newtonsoft.Json;

    /// <summary>
    ///     A set of <see cref="ISubGameDetails" /> extensions
    /// </summary>
    public static class SubGameDetailExtensions
    {
        /// <summary>
        ///  Serializes a group of sub games into a format for Runtime consumption.
        /// </summary>
        /// <param name="this">The group of sub games to serialize</param>
        /// <returns>A json string with the serialized sub games</returns>
        public static string Serialize(this IEnumerable<ISubGameDetails> @this)
        {
            var subGameSerialization = new SubGameSerialization
            {
                GameType = "SubGameDetails",
                SubGames = new List<SubGameInformation>()
            };

            foreach (var subGameInfo in @this.Select(
                         subGame => new SubGameInformation
                         {
                             GameId = subGame.Id, Denominations = subGame.SupportedDenoms
                         }))
            {
                subGameSerialization.SubGames.Add(subGameInfo);
            }

            return JsonConvert.SerializeObject(subGameSerialization);
        }
    }
}
