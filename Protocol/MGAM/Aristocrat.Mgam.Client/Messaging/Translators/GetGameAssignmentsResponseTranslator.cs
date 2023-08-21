namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using System.Linq;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.GetGameAssignmentsResponse"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Messaging.GetGameAssignmentsResponse"/> instance.
    /// </summary>
    public class GetGameAssignmentsResponseTranslator : MessageTranslator<Protocol.GetGameAssignmentsResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.GetGameAssignmentsResponse message)
        {
            return new GetGameAssignmentsResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value,
                Assignments = (message.Assignments.elem?.Select(
                                   x => new GameAssignment
                                   {
                                       UpcNumber = x.GameUPCNumber.Value,
                                       PayTableIndex = x.PayTableIndex.Value,
                                       Denomination = x.Denomination.Value,
                                       Xldf = x.XLDF.Value
                                   })
                               ?? Enumerable.Empty<GameAssignment>()).ToList()
            };
        }
    }
}
