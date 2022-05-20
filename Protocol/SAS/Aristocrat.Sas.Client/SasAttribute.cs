namespace Aristocrat.Sas.Client
{
    using System;

    /// <summary>
    /// The various control groups for SAS
    /// </summary>
    public enum SasGroup
    {
        /// <summary>An Aft consumer or handler</summary>
        Aft,

        /// <summary>Validation consumer or handler</summary>
        Validation,

        /// <summary>A legacy bonus consumer or handler</summary>
        LegacyBonus,

        /// <summary>A general control consumer or handler</summary>
        GeneralControl,

        /// <summary>A game start/end consumer</summary>
        GameStartEnd,

        /// <summary>A consumer that must be loaded especially for each client</summary>
        PerClientLoad,

        /// <summary>A progressive consumer or handler</summary>
        Progressives
    }

    /// <summary>
    /// Defines an attribute for SAS objects
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SasAttribute : Attribute
    {
        /// <summary>
        /// Initializes an instance of the SasAttribute class
        /// </summary>
        /// <param name="group">The group the object belongs to</param>
        public SasAttribute(SasGroup group)
        {
            Group = group;
        }

        /// <summary>
        /// Gets or sets the SAS group this object belongs to
        /// </summary>
        public SasGroup Group { get; }
    }
}
