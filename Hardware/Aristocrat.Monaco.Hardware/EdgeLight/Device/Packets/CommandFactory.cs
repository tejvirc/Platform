namespace Aristocrat.Monaco.Hardware.EdgeLight.Device.Packets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal static class CommandFactory
    {
        private static IDictionary<int, Func<IResponse>> Responses { get; } = GetTypes<IResponse>();

        private static IDictionary<int, Func<IRequest>> Requests { get; } = GetTypes<IRequest>();

        private static IDictionary<int, Func<IResponseHandler>> Handlers { get; } = GetTypes<IResponseHandler>();

        private static IDictionary<int, Func<T>> GetTypes<T>()
        {
            return typeof(T).Assembly
                .GetTypes()
                .Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(
                    t => new
                    {
                        Keys = t.GetCustomAttributes(typeof(AcceptsAttribute), true)
                            .Cast<AcceptsAttribute>().SelectMany(attr => attr.Types),
                        Value = (Func<T>)Expression.Lambda(
                                Expression.Convert(Expression.New(t), typeof(T)))
                            .Compile()
                    })
                .SelectMany(o => o.Keys.Select(key => new { Key = key, o.Value }))
                .ToDictionary(o => o.Key, v => v.Value);
        }

        public static IResponse CreateResponse(byte[] packetBytes)
        {
            if (!packetBytes.Any() || !Responses.TryGetValue(packetBytes.First(), out var func))
            {
                return null;
            }

            var response = func();
            response.Wrap(packetBytes);
            return response;
        }

        public static T CreateRequest<T>(RequestType requestType) where T : class, IRequest
        {
            return CreateRequest(requestType) as T;
        }

        public static IRequest CreateRequest(RequestType requestType)
        {
            if (!Requests.TryGetValue((int)requestType, out var func))
            {
                return null;
            }

            var request = func();
            request.Type = (byte)requestType;
            return request;
        }

        public static IResponseHandler CreateHandler(IResponse response)
        {
            if (response is null || !Handlers.TryGetValue(response.Type, out var func))
            {
                return null;
            }

            var handler = func();
            return handler;
        }
    }
}