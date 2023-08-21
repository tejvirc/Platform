namespace Aristocrat.Monaco.Application.EdgeLight
{
    /// <summary>
    ///     Interface for a EdgeLightComponent.
    /// </summary>
    public interface IEdgeLightHandler
    {
        /// <summary>
        ///     name of the Edge Light Component.
        /// </summary>
        string Name { get; }

        bool Enabled { get; }
    }
}