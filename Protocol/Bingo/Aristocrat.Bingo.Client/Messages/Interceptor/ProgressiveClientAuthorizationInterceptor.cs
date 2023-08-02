namespace Aristocrat.Bingo.Client.Messages.Interceptor
{
    using System;

    public class ProgressiveClientAuthorizationInterceptor : BaseClientAuthorizationInterceptor
    {
        public ProgressiveClientAuthorizationInterceptor(IProgressiveAuthorizationProvider authorizationProvider)
        {
            AuthorizationProvider = authorizationProvider ?? throw new ArgumentNullException(nameof(authorizationProvider));
        }
    }
}