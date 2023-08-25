namespace Aristocrat.Monaco.Gaming.Presentation.Store.Audio;

using Hardware.Contracts.Audio;
using Fluxor;
using System.Linq;

public static class AudioReducers
{
    [ReducerMethod()]
    public static AudioState Loaded(AudioState state, AudioLoadedAction action)
    {
        return state with
        {
            SoundFiles = action.SoundFiles
        };
    }

    [ReducerMethod(typeof(AudioChangeVolumeAction))]
    public static AudioState ChangeVolume(AudioState state)
    {
        return state with
        {
            PlayerVolumeScalar = state.PlayerVolumeScalar == VolumeScalar.Scale100 ? VolumeScalar.Scale20 : state.PlayerVolumeScalar + 1
        };
    }

    [ReducerMethod]
    public static AudioState UpdatePlayerVolumeScalar(AudioState state, AudioUpdatePlayerVolumeScalarAction action)
    {
        return state with
        {
            PlayerVolumeScalar = action.VolumeScalar
        };
    }
}
