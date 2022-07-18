namespace Aristocrat.Monaco.Gaming.Commands
{
    /// <summary>
    ///     Defines the begin game round results.  This will likely only be used when  the central determination host
    ///     wants to report on the selected game results
    /// </summary>
    public class BeginGameRoundResults
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BeginGameRoundResults" /> class.
        /// </summary>
        /// <param name="presentationIndex"></param>
        public BeginGameRoundResults(long presentationIndex)
        {
            PresentationIndex = presentationIndex;
        }

        /// <summary>
        ///     Gets the presentation index used
        /// </summary>
        public long PresentationIndex { get; }
    }
}