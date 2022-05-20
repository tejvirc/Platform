namespace Aristocrat.Monaco.G2S.Tests.Extensions
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using Application.Contracts.Authentication;
    using Common.GAT.Storage;
    using G2S.Handlers.Gat;
    using Kernel.Contracts.Components;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Component = Kernel.Contracts.Components.Component;

    [TestClass]
    public class GatEnumExtensionsTest
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidEnumArgumentException))]
        public void WhenToG2SVerifyStateEnumIsNotDefinedExpectException()
        {
            var componenVerificationState = (ComponentVerificationState)10000;
            componenVerificationState.ToG2SVerifyState();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenToG2SVerifyStateEnumIsNotInCaseExpectException()
        {
            var componenVerificationState = ComponentVerificationState.None;
            componenVerificationState.ToG2SVerifyState();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEnumArgumentException))]
        public void WhenToG2SFunctionTypeEnumIsNotDefinedExpectException()
        {
            var functionType = (FunctionType)10000;
            functionType.ToG2SFunctionType();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenToG2SFunctionTypeEnumIsNotInCaseExpectException()
        {
            var functionType = FunctionType.None;
            functionType.ToG2SFunctionType();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenToAlgorithmTypeEnumIsNotDefinedExpectException()
        {
            var algorithmType = (string)null;
            algorithmType.ToAlgorithmType();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenToAlgorithmTypeEnumIsNotInCaseExpectException()
        {
            var algorithmType = "Not cased";
            algorithmType.ToAlgorithmType();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEnumArgumentException))]
        public void WhenToG2SComponentTypeEnumIsNotDefinedExpectException()
        {
            var componentVerificationState = (ComponentType)10000;
            componentVerificationState.ToG2SComponentType();
        }

        [TestMethod]
        public void WhenToG2SComponentTypeComponentTypeIsNotOSExpectSuccess()
        {
            foreach (var componentType in Enum.GetValues(typeof(ComponentType)))
            {
                var componentTypeEnum = (ComponentType)componentType;

                if (componentTypeEnum == ComponentType.OS)
                {
                    continue;
                }

                var converted = componentTypeEnum.ToG2SComponentType();

                Assert.AreEqual($"G2S_{componentTypeEnum.ToString().ToLower(CultureInfo.InvariantCulture)}", converted);
            }
        }

        [TestMethod]
        public void WhenToG2SComponentTypeComponentTypeIsOSExpectSuccess()
        {
            var componentVerificationState = ComponentType.OS;
            var converted = componentVerificationState.ToG2SComponentType();

            Assert.AreEqual($"G2S_{ComponentType.Module.ToString().ToLowerInvariant()}", converted);
        }

        [TestMethod]
        public void ToG2SVerifyStateTest()
        {
            ExtensionsTestHelper.AssertEnumToG2SData(
                GatEnumExtensions.ToG2SVerifyState,
                new[] { ComponentVerificationState.None });
        }

        [TestMethod]
        public void ToG2SFunctionTypeTest()
        {
            ExtensionsTestHelper.AssertEnumToG2SData(GatEnumExtensions.ToG2SFunctionType, new[] { FunctionType.None });
        }

        [TestMethod]
        public void G2SStringToAlgorithmTypeTest()
        {
            var notCasedEnums = 5;

            var strings = new[] { "G2S_MD5", "G2S_SHA1", "G2S_SHA256", "G2S_SHA384", "G2S_SHA512", "G2S_HMACSHA1" };

            ExtensionsTestHelper.AssertG2SStringsToEnum(GatEnumExtensions.ToAlgorithmType, strings);

            Assert.AreEqual(strings.Length, Enum.GetNames(typeof(AlgorithmType)).Length - notCasedEnums);
        }

        [TestMethod]
        public void AlgorithmTypeToAlgorithmTypeG2SStringTest()
        {
            ExtensionsTestHelper.AssertEnumToG2SData<AlgorithmType, string>(GatEnumExtensions.ToG2SAlgorithmType);
        }
    }
}
