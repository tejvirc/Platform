namespace Aristocrat.Monaco.Bingo.Extensions
{
    using System;
    using Common.Storage;

    /// <summary>
    ///     Extension methods for <see cref="BingoGameDescription"/>
    /// </summary>
    public static class BingoGameDescriptionExtensions
    {
        /// <summary>
        ///     Gets the paytableId that needs to be reported to the server as an integer
        /// </summary>
        /// <param name="description">The <see cref="BingoGameDescription"/> to get the paytable for</param>
        /// <returns>The paytable Id as an integer or zero if it can't be converted</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="description"/> is null</exception>
        public static int GetPaytableID(this BingoGameDescription description)
        {
            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }

            return int.TryParse(description.Paytable, out var id) ? id : 0;
        }
    }
}