using System;

namespace Aristocrat.Monaco.Hhr.Exceptions
{
    /// <summary>
    ///     Exception thrown when game response or recovery response messages arrive but we determine
    ///     that they should be ignored, for instance if recovery had started, or the response ID does
    ///     not match the request ID.
    /// </summary>
	[Serializable]
	public class IgnoreOutcomesException : Exception
	{
		public IgnoreOutcomesException()
		{
		}

		public IgnoreOutcomesException(string message) : base(message)
		{
		}
	}
}