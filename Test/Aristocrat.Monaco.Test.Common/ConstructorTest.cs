namespace Aristocrat.Monaco.Test.Common
{
    using System;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     Test helper methods for constructor tests.
    /// </summary>
    public static class ConstructorTest
    {
        /// <summary>
        ///     Tests the constructor null checks for the specified type.
        /// </summary>
        /// <typeparam name="T">Type to verify.</typeparam>
        public static void TestConstructorNullChecks<T>()
            where T : class
        {
            var constructors = typeof(T).GetConstructors();
            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                foreach (var parameter in parameters)
                {
                    TestParameterNull<T>(parameters, parameter, constructor);
                }
            }
        }

        private static void TestParameterNull<T>(
            ParameterInfo[] parameters,
            ParameterInfo parameter,
            ConstructorInfo constructor) where T : class
        {
            var gotException = false;
            try
            {
                var objects = new object[parameters.Length];
                for (var i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i] == parameter)
                    {
                        objects[i] = null;
                    }
                    else
                    {
                        var mock = TestHelper.CreateMockForType(parameters[i].ParameterType);
                        objects[i] = mock;
                    }
                }

                constructor.Invoke(objects);
            }
            catch (TargetInvocationException exc)
            {
                if (exc.InnerException is ArgumentNullException)
                {
                    gotException = true;
                }
            }

            if (!gotException)
            {
                Assert.Fail($"Constructor for {typeof(T).FullName} accepts null parameter {parameter.Name}");
            }
        }
    }
}