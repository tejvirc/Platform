namespace Aristocrat.Bingo.Client.Messages.Interceptor
{
    using System;
    using Grpc.Core;
    using Grpc.Core.Interceptors;

    public abstract class BaseClientAuthorizationInterceptor : Interceptor
    {
        protected IAuthorizationProvider AuthorizationProvider;

        public int MessageTimeoutMs { get; set; } = 30000;

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            context = AddTimeout(AddAuthorization(context));
            var call = continuation(request, context);
            return new AsyncUnaryCall<TResponse>(
                call.ResponseAsync,
                call.ResponseHeadersAsync,
                call.GetStatus,
                call.GetTrailers,
                call.Dispose);
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            context = AddTimeout(AddAuthorization(context));
            var response = base.BlockingUnaryCall(request, context, continuation);
            return response;
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            context = AddAuthorization(context);
            var call = continuation(context);
            return new AsyncDuplexStreamingCall<TRequest, TResponse>(
                call.RequestStream,
                call.ResponseStream,
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
            context = AddAuthorization(context);
            var call = continuation(request, context);
            return new AsyncServerStreamingCall<TResponse>(
                call.ResponseStream,
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
                call.RequestStream,
                call.ResponseAsync,
                call.ResponseHeadersAsync,
                call.GetStatus,
                call.GetTrailers,
                call.Dispose);
        }

        protected ClientInterceptorContext<TRequest, TResponse> AddAuthorization<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context)
            where TRequest : class
            where TResponse : class
        {
            var metadata = AuthorizationProvider.AuthorizationData;
            if (metadata is null)
            {
                return context;
            }

            return new ClientInterceptorContext<TRequest, TResponse>(
                context.Method,
                context.Host,
                context.Options.WithHeaders(metadata));
        }

        protected ClientInterceptorContext<TRequest, TResponse> AddTimeout<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context)
            where TRequest : class
            where TResponse : class
        {
            return new (
                context.Method,
                context.Host,
                context.Options.WithDeadline(DateTime.UtcNow.AddMilliseconds(MessageTimeoutMs)));
        }
    }
}