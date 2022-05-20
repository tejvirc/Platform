namespace Aristocrat.Monaco.Test.Common
{
    using System;
    using System.Linq;
    using Moq;

    /// <summary>
    ///     Test helper methods.
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        ///     Creates the mock for the generic type from Type object.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Mock object for type.</returns>
        public static object CreateMockForType(Type type)
        {
            var mock = typeof(Mock<>).MakeGenericType(type).GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
            var result = mock.GetType().GetProperties().Single(f => f.Name == "Object" && f.PropertyType == type)
                .GetValue(mock, new object[] { });
            return result;
        }
    }
}