namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using ServerApiGateway;

    public static class ResponseTypeExtensions
    {
        public static ResponseCode ToResponseCode(this RegistrationResponse.Types.ResultType result)
        {
            return result switch
            {
                RegistrationResponse.Types.ResultType.Accepted => ResponseCode.Ok,
                RegistrationResponse.Types.ResultType.Rejected => ResponseCode.Rejected,
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, null),
            };
        }
    }
}