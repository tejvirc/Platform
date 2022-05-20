namespace Aristocrat.Monaco.Gaming.Commands
{
    using Runtime.Client;

    /// <summary>
    ///     Game runtime request
    /// </summary>
    public class RuntimeRequest
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RuntimeRequest" /> class.
        /// </summary>
        /// <param name="state">The new state</param>
        public RuntimeRequest(RuntimeRequestState state)
        {
            State = state;
        }

        /// <summary>
        ///     Gets or sets the requested state
        /// </summary>
        public RuntimeRequestState State { get; }

        /// <summary>
        ///     Gets or sets the result of the requested state change
        /// </summary>
        public bool Result { get; set; }
    }
}
