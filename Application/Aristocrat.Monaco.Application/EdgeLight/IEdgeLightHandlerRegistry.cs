namespace Aristocrat.Monaco.Application.EdgeLight
{
    /// <summary>
    ///     Interface for EdgeLightHandler container. Provides access to a registered EdgeLightHandler by its name.
    /// </summary>
    public interface IEdgeLightHandlerRegistry
    {
        /// <summary>
        ///     Register's a EdgeLightHandler. Overwrites the existing EdgeLightHandler with same name if exists.
        /// </summary>
        /// <param name="handlerSource">The EdgeLightHandler object to register.</param>
        void RegisterHandler(IEdgeLightHandler handlerSource);

        /// <summary>
        ///     Returns a EdgeLightHandler by name if found otherwise null.
        /// </summary>
        /// <param name="name"> name of the EdgeLightHandler.</param>
        /// <returns></returns>
        IEdgeLightHandler GetHandler(string name);
    }
}