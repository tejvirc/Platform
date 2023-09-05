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

    [ReducerMethod]
    public static EdgeLightingState UpdateStateForCanOverride(
        EdgeLightingState state,
        EdgeLightUpdateOverrideAction action) =>
        state with
        {
            CanOverrideEdgeLight = action.CanOverrideEdgeLight,
        };
}