namespace Aristocrat.Monaco.Common.Animation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     The animation mode.
    /// </summary>
    public enum AnimationMode
    {
        /// <summary>
        ///     Clamps value to nearest key frame.
        /// </summary>
        Discrete,

        /// <summary>
        ///     Linear interpolation is used.
        /// </summary>
        Linear
    }

    /// <summary>
    ///     Defines an animation via keyframes.
    /// </summary>
    /// <typeparam name="T">
    ///     Designed to be a scalar, or Vector2, Vector3, Vector4.
    ///     The type must support operator+ and operator*.
    /// </typeparam>
    public class Animation<T>
    {
        private readonly List<KeyFrame<T>> _keyFrames;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Animation{T}" /> class.
        /// </summary>
        public Animation()
        {
            _keyFrames = new List<KeyFrame<T>>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Animation{T}" /> class.
        /// </summary>
        /// <param name="keyFrames">List of keyframes, which should be sorted by KeyTime.</param>
        public Animation(IEnumerable<KeyFrame<T>> keyFrames)
        {
            _keyFrames = new List<KeyFrame<T>>(keyFrames);
        }

        /// <summary>
        ///     Gets or sets the animation mode.  The default is linear.
        /// </summary>
        public AnimationMode AnimationMode { get; set; } = AnimationMode.Linear;

        /// <summary>
        ///     Gets the number of key frames.
        /// </summary>
        public int KeyFrameCount => _keyFrames.Count;

        /// <summary>
        ///     Gets the ith keyframe in the animation.
        /// </summary>
        /// <param name="index">The index to the keyframe.</param>
        public KeyFrame<T> this[int index] => _keyFrames[index];

        /// <summary>
        ///     Inserts a keyframe in order of key timestamp.
        /// </summary>
        /// <param name="keyTime">The keyframe timestamp.</param>
        /// <param name="value">The keyframe value.</param>
        public void Insert(float keyTime, T value)
        {
            var kf = new KeyFrame<T> { KeyTime = keyTime, Value = value };

            var duplicate = _keyFrames.Any(item => Math.Abs(item.KeyTime - keyTime) < 0.001f);
            if (duplicate)
            {
                throw new ArgumentException("Duplicate key");
            }

            if (_keyFrames.Count == 0)
            {
                _keyFrames.Add(kf);
            }
            else if (keyTime >= _keyFrames[_keyFrames.Count - 1].KeyTime)
            {
                _keyFrames.Add(kf);
            }
            else
            {
                var insertIndex = _keyFrames.FindIndex(x => keyTime <= x.KeyTime);
                _keyFrames.Insert(insertIndex, kf);
            }
        }

        /// <summary>
        ///     Removes a key frame.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        public void RemoveAt(int index)
        {
            _keyFrames.RemoveAt(index);
        }

        /// <summary>
        ///     Interpolates the animation at the specified time.
        /// </summary>
        /// <param name="t">The timestamp to evaluate the animation.</param>
        /// <returns>The interpolated value.</returns>
        public T Evaluate(float t)
        {
            if (_keyFrames.Count == 0)
            {
                throw new InvalidOperationException("Must have at least one keyframe.");
            }

            switch (AnimationMode)
            {
                case AnimationMode.Discrete:
                    return EvaluateDiscrete(t);
                default:
                    return EvaluateLinear(t);
            }
        }

        private T EvaluateDiscrete(float t)
        {
            var lastIndex = _keyFrames.Count - 1;

            if (_keyFrames.Count == 1)
            {
                return _keyFrames[0].Value;
            }

            if (t <= _keyFrames[0].KeyTime)
            {
                return _keyFrames[0].Value;
            }

            if (t >= _keyFrames[lastIndex].KeyTime)
            {
                return _keyFrames[lastIndex].Value;
            }

            for (var i = 0; i < _keyFrames.Count - 1; ++i)
            {
                var t0 = _keyFrames[i].KeyTime;
                var t1 = _keyFrames[i + 1].KeyTime;

                if (t >= t0 && t < t1)
                {
                    var m = 0.5f * (t0 + t1);

                    // Snap to nearest keyframe.
                    return t < m ? _keyFrames[i].Value : _keyFrames[i + 1].Value;
                }
            }

            // Should not get here.
            return _keyFrames[0].Value;
        }

        private T EvaluateLinear(float t)
        {
            var lastIndex = _keyFrames.Count - 1;

            if (_keyFrames.Count == 1)
            {
                return _keyFrames[0].Value;
            }

            if (t <= _keyFrames[0].KeyTime)
            {
                return _keyFrames[0].Value;
            }

            if (t >= _keyFrames[lastIndex].KeyTime)
            {
                return _keyFrames[lastIndex].Value;
            }

            for (var i = 0; i < _keyFrames.Count - 1; ++i)
            {
                var t0 = _keyFrames[i].KeyTime;
                var t1 = _keyFrames[i + 1].KeyTime;
                if (t >= t0 && t < t1)
                {
                    // dynamic gets us around C# constraints not being able to specify
                    // whether a type has certain operators.  dynamic probably has overhead,
                    // but we do not plan to be animation intensive in the platform.
                    dynamic v0 = _keyFrames[i].Value;
                    dynamic v1 = _keyFrames[i + 1].Value;

                    // Type arguments must support operators (+, *).
                    return (1.0f - t) * v0 + t * v1;
                }
            }

            // Should not get here.
            return _keyFrames[0].Value;
        }
    }
}