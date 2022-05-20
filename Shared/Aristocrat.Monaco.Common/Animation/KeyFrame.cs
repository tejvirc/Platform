namespace Aristocrat.Monaco.Common.Animation
{
    /// <summary>
    ///     Defines a key frame.
    /// </summary>
    /// <typeparam name="T">
    ///     Designed to be a scalar, or Vector2, Vector3, Vector4.
    ///     The type must support operator+ and operator*.
    /// </typeparam>
    public struct KeyFrame<T>
    {
        /// <summary>
        ///     The timestamp of the key frame.
        /// </summary>
        public float KeyTime;

        /// <summary>
        ///     The value of the key frame.
        /// </summary>
        public T Value;
    }
}