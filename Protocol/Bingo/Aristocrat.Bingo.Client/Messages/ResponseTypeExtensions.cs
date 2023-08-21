namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using ServerApiGateway;

    public static class ResponseTypeExtensions
    {
        public static ResponseCode ToResponseCode(this RegistrationResponse.Types.ResultType result, string token)
        {
            return result switch
            {
                RegistrationResponse.Types.ResultType.Accepted when !string.IsNullOrEmpty(token) => ResponseCode.Ok,
                RegistrationResponse.Types.ResultType.Rejected => ResponseCode.Rejected,
                _ => ResponseCode.Rejected
            };
        }
    }
}