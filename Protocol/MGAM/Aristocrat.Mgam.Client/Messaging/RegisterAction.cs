namespace Aristocrat.Mgam.Client.Messaging
{
    using System;

    /// <summary>
    ///     The RegisterAction message is used to register an action with the site controller.
    /// </summary>
    public class RegisterAction : Request, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }

        /// <summary>
        ///     Gets or sets the GUID that identifies the action.
        /// </summary>
        public Guid ActionGuid { get; set; }

        /// <summary>
        ///     Gets or sets the name of this action.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the brief description of the action.
        /// </summary>
        public string Description { get; set; }
    }
}
