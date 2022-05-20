namespace Aristocrat.Monaco.Application.Contracts.Tests.Metering
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     Tests for MeterSnapshot
    /// </summary>
    [TestClass]
    public class MeterSnapshotTest
    {
        private MeterSnapshot _target;

        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            _target = new MeterSnapshot();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void NameTest()
        {
            _target.Name = "Test";
            Assert.AreEqual("Test", _target.Name);
        }

        [TestMethod]
        public void ValuesTest()
        {
            var values = new Dictionary<MeterValueType, long>
            {
                { MeterValueType.Lifetime, 1 },
                { MeterValueType.Period, 3 }
            };

            _target.Values = values;
            Assert.AreEqual(values, _target.Values);
            Assert.AreEqual(values.Count, _target.Values.Count);
        }

        [TestMethod]
        public void EqualityTest()
        {
            var values = new Dictionary<MeterValueType, long>
            {
                { MeterValueType.Lifetime, 1 },
                { MeterValueType.Period, 3 }
            };

            _target.Values = values;
            _target.Name = "Test";

            var target1 = _target;

            // test for == and !=
            Assert.IsTrue(target1 == _target);
            Assert.IsFalse(target1 != _target);

            // Test for Equals when object is not a MeterSnapshot
            var notMeterSnapshot = "Fail";
            Assert.IsFalse(_target.Equals(notMeterSnapshot));

            // test for Equals when names are different
            target1.Name = "Different";
            Assert.IsFalse(_target.Equals(target1));

            // test for Equals when values are different
            var target2 = new MeterSnapshot
            {
                Name = "Test",
                Values = new Dictionary<MeterValueType, long>
                {
                    { MeterValueType.Lifetime, 1 },
                    { MeterValueType.Period, 4 }
                }
            };

            Assert.IsFalse(_target.Equals(target2));
        }

        [TestMethod]
        public void GetHashCodeTest()
        {
            var values = new Dictionary<MeterValueType, long>
            {
                { MeterValueType.Lifetime, 1 },
                { MeterValueType.Period, 3 }
            };

            _target.Values = values;

            // since we get different hashcodes depending on the machine it runs
            // on we can't check for a specific value.
            _target.GetHashCode();
        }
    }
}