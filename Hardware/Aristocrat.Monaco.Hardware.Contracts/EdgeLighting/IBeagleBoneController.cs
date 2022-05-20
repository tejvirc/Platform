namespace Aristocrat.Monaco.Hardware.Contracts.EdgeLighting
{
    using System;
    using Kernel;

    /// <summary>
    ///     Names of light shows used for SendShow
    /// </summary>
    public enum LightShows
    {
        /// <summary>
        ///     Used for normal operation
        /// </summary>
        [ShowName("DFLT")]
        Default,

        /// <summary>
        ///     Used for Red Screen Free Spins
        /// </summary>
        [ShowName("REDS")]
        Reds,

        /// <summary>
        ///     Used for normal wins
        /// </summary>
        [ShowName("WIN1")]
        Win1,

        /// <summary>
        ///     Used for better than normal wins
        /// </summary>
        [ShowName("WIN2")]
        Win2,

        /// <summary>
        ///     Used for the best wins
        /// </summary>
        [ShowName("WIN4")]
        Win4,

        /// <summary>
        ///     Used for attract mode
        /// </summary>
        [ShowName("ATTR")]
        Attract,

        /// <summary>
        ///     Used for starting play
        /// </summary>
        [ShowName("STRT")]
        Start
    }

    /// <summary>
    ///     An Attribute for getting the name of the show
    /// </summary>
    public class ShowNameAttribute : Attribute
    {
        /// <summary>
        ///     The name of the show to play
        /// </summary>
        public string ShowName { get; }

        /// <summary>
        ///     Creates an instance of <see cref="ShowNameAttribute"/>
        /// </summary>
        /// <param name="showName">The show name</param>
        public ShowNameAttribute(string showName)
        {
            ShowName = showName;
        }
    }

    /// <summary>
    ///     Interface for controlling Beagle Bone Edge lighting device
    /// </summary>
    public interface IBeagleBoneController : IService
    {
        /// <summary>
        ///     Sends a show to the Beagle Bone Edge lighting controller
        /// </summary>
        /// <param name="show">Show index for the desired show</param>
        void SendShow(LightShows show);
    }
}
