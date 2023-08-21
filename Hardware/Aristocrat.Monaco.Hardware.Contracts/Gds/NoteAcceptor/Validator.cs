namespace Aristocrat.Monaco.Hardware.Contracts.Gds.NoteAcceptor
{
    using Contracts.NoteAcceptor;
    using System;
    using System.Timers;

    /// <summary>A validator.</summary>
    /// <seealso cref="T:System.IDisposable"/>
    public class Validator : IDisposable
    {
        #region constants

        private const int ExtendTimeoutInterval = 3000; // 3 seconds

        #endregion // constants

        #region private fields

        private readonly Note _note;
        private readonly string _barcode;
        private readonly Timer _timer;

        #endregion // private fields

        #region constructors

        /// <summary>
        /// Initializes a new instance of the Aristocrat.Monaco.Hardware.NoteAcceptor.NoteAcceptorGds.Validator class.
        /// </summary>
        /// <param name="note">The note to validate.</param>
        public Validator(Note note)
            : this()
        {
            _note = note;
        }

        /// <summary>
        /// Initializes a new instance of the Aristocrat.Monaco.Hardware.NoteAcceptor.NoteAcceptorGds.Validator class.
        /// </summary>
        /// <param name="barcode">The barcode.</param>
        public Validator(string barcode)
            : this()
        {
            _barcode = barcode;
        }

        /// <summary>
        /// Initializes a new instance of the Aristocrat.Monaco.Hardware.NoteAcceptor.NoteAcceptorGds.Validator class.
        /// </summary>
        protected Validator()
        {
            _timer = new Timer(ExtendTimeoutInterval) {
                AutoReset = true,
                Enabled = true
            };
        }

        #endregion // constructors

        #region operators

        /// <summary>Implicit cast that converts the given Validator to a string.</summary>
        /// <param name="validator">The validator.</param>
        /// <returns>The result of the operation.</returns>
        public static implicit operator string(Validator validator)
        {
            return validator?._barcode;
        }

        /// <summary>Implicit cast that converts the given Validator to a Note.</summary>
        /// <param name="validator">The validator.</param>
        /// <returns>The result of the operation.</returns>
        public static implicit operator Note(Validator validator)
        {
            return validator?._note;
        }

        #endregion // operators

        #region events

        /// <summary>Occurs when Timeout Expired.</summary>
        public event ElapsedEventHandler TimerElapsed
        {
            add => _timer.Elapsed += value;
            remove => _timer.Elapsed -= value;
        }

        #endregion // events

        #region public methods

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion // public methods

        #region protected methods

        /// <summary>
        /// Releases the unmanaged resources used by the
        /// Aristocrat.Monaco.Hardware.NoteAcceptor.NoteAcceptorGds.Validating and optionally releases the managed
        /// resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only
        /// unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_timer != null)
                {
                    _timer.Enabled = false;
                    _timer.Dispose();
                }
            }
        }

        #endregion // protected methods
    }
}
