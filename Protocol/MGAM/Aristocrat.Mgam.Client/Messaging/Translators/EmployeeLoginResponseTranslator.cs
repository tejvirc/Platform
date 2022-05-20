namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using System;
    using System.Linq;
    using Action;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.EmployeeLoginResponse"/> to a <see cref="T:Aristocrat.Mgam.Client.Messaging.EmployeeLoginResponse"/>.
    /// </summary>
    public class EmployeeLoginResponseTranslator : MessageTranslator<Protocol.EmployeeLoginResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.EmployeeLoginResponse message)
        {
            return new EmployeeLoginResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value,
                CardString = message.CardString.Value,
                EmployeeName = message.EmployeeName.Value,
                EmployeeId = message.EmployeeID.Value,
                Actions = (message.Actions.elem?.Select(
                               action => new ActionInfo(
                                   Guid.Parse(action.ActionGUID.Value), action.ActionName.Value, action.ActionDescription.Value))
                           ?? Enumerable.Empty<ActionInfo>()).ToList()
            };
        }
    }
}
