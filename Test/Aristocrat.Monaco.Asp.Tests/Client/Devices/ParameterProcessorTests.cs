namespace Aristocrat.Monaco.Asp.Tests.Client.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Asp.Client.Contracts;
    using Asp.Client.Devices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ParameterProcessorTests : AspUnitTestBase<TestDataContext>
    {
        private ParameterProcessor CreateParameterProcessor()
        {
            SetupDataSource();
            SetupParameterFactory();

            return new ParameterProcessor(ParameterFactory);
        }

        [TestMethod]
        public void ParameterProcessorTest()
        {
            var processor = CreateParameterProcessor();
            Assert.IsNotNull(processor);
        }

        [TestMethod]
        public void ParameterProcessorAlwaysEvents()
        {
            var raisedEventParameters = new List<IParameter>();
            var processor = CreateParameterProcessor();
            processor.ParameterEvent += (x, y) => { raisedEventParameters.Add(y); };
            var expectedParameters =
                ParameterFactory.SelectParameterPrototypes(x => x.EventAccessType == EventAccessType.Always);
            var members = new Dictionary<string, object>() { { "DM1", 2 } };
            DataSourceMock.Raise(i => i.MemberValueChanged += null, null, members);
            Assert.IsTrue(expectedParameters.Count != 0);
            Assert.AreEqual(raisedEventParameters.Count, expectedParameters.Count);
            Assert.AreEqual(
                0,
                raisedEventParameters.Count(x => x.EventAccessType != EventAccessType.Always));
            Assert.IsNotNull(processor);
        }

        [TestMethod]
        public void ClearEventTest()
        {
            var processor = CreateParameterProcessor();

            var parPrototype = ParameterFactory
                .SelectParameterPrototypes(x => x.EventAccessType == EventAccessType.Always).First();
            var parameter = ParameterFactory.Create(parPrototype.ClassId, parPrototype.TypeId, parPrototype);
            Assert.IsFalse(processor.ClearEvent(parameter));

            parPrototype = ParameterFactory.SelectParameterPrototypes(x => x.EventAccessType == EventAccessType.Never)
                .First();
            parameter = ParameterFactory.Create(parPrototype.ClassId, parPrototype.TypeId, parPrototype);
            Assert.IsFalse(processor.ClearEvent(parameter));

            parPrototype = ParameterFactory
                .SelectParameterPrototypes(x => x.EventAccessType == EventAccessType.OnRequest).First();
            parameter = ParameterFactory.Create(parPrototype.ClassId, parPrototype.TypeId, parPrototype);
            Assert.IsTrue(processor.ClearEvent(parameter));
            Assert.IsTrue(processor.SetEvent(parameter));
            Assert.IsTrue(processor.ClearEvent(parameter));

            var raisedEventParameters = new List<IParameter>();
            processor.ParameterEvent += (x, y) => { raisedEventParameters.Add(y); };
            var members = new Dictionary<string, object>() { { "DM1", 2 } };
            DataSourceMock.Raise(i => i.MemberValueChanged += null, null, members);

            Assert.IsTrue(raisedEventParameters.Count(x => x.Prototype == parPrototype) == 0);

            var parPrototypes =
                ParameterFactory.SelectParameterPrototypes(x => x.EventAccessType == EventAccessType.OnRequest);
            foreach (var prototype in parPrototypes)
            {
                parameter = ParameterFactory.Create(prototype.ClassId, prototype.TypeId, prototype);
                Assert.IsTrue(processor.ClearEvent(parameter));
                Assert.IsTrue(processor.SetEvent(parameter));
                Assert.IsTrue(processor.ClearEvent(parameter));
            }

            raisedEventParameters.Clear();
            DataSourceMock.Raise(i => i.MemberValueChanged += null, null, members);
            Assert.IsTrue(raisedEventParameters.Count(x => x.EventAccessType == EventAccessType.OnRequest) == 0);
        }

        [TestMethod]
        public void SetEventTest()
        {
            var processor = CreateParameterProcessor();

            var parPrototype = ParameterFactory
                .SelectParameterPrototypes(x => x.EventAccessType == EventAccessType.Always).First();
            var parameter = ParameterFactory.Create(parPrototype.ClassId, parPrototype.TypeId, parPrototype);
            Assert.IsFalse(processor.SetEvent(parameter));

            parPrototype = ParameterFactory.SelectParameterPrototypes(x => x.EventAccessType == EventAccessType.Never)
                .First();
            parameter = ParameterFactory.Create(parPrototype.ClassId, parPrototype.TypeId, parPrototype);
            Assert.IsFalse(processor.SetEvent(parameter));

            parPrototype = ParameterFactory
                .SelectParameterPrototypes(x => x.EventAccessType == EventAccessType.OnRequest).First();
            parameter = ParameterFactory.Create(parPrototype.ClassId, parPrototype.TypeId, parPrototype);
            Assert.IsTrue(processor.SetEvent(parameter));

            var raisedEventParameters = new List<IParameter>();
            processor.ParameterEvent += (x, y) => { raisedEventParameters.Add(y); };
            var members = new Dictionary<string, object>() { { "DM1", 2 } };
            DataSourceMock.Raise(i => i.MemberValueChanged += null, null, members);

            Assert.IsTrue(
                raisedEventParameters.Count(
                    x => x.Prototype == parPrototype && x.EventAccessType != EventAccessType.Always) == 1);

            var parPrototypes =
                ParameterFactory.SelectParameterPrototypes(x => x.EventAccessType == EventAccessType.OnRequest);
            foreach (var prototype in parPrototypes)
            {
                parameter = ParameterFactory.Create(prototype.ClassId, prototype.TypeId, prototype);
                Assert.IsTrue(processor.SetEvent(parameter));
            }

            raisedEventParameters.Clear();
            DataSourceMock.Raise(i => i.MemberValueChanged += null, null, members);
            Assert.IsTrue(
                raisedEventParameters.Count(x => x.EventAccessType == EventAccessType.OnRequest) ==
                parPrototypes.Count);
        }

        [TestMethod]
        public void GetParameterTest()
        {
            var processor = CreateParameterProcessor();

            DataSourceMock.Setup(x => x.GetMemberValue("DM1")).Returns(2);
            var parPrototypes = ParameterFactory.SelectParameterPrototypes(x => x.ClassId.Id != 1);
            var fieldCount = 0;
            foreach (var prototype in parPrototypes)
            {
                var parameter = ParameterFactory.Create(prototype.ClassId, prototype.TypeId, prototype);
                Assert.IsNotNull(processor.GetParameter(parameter));
                foreach (var field in parameter.Fields)
                {
                    Assert.AreEqual(field.Id, 1); // Usually we have Id == 1
                    Assert.AreEqual((byte)2, field.Value);
                    fieldCount++;
                }
            }

            DataSourceMock.Verify(x => x.GetMemberValue("DM1"), Times.Exactly(fieldCount));
        }

        [TestMethod]
        public void SetParameterTest()
        {
            var processor = CreateParameterProcessor();

            DataSourceMock.Setup(x => x.SetMemberValue("DM1", (byte)3));
            var parPrototypes = ParameterFactory.SelectParameterPrototypes(x => true);
            var fieldCount = 0;
            foreach (var prototype in parPrototypes)
            {
                var parameter = ParameterFactory.Create(prototype.ClassId, prototype.TypeId, prototype);
                foreach (var field in parameter.Fields)
                {
                    field.Value = 3;
                    if (parameter.MciAccessType != AccessType.ReadOnly)
                    {
                        fieldCount++;
                    }
                }

                Assert.AreEqual(parameter.MciAccessType == AccessType.ReadWrite, processor.SetParameter(parameter));
            }

            DataSourceMock.Verify(x => x.SetMemberValue("DM1", (byte)3), Times.Exactly(fieldCount));
        }

        [TestMethod]
        public void SetParameterTestException()
        {
            var processor = CreateParameterProcessor();

            DataSourceMock.Setup(x => x.SetMemberValue("DM1", (byte)3)).Throws<Exception>();
            var parPrototypes = ParameterFactory.SelectParameterPrototypes(x => true);
            var fieldCount = 0;
            foreach (var prototype in parPrototypes)
            {
                var parameter = ParameterFactory.Create(prototype.ClassId, prototype.TypeId, prototype);
                foreach (var field in parameter.Fields)
                {
                    field.Value = 3;
                    if (parameter.MciAccessType != AccessType.ReadOnly)
                    {
                        fieldCount++;
                    }
                }

                Assert.AreEqual(false, processor.SetParameter(parameter));
            }

            DataSourceMock.Verify(x => x.SetMemberValue("DM1", (byte)3), Times.Exactly(fieldCount));
        }
    }
}