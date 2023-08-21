namespace Aristocrat.Monaco.Gaming.Presentation.Store.Audio;

using Hardware.Contracts.Audio;
using Fluxor;
using System.Linq;

public static class AudioReducers
{
    [ReducerMethod()]
    public static AudioState Reduce(AudioState state, AudioLoadedAction action)
    {
        return state with
        {
            SoundFiles = action.SoundFiles,
            CurrentSound = action.SoundFiles.FirstOrDefault()
        };
    }

    [ReducerMethod(typeof(AudioChangeVolumeAction))]
    public static AudioState Reduce(AudioState state)
    {
        return state with
        {
            PlayerVolumeScalar = state.PlayerVolumeScalar == VolumeScalar.Scale100 ? VolumeScalar.Scale20 : state.PlayerVolumeScalar + 1
        };
    }

    [ReducerMethod]
    public static AudioState Reduce(AudioState state, AudioUpdatePlayerVolumeScalarAction action)
    {
        return state with
        {
            PlayerVolumeScalar = action.VolumeScalar
        };
    }
}
