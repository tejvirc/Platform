namespace Aristocrat.Bingo.Client.Messages.Interceptor
{
    using System.Reflection;
    using Grpc.Core;
    using Grpc.Core.Interceptors;
    using log4net;

    public class BingoClientInterceptor : BaseClientInterceptor
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        public BingoClientInterceptor(IAuthorizationProvider authorizationProvider)
            : base(authorizationProvider)
        {
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            context = AddAuthorization(context);
            var call = continuation(context);
            return new AsyncDuplexStreamingCall<TRequest, TResponse>(
                new BingoClientClientStreamingLogger<TRequest>(call.RequestStream),
                new BingoClientServerStreamingLogger<TResponse>(call.ResponseStream, this),
                call.ResponseHeadersAsync,
                call.GetStatus,
                call.GetTrailers,
                call.Dispose);
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            Logger.Debug($"Sending Request {request}");
            context = AddAuthorization(context);
            var call = continuation(request, context);
            return new AsyncServerStreamingCall<TResponse>(
                new BingoClientServerStreamingLogger<TResponse>(call.ResponseStream, this),
                call.ResponseHeadersAsync,
                call.GetStatus,
                call.GetTrailers,
                call.Dispose);
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            context = AddAuthorization(context);
            var call = continuation(context);
            return new AsyncClientStreamingCall<TRequest, TResponse>(
                new BingoClientClientStreamingLogger<TRequest>(call.RequestStream),
                LogResponse(call.ResponseAsync),
                call.ResponseHeadersAsync,
                call.GetStatus,
                call.GetTrailers,
                call.Dispose);
        }
    }
}