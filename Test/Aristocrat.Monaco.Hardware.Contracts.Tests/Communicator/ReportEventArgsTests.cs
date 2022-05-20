namespace Aristocrat.Monaco.Hardware.Contracts.Communicator.Tests
{
    using System;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ReportEventArgsTests
    {
        [TestMethod]
        public void TestConstructor()
        {
            byte[] buffer = { 0x09, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 };

            ReportEventArgs target = new ReportEventArgs(buffer);

            Assert.IsNotNull(target);
            Assert.AreEqual(0x09, target.ReportId);
            CollectionAssert.AreEqual(new byte[] { 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 }, target.Buffer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestConstructor1()
        {
            byte[] buffer = null;

            ReportEventArgs target = new ReportEventArgs(buffer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestConstructor2()
        {
            byte[] buffer = { };

            ReportEventArgs target = new ReportEventArgs(buffer);
        }

        [TestMethod]
        public void TestConstructor3()
        {
            byte[] buffer = { 0x09, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 };
            int offset = 1;
            int count = 5;

            ReportEventArgs target = new ReportEventArgs(buffer, offset, count);

            Assert.IsNotNull(target);
            Assert.AreEqual(0x08, target.ReportId);
            Console.WriteLine(target.Buffer.Length);
            CollectionAssert.AreEqual(new byte[] { 0x07, 0x06, 0x05, 0x04 }, target.Buffer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestConstructor4()
        {
            byte[] buffer = null;
            int offset = 0;
            int count = 9;

            ReportEventArgs target = new ReportEventArgs(buffer, offset, count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestConstructor5()
        {
            byte[] buffer = { };
            int offset = 0;
            int count = 0;

            ReportEventArgs target = new ReportEventArgs(buffer, offset, count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestConstructor6()
        {
            byte[] buffer = { 0x09, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 };
            int offset = -1;
            int count = 9;

            ReportEventArgs target = new ReportEventArgs(buffer, offset, count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestConstructor7()
        {
            byte[] buffer = { 0x09, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 };
            int offset = 0;
            int count = 0;

            ReportEventArgs target = new ReportEventArgs(buffer, offset, count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestConstructor8()
        {
            byte[] buffer = { 0x09, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 };
            int offset = 0;
            int count = 0;

            ReportEventArgs target = new ReportEventArgs(buffer, offset, count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestConstructor9()
        {
            byte[] buffer = { 0x09, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 };
            int offset = 0;
            int count = 10;

            ReportEventArgs target = new ReportEventArgs(buffer, offset, count);
        }
    }
}
