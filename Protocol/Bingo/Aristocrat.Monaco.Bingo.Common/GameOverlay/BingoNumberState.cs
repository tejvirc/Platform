namespace Aristocrat.Monaco.Bingo.Common.GameOverlay
{
    /// <summary>
    ///     States of bingo numbers.
    /// </summary>
    public enum BingoNumberState
    {
        /// <summary>Card, initial</summary>
        CardInitial,

        /// <summary>Card, covered</summary>
        CardCovered,

        /// <summary>Ball call, initial</summary>
        BallCallInitial,

        /// <summary>Ball call, late</summary>
        BallCallLate,

        /// <summary>Help page</summary>
        HelpPattern,

        /// <summary>Undefined</summary>
        Undefined
    }
}
