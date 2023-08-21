namespace Aristocrat.Monaco.Gaming.Presentation.Store.EdgeLighting;

using Fluxor;

public static class EdgeLightingReducers
{
    [ReducerMethod]
    public static EdgeLightingState UpdateState(EdgeLightingState state, EdgeLightUpdateStateAction action) =>
        state with
        {
            EdgeLightState = action.EdgeLightState,
        };
}
