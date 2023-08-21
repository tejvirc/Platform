namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using Asp.Client.DataSources;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DummyDataSourceTests
    {
        private DummyDataSource _dummyDataSource;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _dummyDataSource = new DummyDataSource();
        }

        [TestMethod]
        public void MemberValuesTest()
        {
            var testMembers = new Dictionary<string, object>
            {
                { "Test_Member_1", 0 },
                { "Test_Member_2", false },
                { "Test_Member_3", "test_string" }
            };

            _dummyDataSource.MemberValues = testMembers;

            CollectionAssert.AreEqual(testMembers, _dummyDataSource.MemberValues);
            Assert.AreEqual(false, _dummyDataSource.GetMemberValue("Test_Member_2"));

            Assert.AreEqual(3, _dummyDataSource.Members.Count);
            Assert.AreEqual("test_string", _dummyDataSource.GetMemberValue(_dummyDataSource.Members[2]));
        }

        [TestMethod]
        public void ChangeDataMemberTest()
        {
            var memberValueChangedEventsReceived = 0;
            _dummyDataSource.MemberValueChanged += (sender, eventargs) => ++memberValueChangedEventsReceived;

            _dummyDataSource.SetMemberValue("Test_Member", 0);
            _dummyDataSource.ChangeDataMember("Test_Member", 1);
            Assert.AreEqual(1, _dummyDataSource.GetMemberValue("Test_Member"));
            Assert.AreEqual(1, memberValueChangedEventsReceived);

            _dummyDataSource.ChangeDataMember("Test_Member_2", 2);
            Assert.AreEqual(2, _dummyDataSource.GetMemberValue("Test_Member_2"));
            Assert.AreEqual(2, memberValueChangedEventsReceived);
        }
    }
}