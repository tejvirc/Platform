using System;

namespace Aristocrat.Monaco.Hhr.Exceptions
{
    /// <summary>
    ///     Exception thrown when attempt to recover from failed GamePlay request.
    /// </summary>
	[Serializable]
	public class GameRecoveryFailedException : Exception
	{
		public GameRecoveryFailedException()
		{
		}

		public GameRecoveryFailedException(string message) : base(message)
		{
		}
	}
}