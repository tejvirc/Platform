﻿namespace Aristocrat.Bingo.Client.Messages.Interceptor
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Grpc.Core;
    using Grpc.Core.Interceptors;
    using log4net;

    public class LoggingInterceptor : Interceptor
    {
        protected static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        public EventHandler<EventArgs> MessageReceived { get; set; }

        public void OnMessageReceived()
        {
            MessageReceived?.Invoke(this, EventArgs.Empty);
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            Logger.Debug($"Sending Request: {request}");
            var call = continuation(request, context);
            return new AsyncUnaryCall<TResponse>(
                LogResponse(call.ResponseAsync),
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
            Logger.Debug($"Sending Request {request}");
            var response = base.BlockingUnaryCall(request, context, continuation);
            Logger.Debug($"Response Received: {response}");
            return response;
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            var call = continuation(context);
            return new AsyncDuplexStreamingCall<TRequest, TResponse>(
                new ClientStreamingLogger<TRequest>(call.RequestStream),
                new ServerStreamingLogger<TResponse>(call.ResponseStream, OnMessageReceived),
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
                new ServerStreamingLogger<TResponse>(call.ResponseStream, OnMessageReceived),
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
                new ClientStreamingLogger<TRequest>(call.RequestStream),
                LogResponse(call.ResponseAsync),
                call.ResponseHeadersAsync,
                call.GetStatus,
                call.GetTrailers,
                call.Dispose);
        }

        protected async Task<TResponse> LogResponse<TResponse>(Task<TResponse> callingTask)
        {
            try
            {
                var response = await callingTask;
                Logger.Debug($"Response Received: {response}");
                OnMessageReceived();
                return response;
            }
            catch (Exception ex)
            {
                Logger.Error("An exception occurred trying to get a response", ex);
                throw;
            }
        }
    }
}