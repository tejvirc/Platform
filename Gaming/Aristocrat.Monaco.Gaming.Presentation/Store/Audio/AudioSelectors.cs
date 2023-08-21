namespace Aristocrat.Monaco.Gaming.Presentation.Store.Audio;

using Extensions.Fluxor;
using Hardware.Contracts.Audio;
using static Extensions.Fluxor.Selectors;

public static class AudioSelectors
{
    public static readonly ISelector<AudioState, VolumeScalar> SelectPlayerVolumeScalar = CreateSelector(
        (AudioState state) => state.PlayerVolumeScalar);
}
