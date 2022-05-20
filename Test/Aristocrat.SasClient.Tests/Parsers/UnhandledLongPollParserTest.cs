namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sas.Client;

    /// <summary>
    ///     Contains the tests for the UnhandledLongPollHandler class
    /// </summary>
    [TestClass]
    public class UnhandledLongPollParserTest
    {
        private UnhandledLongPollParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new UnhandledLongPollParser();
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.None, _target.Command);
        }

        [TestMethod]
        public void ParseTest()
        {
            _target.InjectHandler(null);
            Assert.IsNull(_target.Parse(new List<byte> { 0x00, 0x00 }));
        }
    }
}
