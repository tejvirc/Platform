namespace Aristocrat.Monaco.Application.Contracts.Tests.Metering
{
    public class TestMeterProvider : BaseMeterProvider
    {
        public TestMeterProvider()
            : base(typeof(TestMeterProvider).ToString())
        {
        }
    }
}