namespace Aristocrat.Monaco.Bingo.Common
{
    using GameOverlay;

    public interface IBingoCardProvider
    {
        /// <summary>
        ///     Requests a card with the provided card serial number
        /// </summary>
        /// <param name="cardSerial">The serial number to use</param>
        /// <returns>A new bingo card generated from the serial number</returns>
        BingoCard GetCardBySerial(uint cardSerial);
    }
}