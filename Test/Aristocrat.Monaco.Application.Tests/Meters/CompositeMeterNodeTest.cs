namespace Aristocrat.Monaco.Application.Tests.Meters
{
    #region Using

    using System.Collections.Generic;
    using Application.Meters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Test.Common;

    #endregion

    /// <summary>
    ///     Contains the tests for the CompositeMeterNode class
    /// </summary>
    [TestClass]
    public class CompositeMeterNodeTest
    {
        private dynamic _accessor;

        private CompositeMeterNode _target;

        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new CompositeMeterNode();
            _accessor = new DynamicPrivateObject(_target);
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.IsNull(_target.Name);
            string name = "Test";
            _accessor._name = name;
            Assert.AreEqual(name, _target.Name);
        }

        [TestMethod]
        public void ClassificationTest()
        {
            Assert.IsNull(_target.Classification);
            string classification = "Test";
            _accessor._classification = classification;
            Assert.AreEqual(classification, _target.Classification);
        }

        [TestMethod]
        public void ExpressionTest()
        {
            Assert.IsNull(_target.Expression);
            string expression = "Test";
            _accessor._expression = expression;
            Assert.AreEqual(expression, _target.Expression);
        }

        [TestMethod]
        public void ExpressionMetersTest()
        {
            Assert.IsNull(_target.ExpressionMeters);
            List<string> meters = new List<string> { "Test1", "Test2" };
            _accessor._expressionMeters = meters;
            Assert.AreEqual(meters, _target.ExpressionMeters);
        }

        [TestMethod]
        public void InitializeTest()
        {
            // test happy path
            string expression = "Meter1 + Meter2";
            _accessor._expression = expression;

            _target.Initialize();
            Assert.AreEqual(2, _target.ExpressionMeters.Count);
            Assert.AreEqual("Meter1", _target.ExpressionMeters[0]);
            Assert.AreEqual("Meter2", _target.ExpressionMeters[1]);

            _accessor._expressionMeters.Clear();

            // test empty string
            expression = "Meter1 + + Meter2";
            _accessor._expression = expression;

            _target.Initialize();
            Assert.AreEqual(2, _target.ExpressionMeters.Count);
            Assert.AreEqual("Meter1", _target.ExpressionMeters[0]);
            Assert.AreEqual("Meter2", _target.ExpressionMeters[1]);

            _accessor._expressionMeters.Clear();

            // test constant in string
            expression = "Meter1 + 2 + Meter2";
            _accessor._expression = expression;

            _target.Initialize();
            Assert.AreEqual(2, _target.ExpressionMeters.Count);
            Assert.AreEqual("Meter1", _target.ExpressionMeters[0]);
            Assert.AreEqual("Meter2", _target.ExpressionMeters[1]);

            _accessor._expressionMeters.Clear();

            // test meter already in list
            expression = "Meter1 + Meter2 - Meter1";
            _accessor._expression = expression;

            _target.Initialize();
            Assert.AreEqual(2, _target.ExpressionMeters.Count);
            Assert.AreEqual("Meter1", _target.ExpressionMeters[0]);
            Assert.AreEqual("Meter2", _target.ExpressionMeters[1]);

            _accessor._expressionMeters.Clear();
        }
    }
}