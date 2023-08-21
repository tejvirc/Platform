namespace Aristocrat.G2S.Emdi.Extensions
{
    using System;
    using System.Collections.Generic;
    using Host;
    using Protocol.v21ext1b1;

    /// <summary>
    ///     Extension methods for <see cref="mdMsg"/>
    /// </summary>
    internal static class MessageExtensions
    {
        /// <summary>
        ///     Creates a response for request message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static mdMsg CreateResponse(this mdMsg message)
        {
            var response = new mdMsg();

            var functionalGroup = response.NewFunctionalGroup(message.GetFunctionalGroup().GetType(), t_cmdType.response);
            functionalGroup.cmdType = t_cmdType.response;
            response.SetSessionId(message.GetSessionId());

            return response;
        }

        /// <summary>
        ///     Creates a response for request message
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="TCommand"></typeparam>
        /// <returns></returns>
        public static mdMsg CreateResponse<TCommand>(this mdMsg message)
            where TCommand : c_baseCommand, new()
        {
            var response = new mdMsg();

            response.NewFunctionalGroup(message.GetFunctionalGroup().GetType(), t_cmdType.response);
            response.NewCommand<TCommand>();
            response.SetSessionId(message.GetSessionId());

            return response;
        }

        /// <summary>
        ///     Creates a response for request message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="command"></param>
        /// <typeparam name="TCommand"></typeparam>
        /// <returns></returns>
        public static mdMsg CreateResponse<TCommand>(this mdMsg message, TCommand command)
            where TCommand : c_baseCommand
        {
            var response = new mdMsg();

            response.NewFunctionalGroup(message.GetFunctionalGroup().GetType(), t_cmdType.response);
            response.SetCommand(command);
            response.SetSessionId(message.GetSessionId());

            return response;
        }

        /// <summary>
        ///     Creates an error response for request message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public static mdMsg CreateErrorResponse(this mdMsg message, EmdiErrorCode errorCode)
        {
            var response = new mdMsg();

            var functionalGroup = response.NewFunctionalGroup(message.GetFunctionalGroup().GetType(), t_cmdType.response);
            functionalGroup.errorCode = (long)errorCode;
            response.SetSessionId(message.GetSessionId());

            return response;
        }

        /// <summary>
        ///     Gets the functional group for the message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static c_baseClass GetFunctionalGroup(this mdMsg message)
        {
            var functionalGroup = message.Item as c_baseClass;

            if (functionalGroup == null)
            {
                throw new InvalidOperationException("Functional group not found");
            }

            return functionalGroup;
        }

        /// <summary>
        ///     Creates a new functional group for the message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cmdType"></param>
        /// <typeparam name="TGroup"></typeparam>
        /// <returns></returns>
        public static TGroup NewFunctionalGroup<TGroup>(this mdMsg message, t_cmdType cmdType = t_cmdType.request)
            where TGroup : c_baseClass, new()
        {
            var functionalGroup = new TGroup { cmdType = cmdType };

            message.Item = functionalGroup;

            return functionalGroup;
        }

        /// <summary>
        ///     Creates a new functional group for the message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="groupType"></param>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static c_baseClass NewFunctionalGroup(this mdMsg message, Type groupType, t_cmdType cmdType = t_cmdType.request)
        {
            var functionalGroup  = Activator.CreateInstance(groupType) as c_baseClass;

            if (functionalGroup == null)
            {
                throw new InvalidOperationException($"Could not create functional group for type {groupType.FullName}.");
            }

            functionalGroup.cmdType = t_cmdType.response;
            message.Item = functionalGroup;

            return functionalGroup;
        }

        /// <summary>
        ///     Determines if the message has a functional group
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool HasFunctionalGroup(this mdMsg message)
        {
            return message.Item is c_baseClass;
        }

        /// <summary>
        ///     Gets the <see cref="t_cmdType"/> for the message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static t_cmdType GetCommandType(this mdMsg message)
        {
            var functionalGroup = message.GetFunctionalGroup();

            if (functionalGroup == null)
            {
                throw new InvalidOperationException("Functional group not found");
            }

            return functionalGroup.cmdType;
        }

        /// <summary>
        ///     Gets the command for the message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static dynamic GetCommand(this mdMsg message)
        {
            var functionalGroup = message.GetFunctionalGroup();

            var command = ((dynamic)functionalGroup).Item as c_baseCommand;

            if (command == null)
            {
                throw new InvalidOperationException("Command not found");
            }

            return command;
        }

        /// <summary>
        ///     Creates a new command for the message
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="TGroup"></typeparam>
        /// <typeparam name="TCommand"></typeparam>
        /// <returns></returns>
        public static TCommand NewCommand<TGroup, TCommand>(this mdMsg message)
            where TGroup : c_baseClass, new()
            where TCommand : c_baseCommand, new()
        {
            message.NewFunctionalGroup<TGroup>();
            var command = message.NewCommand<TCommand>();
            return command;
        }

        /// <summary>
        ///     Creates a new command for the message
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="TCommand"></typeparam>
        /// <returns></returns>
        public static TCommand NewCommand<TCommand>(this mdMsg message)
            where TCommand : c_baseCommand, new()
        {
            var command = new TCommand();
            message.SetCommand(command);
            return command;
        }

        /// <summary>
        ///     Sets the command for the message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="command"></param>
        /// <typeparam name="TCommand"></typeparam>
        /// <exception cref="InvalidOperationException"></exception>
        public static void SetCommand<TCommand>(this mdMsg message, TCommand command)
            where TCommand : c_baseCommand
        {
            var functionalGroup = message.GetFunctionalGroup();

            if (functionalGroup == null)
            {
                throw new InvalidOperationException("Functional group not found");
            }

            var itemProp = functionalGroup.GetType().GetProperty("Item");

            if (itemProp == null)
            {
                throw new InvalidOperationException("Item property not found on functional group class");
            }

            itemProp.SetValue(functionalGroup, command);
        }

        /// <summary>
        ///     Determines if the message has a command
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool HasCommand(this mdMsg message)
        {
            if (!message.HasFunctionalGroup())
            {
                return false;
            }

            var functionalGroup = message.GetFunctionalGroup();

            return ((dynamic)functionalGroup).Item is c_baseCommand;
        }

        /// <summary>
        ///     Gets the Session ID form the message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static long GetSessionId(this mdMsg message)
        {
            return message.GetFunctionalGroup().sessionId;
        }

        /// <summary>
        ///     Sets the Session ID for the message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionId"></param>
        public static void SetSessionId(this mdMsg message, long sessionId)
        {
            message.GetFunctionalGroup().sessionId = sessionId;
        }

        /// <summary>
        ///     Determines if the message is valid
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool IsValid(this mdMsg message)
        {

            if (!message.HasFunctionalGroup())
            {
                return false;
            }

            if (!message.IsRequest() && !message.IsResponse())
            {
                return false;
            }

            if (!message.IsValidErrorCode())
            {
                return false;
            }

            if (!message.IsValidCommand())
            {
                return false;
            }

            return true;
        }

        public static bool IsValidCommand(this mdMsg message)
        {
            return message.IsValidCommand<c_baseCommand>();
        }

        /// <summary>
        ///     Determines if the message command is valid
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool IsValidCommand<TCommand>(this mdMsg message)
            where TCommand : c_baseCommand
        {
            if (!message.HasFunctionalGroup())
            {
                return false;
            }

            var functionalGroup = message.GetFunctionalGroup();

            if (!message.HasCommand())
            {
                return false;
            }

            var command = message.GetCommand() as TCommand;

            if (command == null)
            {
                return false;
            }

            var supportedCommands = SupportedCommands.Get();

            if (!supportedCommands.TryGetValue(functionalGroup.GetType().Name, out List<string> commands) ||
                !commands.Contains(command.GetType().Name))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Determines if the message is a response message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool IsResponse(this mdMsg message)
        {
            return message.HasFunctionalGroup() && message.GetFunctionalGroup()?.cmdType == t_cmdType.response;
        }

        /// <summary>
        ///     Determines if the message is a request message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool IsRequest(this mdMsg message)
        {
            return message.HasFunctionalGroup() && message.GetFunctionalGroup().cmdType == t_cmdType.request;
        }

        /// <summary>
        ///     Gets the error code for the message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static EmdiErrorCode GetErrorCode(this mdMsg message)
        {
            if (!message.IsValidErrorCode())
            {
                return EmdiErrorCode.UnknownError;
            }
            
            return (EmdiErrorCode)message.GetFunctionalGroup().errorCode;
        }

        /// <summary>
        ///     Determines if the message contains an error code
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool IsValidErrorCode(this mdMsg message)
        {
            if (!message.HasFunctionalGroup())
            {
                return false;
            }

            var functionalGroup = message.GetFunctionalGroup();

            var errorCode = functionalGroup.errorCode;

            return Enum.IsDefined(typeof(EmdiErrorCode), (int)errorCode);
        }

        /// <summary>
        ///     Determines if the message contains an error code
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool IsError(this mdMsg message)
        {
            return message.GetFunctionalGroup().errorCode != 0;
        }
    }
}
