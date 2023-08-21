namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using Kernel;

    /// <summary>
    ///     An event used to clear the daubs for the bingo card and ball call.
    ///     The will also clear bingo patterns if any are displayed.
    ///     The current existing ball call and bingo card numbers will be shown without any daubs
    /// </summary>
    public class ClearBingoDaubsEvent : BaseEvent
    {
    }
}