namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using System;

    /// <summary>
    ///     The AssignedProgressiveId defines a type and unique string identifier used to
    ///     associate a <see cref="ProgressiveLevel"/> with the assigned progressive
    ///     key. The key should be unique and can be used to lookup the associated selectable
    ///     progressive in the corresponding provider that owns the progressive level.
    /// </summary>
    [Serializable]
    public class AssignableProgressiveId
    {
        /// <summary>
        ///     Creates a new instance of the AssignableProgressiveId
        /// </summary>
        /// <param name="assignedProgressiveType">The assigned progressive type</param>
        /// <param name="assignedProgressiveKey">The assigned progressive key</param>
        public AssignableProgressiveId(AssignableProgressiveType assignedProgressiveType, string assignedProgressiveKey)
        {
            AssignedProgressiveType = assignedProgressiveType;
            AssignedProgressiveKey = assignedProgressiveKey;
        }

        /// <summary>
        ///     The type of selectable progressive
        /// </summary>
        public AssignableProgressiveType AssignedProgressiveType { get; }

        /// <summary>
        ///     The unique identifier used to lookup the assigned level
        /// </summary>
        public string AssignedProgressiveKey { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as AssignableProgressiveId);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)AssignedProgressiveType * 397) ^ (AssignedProgressiveKey != null ? AssignedProgressiveKey.GetHashCode() : 0);
            }
        }

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Equals(AssignableProgressiveId obj)
        {
            return ReferenceEquals(this, obj)
                || string.Equals(AssignedProgressiveKey, obj.AssignedProgressiveKey)
                && AssignedProgressiveType == obj.AssignedProgressiveType;
        }
    }
}