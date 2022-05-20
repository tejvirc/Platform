namespace Aristocrat.Monaco.Hhr.Client.Tests.WorkFlow
{
    using System;
    using Client.WorkFlow;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SequenceIdManagerTests : IObserver<uint>
    {
        private SequenceIdManager _target;
        private readonly uint _currentSequenceNumber = 10;
        private uint _sequenceId;

        public void OnNext(uint value)
        {
            _sequenceId = value;
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _target = new SequenceIdManager(_currentSequenceNumber);
            _target.SequenceIdObservable.Subscribe(this);
        }

        [TestMethod]
        public void FetchNextSequenceIdExpectNext()
        {
            Assert.AreEqual(_currentSequenceNumber + 1, _target.NextSequenceId);
            Assert.AreEqual(_currentSequenceNumber + 1, _sequenceId);
        }

        [TestMethod]
        public void OverflowSequenceIdFetchNextExpectSequenceIdToRollover()
        {
            _target = new SequenceIdManager(uint.MaxValue);
            _target.SequenceIdObservable.Subscribe(this);

            Assert.AreEqual(1u, _target.NextSequenceId);
            Assert.AreEqual(1u, _sequenceId);
        }
    }
}