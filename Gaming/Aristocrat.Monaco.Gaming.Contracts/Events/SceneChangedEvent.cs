namespace Aristocrat.Monaco.Gaming.Contracts.Events
{
    using Kernel;

    /// <summary>
    ///     This event holds game Scene Change information
    /// </summary>
    public class SceneChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="scene">The name of the scene we're changing to</param>
        public SceneChangedEvent(string scene)
        {
            Scene = scene;
        }

        /// <summary>
        ///     The name of the scene we're changing to.
        /// </summary>
        public string Scene { get; set; }
    }
}