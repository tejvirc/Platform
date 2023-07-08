namespace Aristocrat.Monaco.Gaming.Lobby.Store.EdgeLighting;

using Fluxor;

public static class EdgeLightingReducers
{
    [ReducerMethod]
    public static EdgeLightingState Reduce(EdgeLightingState state, UpdateEdgeLightStateAction payload) =>
        state with
        {
            EdgeLightState = payload.EdgeLightState,
        };
}
