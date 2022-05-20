namespace Aristocrat.Monaco.Mgam.Services.Communications
{
    using System;
    using Aristocrat.Mgam.Client;
    using Common;

    /// <summary>
    ///     Registered instance event args.
    /// </summary>
    public class RegisteredInstanceEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RegisteredInstanceEventArgs"/> class.
        /// </summary>
        /// <param name="instance">The registered instance.</param>
        public RegisteredInstanceEventArgs(InstanceInfo instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            Instance = new RegisteredInstance(instance);
        }

        /// <summary>
        ///     Gets the registered instance.
        /// </summary>
        public RegisteredInstance Instance { get; }
    }
}
