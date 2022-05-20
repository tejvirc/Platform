namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.GetGameAssignments"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Protocol.GetGameAssignments"/> instance.
    /// </summary>
    public class GetGameAssignmentsTranslator : MessageTranslator<Messaging.GetGameAssignments>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.GetGameAssignments message)
        {
            return new GetGameAssignments
            {
                InstanceID = new GetGameAssignmentsInstanceID
                {
                    Value = message.InstanceId
                },
            };
        }
    }
}
