namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    /// <summary>
    ///     Implementation of <see cref="IHardwareHelper" />
    /// </summary>
    public class HardwareHelper : IHardwareHelper
    {
        /// <inheritdoc />
        public string Name => typeof(HardwareHelper).FullName;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IHardwareHelper) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public bool CheckForVirtualButtonDeckHardware()
        {
            return HardwareHelpers.CheckForVirtualButtonDeckHardware();
        }

        /// <inheritdoc />
        public bool CheckForUsbButtonDeckHardware()
        {
            return HardwareHelpers.CheckForUsbButtonDeckHardware();
        }
    }
}
