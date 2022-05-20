namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using Contracts;

    /// <summary>
    ///     Definition of the TestTransaction class.
    /// </summary>
    public class TestTransaction : BaseTransaction
    {
        public TestTransaction()
        {
        }

        public TestTransaction(
            int deviceId,
            string name,
            DateTime transactionDateTime)
            : base(deviceId, transactionDateTime)
        {
            Name = name;
        }

        public override string Name { get; }

        public override object Clone()
        {
            var copy = new TestTransaction(DeviceId, Name, TransactionDateTime)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId
            };

            return copy;
        }
    }
}