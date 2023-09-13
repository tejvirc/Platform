namespace Aristocrat.Monaco.Gaming.Commands
{
    using Kernel;
    using System;
    using Contracts;

    public class UpdateVolumeCommandHandler
    {
        private readonly IPropertiesManager _properties;
        public UpdateVolumeCommandHandler(IPropertiesManager properties)

        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public void Handle(UpdateVolume command)
        {
            _properties.SetProperty(GamingConstants.GamePlayerVolumeScalarKey, command.Volume);
        }
    }
}
