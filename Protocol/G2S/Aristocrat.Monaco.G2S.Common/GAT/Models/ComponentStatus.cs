namespace Aristocrat.Monaco.G2S.Common.GAT.Models
{
    using Storage;

    /// <summary>
    ///     Component status class
    /// </summary>
    public class ComponentStatus
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ComponentStatus" /> class.
        /// </summary>
        /// <param name="componentId">Component identifier</param>
        /// <param name="state">Component verification state</param>
        public ComponentStatus(string componentId, ComponentVerificationState state)
        {
            ComponentId = componentId;
            State = state;
        }

        /// <summary>
        ///     Gets component identifier
        /// </summary>
        public string ComponentId { get; }

        /// <summary>
        ///     Gets component verification state
        /// </summary>
        public ComponentVerificationState State { get; }
    }
}