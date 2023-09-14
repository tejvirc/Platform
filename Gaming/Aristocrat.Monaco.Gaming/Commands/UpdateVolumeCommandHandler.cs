namespace Aristocrat.Monaco.Gaming.Commands
{
    using Aristocrat.Monaco.Kernel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
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
            _properties.SetProperty(GamingConstants.GameVolumeKey, command.Volume);
        }
    }
}
