namespace Aristocrat.Bingo.Client.Messages.Interceptor
{
    using System;

    public class BingoClientAuthorizationInterceptor : BaseClientAuthorizationInterceptor
    {
        public BingoClientAuthorizationInterceptor(IBingoAuthorizationProvider authorizationProvider)
        {
            AuthorizationProvider = authorizationProvider ?? throw new ArgumentNullException(nameof(authorizationProvider));
        }
    }
}