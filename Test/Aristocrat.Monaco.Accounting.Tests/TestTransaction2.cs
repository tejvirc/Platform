namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using Contracts;

    /// <summary>
    ///     Definition of the TestTransaction2 class.
    /// </summary>
    public class TestTransaction2 : BaseTransaction
    {
        public TestTransaction2(int deviceId, DateTime transactionDateTime)
            : base(deviceId, transactionDateTime)
        {
        }

        public TestTransaction2()
        {
        }

        public override string Name => "Test Transaction2";

        public override object Clone()
        {
            var copy = new TestTransaction2(DeviceId, TransactionDateTime)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId
            };

            return copy;
        }
    }
}