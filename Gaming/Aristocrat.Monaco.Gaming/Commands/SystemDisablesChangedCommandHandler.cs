namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts;

    /// <summary>
    ///     Command handler for the <see cref="SystemDisablesChanged" /> command.
    /// </summary>
    public class SystemDisablesChangedCommandHandler : ICommandHandler<SystemDisablesChanged>
    {
        private readonly IHandpayRuntimeFlagsHelper _helper;

        public SystemDisablesChangedCommandHandler(IHandpayRuntimeFlagsHelper helper)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        public void Handle(SystemDisablesChanged command)
        {
            _helper.SetHandpayRuntimeLockupFlags();
        }
    }
}