namespace Aristocrat.G2S.Client
{
    using System;
    using System.Threading.Tasks;
    using Devices;
    using Diagnostics;
    using Properties;

    /// <summary>
    ///     The CommandDispatcher is responsible for routing commands to the appropriate handler if the handler exists. The
    ///     dispatcher will generate basic application level errors if the message handler does not exist or fails during
    ///     validation or execution.
    /// </summary>
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly IDeviceConnector _devices;
        private readonly IHandlerConnector _handlers;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommandDispatcher" /> class.
        ///     Creates a new instance of the CommandDispatcher
        /// </summary>
        /// <param name="handlers">The handler connector</param>
        /// <param name="devices">The device connector</param>
        public CommandDispatcher(IHandlerConnector handlers, IDeviceConnector devices)
        {
            _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
            _devices = devices ?? throw new ArgumentNullException(nameof(devices));
        }

        /// <inheritdoc />
        public async Task<bool> Dispatch(ClassCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var incomingError = command.Error.IsError;

            var device = _devices.GetDevice(command.ClassName, command.IClass.deviceId);
            if (device == null)
            {
                SourceTrace.TraceWarning(
                    G2STrace.Source,
                    $"CommandDispatcher.Dispatch : Received Invalid device identifier CommandId : {command.CommandId} RequestType : {command.GetType()}");

                command.Error.SetErrorCode(ErrorCode.G2S_APX003); // Invalid device identifier
                return !incomingError;
            }

            if (!_handlers.IsClassSupported(command))
            {
                SourceTrace.TraceWarning(
                    G2STrace.Source,
                    $"CommandDispatcher.DispatchCommand : Received Unsupported Class CommandId : {command.CommandId} ExtendType : {command.ClassName}");

                command.Error.SetErrorCode(ErrorCode.G2S_APX007); // Class Not Supported
                return !incomingError;
            }

            // Without modifying the underlying ClassCommand<,> we can't do any casting, so we have to assume the handlers are in good shape
            var handler = _handlers.GetHandler(command) as dynamic;
            if (handler == null)
            {
                SourceTrace.TraceWarning(
                    G2STrace.Source,
                    $"CommandDispatcher.Dispatch : Received Unsupported Command CommandId : {command.CommandId} ExtendType : {command.ClassName}");

                command.Error.SetErrorCode(ErrorCode.G2S_APX008); // Command Not Supported
                return !incomingError;
            }

            if (!device.IsEnabled() && Attribute.GetCustomAttribute(handler.GetType(), typeof(ProhibitWhenDisabledAttribute), false) != null)
            {
                SourceTrace.TraceWarning(
                    G2STrace.Source,
                    $"CommandDispatcher.Dispatch : Received Unsupported Command CommandId : {command.CommandId} ExtendType : {command.ClassName}");

                command.Error.SetErrorCode(ErrorCode.G2S_APX016); // Device disabled
                return !incomingError;
            }

            try
            {
                // We have to use a dynamic type here to ensure the method is called with the proper type
                var typedCommand = command as dynamic;

                if (await handler.Verify(typedCommand) is Error error && error.IsError)
                {
                    typedCommand.Error.Text = error.Text;
                    typedCommand.Error.SetErrorCode(error.Code);

                    SourceTrace.TraceError(
                        G2STrace.Source,
                        $"CommandDispatcher.DispatchCommand : Verification failed HostId : {command.HostId} CommandType : {command.GetType()} Error : {error.Code} Message : {error.Text}");
                }
                else
                {
                    await handler.Handle(typedCommand);
                }
            }
            catch (Exception e)
            {
                SourceTrace.TraceError(
                    G2STrace.Source,
                    $"CommandDispatcher.DispatchCommand : Error handling message HostId : {command.HostId} CommandType : {command.GetType()} Exception : {e}");

                command.Error.SetErrorCode(ErrorCode.G2S_APX999);
                command.Error.Text = Resources.UnexpectedError;
            }

            return !incomingError;
        }
    }
}
