namespace Aristocrat.Monaco.Test.Common
{
    using System;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class AssertEx
    {
        public static void IsAttributeDefined(MemberInfo element, Type attributeType)
        {
            Assert.IsTrue(Attribute.IsDefined(element, attributeType));
        }
    }
}