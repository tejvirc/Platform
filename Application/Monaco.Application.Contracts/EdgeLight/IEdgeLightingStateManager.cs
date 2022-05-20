namespace Aristocrat.Monaco.Application.Contracts.EdgeLight
{
    using Hardware.Contracts.EdgeLighting;

    /// <summary>
    ///     Service that stores and handles the different states of the edgelight. It should be noted that they are not states but rather conditions,
    ///     as there is a list of states inside <c>EdgeLightingControllerService.cs</c> that can be active at the same time but the highest priority one
    ///     will be chosen to be displayed)
    /// </summary>
    public interface IEdgeLightingStateManager
    {
        /// <summary>
        ///     Adds a state to the state list to be rendered based on the highest priority.
        ///     List is stored on <c>EdgeLightingControllerService.cs.</c>
        /// </summary>
        /// <param name="state">The new edge lighting state to add to the list.</param>
        /// <returns>An <c>EdgeLightToken</c> that can be used to remove the state from the list.</returns>
        IEdgeLightToken SetState(EdgeLightState state);

        /// <summary>
        ///     Removes the state from the state list.
        /// </summary>
        /// <param name="stateToken">Token that was returned when setting state.</param>
        void ClearState(IEdgeLightToken stateToken);
    }
}