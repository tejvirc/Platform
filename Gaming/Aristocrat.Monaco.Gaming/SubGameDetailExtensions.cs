namespace Aristocrat.Monaco.Gaming
{
    using System.Collections.Generic;
    using Contracts;
    using Newtonsoft.Json;

    /// <summary>
    ///     A set of <see cref="ISubGameDetails" /> extensions
    /// </summary>
    public static class SubGameDetailExtensions
    {
        /// <summary>
        ///     Serializes a group of sub games into a format for Runtime consumption.
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

            foreach (var subGameInfo in @this)
            {
                subGameSerialization.SubGames.Add(
                    new SubGameInformation
                        {
                            GameId = subGameInfo.Id,
                            Denominations = subGameInfo.SupportedDenoms
                        });
            }

            return JsonConvert.SerializeObject(subGameSerialization);
        }
    }
}
