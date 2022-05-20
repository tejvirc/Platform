namespace Aristocrat.G2S.Emdi.Meters
{
    using Protocol.v21ext1b1;

    /// <summary>
    /// Meter subscription
    /// </summary>
    public class MeterSubscription
    {
        private readonly int _hash;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeterSubscription"/> class.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="meter"></param>
        public MeterSubscription(int port, (string Name, t_meterTypes Type) meter)
        {
            Port = port;
            Meter = meter;

            _hash = 33 * Port.GetHashCode() + Meter.GetHashCode();
        }

        /// <summary>
        /// Gets the port
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Gets the meter name and type
        /// </summary>
        public (string Name, t_meterTypes Type) Meter { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is MeterSubscription sub))
            {
                return false;
            }

            return sub.Port.Equals(Port) && sub.Meter.Equals(Meter);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _hash;
        }
    }
}