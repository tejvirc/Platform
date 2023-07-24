namespace Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData
{
    using System;

    /// <summary>
    ///     The data associated with a reel controller animation file
    /// </summary>
    [CLSCompliant(false)]
    public class AnimationFile
    {
        /// <summary>
        ///     Create an AnimationFile
        /// </summary>
        /// <param name="path">The path to the animation file</param>
        /// <param name="animationType">The type of the animation file</param>
        public AnimationFile(string path, AnimationType animationType) : this(path, animationType, System.IO.Path.GetFileNameWithoutExtension(path))
        {
        }

        /// <summary>
        ///     Create an AnimationFile
        /// </summary>
        /// <param name="path">The path to the animation file</param>
        /// <param name="animationType">The type of the animation file</param>
        /// <param name="friendlyName">The friendly name of the animation file</param>
        public AnimationFile(string path, AnimationType animationType, string friendlyName)
        {
            Path = path;
            AnimationType = animationType;
            FriendlyName = friendlyName;
        }

        /// <summary>
        ///     The path to the animation file
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        ///     The friendly name of the light show file
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        ///     The id of the file
        /// </summary>
        public uint AnimationId { get; set; }

        /// <summary>
        ///     The type of the animation file
        /// </summary>
        public AnimationType AnimationType { get; set; }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((AnimationFile)obj);
        }
        
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Path != null ? Path.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FriendlyName != null ? FriendlyName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)AnimationId;
                hashCode = (hashCode * 397) ^ (int)AnimationType;
                return hashCode;
            }
        }

        /// <summary>
        ///     Returns a value indicating if this object is equal to the other object
        /// </summary>
        /// <param name="other">The other instance to compare to</param>
        /// <returns>True is the objects are equal</returns>
        protected bool Equals(AnimationFile other)
        {
            return Path == other.Path && FriendlyName == other.FriendlyName && AnimationId == other.AnimationId && AnimationType == other.AnimationType;
        }
    }
}
