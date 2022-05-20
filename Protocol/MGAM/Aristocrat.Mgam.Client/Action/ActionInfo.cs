namespace Aristocrat.Mgam.Client.Action
{
    using System;

    /// <summary>
    ///     Describes an action.
    /// </summary>
    public readonly struct ActionInfo
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ActionInfo"/> class.
        /// </summary>
        /// <param name="actionGuid"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public ActionInfo(Guid actionGuid, string name, string description)
        {
            ActionGuid = actionGuid;
            Name = name;
            Description = description;
        }

        /// <summary>
        ///     Gets or set the action identifier.
        /// </summary>
        public Guid ActionGuid { get; }

        /// <summary>
        ///     Gets or sets the action name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets or sets the action description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Compare for equality.
        /// </summary>
        /// <param name="lhs">Value to compare.</param>
        /// <param name="rhs">Value on the left.</param>
        /// <returns></returns>
        public static bool operator ==(ActionInfo lhs, ActionInfo rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        ///     Compare for inequality.
        /// </summary>
        /// <param name="lhs">Value on the left.</param>
        /// <param name="rhs">Value on the left.</param>
        /// <returns></returns>
        public static bool operator !=(ActionInfo lhs, ActionInfo rhs)
        {
            return !lhs.Equals(rhs);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is ActionInfo other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ActionGuid.GetHashCode();
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                return hashCode;
            }
        }

        private bool Equals(ActionInfo other)
        {
            return ActionGuid.Equals(other.ActionGuid) && Name == other.Name && Description == other.Description;
        }
    }
}
