namespace Aristocrat.Monaco.Gaming.Lobby.Services.Sound;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aristocrat.Monaco.Application.Contracts;
using Contracts.Models;
using Hardware.Contracts.Audio;
using Kernel;
using Kernel.Contracts.Events;

public class SoundController : ISoundController
{
    private readonly IPropertiesManager _properties;
    private readonly IEventBus _eventBus;
    private readonly IAudio _audio;

    private readonly Dictionary<Sound, string> _soundFilePathMap = new();

    public SoundController(IPropertiesManager properties, IEventBus eventBus, IAudio audio)
    {
        _properties = properties;
        _eventBus = eventBus;
        _audio = audio;

        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        _eventBus.Subscribe<InitializationCompletedEvent>(this, Handle);
    }

    private Task Handle(InitializationCompletedEvent evt, CancellationToken cancellationToken)
    {
        _soundFilePathMap.Add(Sound.Touch, _properties.GetValue(ApplicationConstants.TouchSoundKey, string.Empty));
        _soundFilePathMap.Add(Sound.CoinIn, _properties.GetValue(ApplicationConstants.CoinInSoundKey, string.Empty));
        _soundFilePathMap.Add(Sound.CoinOut, _properties.GetValue(ApplicationConstants.CoinOutSoundKey, string.Empty));
        _soundFilePathMap.Add(Sound.FeatureBell, _properties.GetValue(ApplicationConstants.FeatureBellSoundKey, string.Empty));
        _soundFilePathMap.Add(Sound.Collect, _properties.GetValue(ApplicationConstants.CollectSoundKey, string.Empty));
        _soundFilePathMap.Add(Sound.PaperInChute, _properties.GetValue(ApplicationConstants.PaperInChuteSoundKey, string.Empty));

        foreach (var filePath in _soundFilePathMap.Values.Where(filePath => !string.IsNullOrEmpty(filePath)))
        {
            _audio.Load(Path.GetFullPath(filePath));
        }

        return Task.CompletedTask;
    }
}
