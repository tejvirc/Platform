namespace Aristocrat.Monaco.Gaming.Lobby.Store.EdgeLighting;

using Fluxor;

public class EdgeLightingReducers
{
    [ReducerMethod]
    public static EdgeLightingState Reduce(EdgeLightingState state, StartupAction payload) =>
        state with { UseGen8IdleMode = payload.Configuration.EdgeLightingOverrideUseGen8IdleMode };
}
