namespace Aristocrat.Monaco.Application.Contracts.Tests.Metering
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     Tests for MeterClassification
    /// </summary>
    [TestClass]
    public class MeterClassificationTest
    {
        private MeterClassification _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new TestMeterClassification();
        }

        [TestMethod]
        public void UpperBoundsTest()
        {
            long expected = long.MaxValue;
            Assert.AreEqual(expected, _target.UpperBounds);
        }

        [TestMethod]
        public void NameTest()
        {
            string expected = "TestClassification";
            Assert.AreEqual(expected, _target.Name);
        }

        [TestMethod]
        public void MeterClassificationConstructorTest()
        {
            Assert.AreEqual("TestClassification", _target.Name);
            Assert.AreEqual(long.MaxValue, _target.UpperBounds);
        }

        [TestMethod]
        public void EqualsTestForNullObject()
        {
            Assert.IsFalse(_target.Equals((object)null));
            Assert.AreEqual("TestClassification", _target.Name);
            Assert.AreEqual(long.MaxValue, _target.UpperBounds);
        }

        [TestMethod]
        public void EqualsTestForObjectOfDifferentType()
        {
            Assert.AreEqual("TestClassification", _target.Name);
            Assert.AreEqual(long.MaxValue, _target.UpperBounds);
        }

        [TestMethod]
        public void EqualsTestForObjectOfSameType()
        {
            MeterClassification other = new TestMeterClassification();
            Assert.IsTrue(_target.Equals((object)other));
            Assert.AreEqual("TestClassification", _target.Name);
            Assert.AreEqual(long.MaxValue, _target.UpperBounds);
        }

        [TestMethod]
        public void EqualsTestForMeterClassification()
        {
            MeterClassification other = new TestMeterClassification();
            Assert.IsTrue(_target.Equals(other));
            Assert.AreEqual("TestClassification", _target.Name);
            Assert.AreEqual(long.MaxValue, _target.UpperBounds);
        }

        [TestMethod]
        public void EqualsTestForEqualityOperator()
        {
            MeterClassification other = new TestMeterClassification();
            Assert.IsTrue(_target == other);
            Assert.AreEqual("TestClassification", _target.Name);
            Assert.AreEqual(long.MaxValue, _target.UpperBounds);
        }

        [TestMethod]
        public void EqualsTestForInequalityOperator()
        {
            MeterClassification other = new TestMeterClassification();
            Assert.IsFalse(_target != other);
            Assert.AreEqual("TestClassification", _target.Name);
            Assert.AreEqual(long.MaxValue, _target.UpperBounds);
        }

        [TestMethod]
        public void HashCodeTest()
        {
            Assert.AreEqual("TestClassification".GetHashCode(), _target.GetHashCode());
        }

        [TestMethod]
        public void EqualsTestWithNull()
        {
            MeterClassification other = null;
            bool testNull = other == null;
            Assert.IsTrue(testNull);
        }
    }
}