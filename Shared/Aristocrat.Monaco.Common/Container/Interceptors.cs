namespace Aristocrat.Monaco.Common.Container
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    //TODO: Implement it
    //using System.Runtime.Remoting.Messaging;  //It has been comment temporary
    //using System.Runtime.Remoting.Proxies;    //It has been comment temporary
    using SimpleInjector;

    /// <summary>
    ///     Provides a mechanism to intercept an invocation
    /// </summary>
    public interface IInterceptor
    {
        /// <summary>
        /// </summary>
        /// <param name="invocation"></param>
        void Intercept(IInvocation invocation);
    }

    /// <summary>
    ///     Provides and container for intercepted target
    /// </summary>
    public interface IInvocation
    {
        /// <summary>
        ///     Gets the target
        /// </summary>
        object InvocationTarget { get; }

        /// <summary>
        ///     Gets the return value for the intercepted method
        /// </summary>
        object ReturnValue { get; set; }

        /// <summary>
        ///     Gets the arguments of the intercepted method
        /// </summary>
        object[] Arguments { get; }

        /// <summary>
        ///     Invokes the intercepted method
        /// </summary>
        void Proceed();

        /// <summary>
        ///     Gets the underlying method
        /// </summary>
        /// <returns></returns>
        MethodBase GetConcreteMethod();
    }

    /// <summary>
    ///     Extension methods for interceptor registration
    /// </summary>
    /// <remarks>
    ///     These extension methods can only intercept interfaces, not abstract types.
    /// </remarks>
    public static class InterceptorExtensions
    {
        /// <summary>
        ///     Container extension used to intercept matching services
        /// </summary>
        /// <typeparam name="TInterceptor">The interceptor type</typeparam>
        /// <param name="this">The container instance</param>
        /// <param name="predicate">The predicate used to determine if a given type should be intercepted</param>
        public static void InterceptWith<TInterceptor>(
            this Container @this,
            Func<Type, bool> predicate)
            where TInterceptor : class, IInterceptor
        {
            @this.Options.ConstructorResolutionBehavior.GetConstructor(typeof(TInterceptor));

            var interceptWith = new InterceptionHelper
            {
                BuildInterceptorExpression =
                    e => BuildInterceptorExpression<TInterceptor>(@this),
                Predicate = predicate
            };

            @this.ExpressionBuilt += interceptWith.OnExpressionBuilt;
        }

        /// <summary>
        ///     Container extension used to intercept matching services
        /// </summary>
        /// <param name="this">The container instance</param>
        /// <param name="interceptorCreator">Callback used to create the interceptor</param>
        /// <param name="predicate">The predicate used to determine if a given type should be intercepted</param>
        public static void InterceptWith(
            this Container @this,
            Func<IInterceptor> interceptorCreator,
            Func<Type, bool> predicate)
        {
            var interceptWith = new InterceptionHelper
            {
                BuildInterceptorExpression =
                    e => Expression.Invoke(Expression.Constant(interceptorCreator)),
                Predicate = predicate
            };

            @this.ExpressionBuilt += interceptWith.OnExpressionBuilt;
        }

        /// <summary>
        ///     Container extension used to intercept matching services
        /// </summary>
        /// <param name="this">The container instance</param>
        /// <param name="interceptorCreator">Callback used to create the interceptor</param>
        /// <param name="predicate">The predicate used to determine if a given type should be intercepted</param>
        public static void InterceptWith(
            this Container @this,
            Func<ExpressionBuiltEventArgs, IInterceptor> interceptorCreator,
            Func<Type, bool> predicate)
        {
            var interceptWith = new InterceptionHelper
            {
                BuildInterceptorExpression = e => Expression.Invoke(
                    Expression.Constant(interceptorCreator),
                    Expression.Constant(e)),
                Predicate = predicate
            };

            @this.ExpressionBuilt += interceptWith.OnExpressionBuilt;
        }

        /// <summary>
        ///     Container extension used to intercept matching services
        /// </summary>
        /// <param name="this">The container instance</param>
        /// <param name="interceptor">The configured interceptor</param>
        /// <param name="predicate">The predicate used to determine if a given type should be intercepted</param>
        public static void InterceptWith(
            this Container @this,
            IInterceptor interceptor,
            Func<Type, bool> predicate)
        {
            var interceptWith = new InterceptionHelper
            {
                BuildInterceptorExpression = e => Expression.Constant(interceptor),
                Predicate = predicate
            };

            @this.ExpressionBuilt += interceptWith.OnExpressionBuilt;
        }

        [DebuggerStepThrough]
        private static Expression BuildInterceptorExpression<TInterceptor>(
            Container container)
            where TInterceptor : class
        {
            var interceptorRegistration = container.GetRegistration(typeof(TInterceptor));

            if (interceptorRegistration == null)
            {
                // This will throw an ActivationException
                container.GetInstance<TInterceptor>();
            }

            return interceptorRegistration?.BuildExpression();
        }

        private class InterceptionHelper
        {
            private static readonly MethodInfo NonGenericInterceptorCreateProxyMethod = (
                    from method in typeof(Interceptor).GetMethods()
                    where method.Name == "CreateProxy"
                    where method.GetParameters().Length == 3
                    select method)
                .Single();

            internal Func<ExpressionBuiltEventArgs, Expression> BuildInterceptorExpression;
            internal Func<Type, bool> Predicate;

            [DebuggerStepThrough]
            public void OnExpressionBuilt(object sender, ExpressionBuiltEventArgs e)
            {
                if (Predicate(e.RegisteredServiceType))
                {
                    ThrowIfServiceTypeNotInterface(e);
                    e.Expression = BuildProxyExpression(e);
                }
            }

            [DebuggerStepThrough]
            private static void ThrowIfServiceTypeNotInterface(ExpressionBuiltEventArgs e)
            {
                // NOTE: We can only handle interfaces, because
                // System.Runtime.Remoting.Proxies.RealProxy only supports interfaces.
                if (!e.RegisteredServiceType.IsInterface)
                {
                    throw new NotSupportedException("Can't intercept type " + e.RegisteredServiceType.Name + " because it is not an interface.");
                }
            }

            [DebuggerStepThrough]
            private Expression BuildProxyExpression(ExpressionBuiltEventArgs e)
            {
                var expr = BuildInterceptorExpression(e);

                // Create call to
                // (ServiceType)Interceptor.CreateProxy(Type, IInterceptor, object)
                var proxyExpression =
                    Expression.Convert(
                        Expression.Call(
                            NonGenericInterceptorCreateProxyMethod,
                            Expression.Constant(e.RegisteredServiceType, typeof(Type)),
                            expr,
                            e.Expression),
                        e.RegisteredServiceType);

                if (e.Expression is ConstantExpression && expr is ConstantExpression)
                {
                    return Expression.Constant(CreateInstance(proxyExpression), e.RegisteredServiceType);
                }

                return proxyExpression;
            }

            [DebuggerStepThrough]
            private static object CreateInstance(Expression expression)
            {
                var instanceCreator = Expression.Lambda<Func<object>>(expression)
                    .Compile();

                return instanceCreator();
            }
        }
    }

    /// <summary>
    /// </summary>
    public static class Interceptor
    {
        /// <summary>
        ///     Creates a proxy used to invoke the intercepted type
        /// </summary>
        /// <typeparam name="T">The underlying type</typeparam>
        /// <param name="interceptor">The interceptor</param>
        /// <param name="realInstance">The intercepted instance</param>
        /// <returns>The typed proxy</returns>
        public static T CreateProxy<T>(IInterceptor interceptor, T realInstance)
        {
            return (T)CreateProxy(typeof(T), interceptor, realInstance);
        }

        /// <summary>
        ///     Creates a proxy used to invoke the intercepted type
        /// </summary>
        /// <param name="serviceType">The underlying type</param>
        /// <param name="interceptor">The interceptor</param>
        /// <param name="realInstance">The intercepted instance</param>
        /// <returns>The proxy instance</returns>
        [DebuggerStepThrough]
        public static object CreateProxy(
            Type serviceType,
            IInterceptor interceptor,
            object realInstance)
        {
            var proxy = new InterceptorProxy(serviceType, realInstance, interceptor);
            //return proxy.GetTransparentProxy();       //Disabled temporary
            // TODO: Return Transparent proxy
            return proxy;
        }

        //TODO: Disabled temporary
        private sealed class InterceptorProxy //: RealProxy
        {
            private static readonly MethodBase GetTypeMethod = typeof(object).GetMethod("GetType");

            private readonly IInterceptor _interceptor;
            private readonly object _realInstance;

            [DebuggerStepThrough]
            public InterceptorProxy(Type classToProxy, object obj, IInterceptor interceptor)
                //TODO: Disabled temporary
                //: base(classToProxy)  
            {
                _realInstance = obj;
                _interceptor = interceptor;
            }

            //TODO: Disabled temporary
            /*
            public override IMessage Invoke(IMessage msg)
            {
                if (msg is IMethodCallMessage message)
                {
                    return ReferenceEquals(message.MethodBase, GetTypeMethod)
                        ? Bypass(message)
                        : InvokeMethodCall(message);
                }

                return msg;
            }

            private IMessage InvokeMethodCall(IMethodCallMessage msg)
            {
                var i = new Invocation { Proxy = this, Message = msg, Arguments = msg.Args };

                i.Proceeding = () =>
                    i.ReturnValue = msg.MethodBase.Invoke(_realInstance, i.Arguments);
                _interceptor.Intercept(i);

                return new ReturnMessage(
                    i.ReturnValue,
                    i.Arguments,
                    i.Arguments.Length,
                    null,
                    msg);
            }

            private IMessage Bypass(IMethodCallMessage msg)
            {
                var value = msg.MethodBase.Invoke(_realInstance, msg.Args);
                return new ReturnMessage(value, msg.Args, msg.Args.Length, null, msg);
            }
            */

            private class Invocation : IInvocation
            {
                //TODO: Disabled temporary
                //public Action Proceeding;

                //TODO: Redefine it as a simple public variable instead of property as above commented line
                public Action Proceeding { get; set; }

                public InterceptorProxy Proxy { get; set; }

                //TODO: Disabled temporary
                //public IMethodCallMessage Message { get; set; }
                public object[] Arguments { get; set; }
                public object ReturnValue { get; set; }
                public object InvocationTarget => Proxy._realInstance;

                public void Proceed()
                {
                    Proceeding();
                }

                public MethodBase GetConcreteMethod()
                {
                    //TODO: Disabled temporary
                    //return Message.MethodBase;

                    return GetTypeMethod;
                }
            }
        }
    }
}