namespace Aristocrat.Bingo.Client.Messages.Interceptor
{
    using Grpc.Core;
    using Grpc.Core.Interceptors;

    public class ProgressiveClientInterceptor : BaseClientInterceptor
    {
        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            var call = continuation(context);
            return new AsyncDuplexStreamingCall<TRequest, TResponse>(
                new ProgressiveClientClientStreamingLogger<TRequest>(call.RequestStream),
                new ProgressiveClientServerStreamingLogger<TResponse>(call.ResponseStream, this),
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
            var call = continuation(request, context);
            return new AsyncServerStreamingCall<TResponse>(
                new ProgressiveClientServerStreamingLogger<TResponse>(call.ResponseStream, this),
                call.ResponseHeadersAsync,
                call.GetStatus,
                call.GetTrailers,
                call.Dispose);
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            var call = continuation(context);
            return new AsyncClientStreamingCall<TRequest, TResponse>(
                new ProgressiveClientClientStreamingLogger<TRequest>(call.RequestStream),
                LogResponse(call.ResponseAsync),
                call.ResponseHeadersAsync,
                call.GetStatus,
                call.GetTrailers,
                call.Dispose);
        }
    }
}