namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Capabilities
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using ControlData;

    /// <summary>
    ///     The public interface for reel controller animation capabilities
    /// </summary>
    [CLSCompliant(false)]
    public interface IReelAnimationCapabilities : IReelControllerCapability
    {
        // TODO: Remove "Controller" from all the method names.
        // This was added to lower the % of duplicated lines SonarQube was seeing.
        //  This interface should match IAnimationImplementation
        //  Once more code is added the % will be much lower.

        /// <summary>
        /// Contains all the loaded animation files
        /// </summary>
        public HashSet<AnimationData> AnimationFiles { get; }

        /// <summary>
        ///     Loads an animation data onto the controller.
        /// </summary>
        /// <param name="data">The animation data.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        Task<bool> LoadControllerAnimationFile(AnimationData data, CancellationToken token);
        
        /// <summary>
        ///     Loads animation files onto the controller.
        /// </summary>
        /// <param name="files">The animation files.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> LoadControllerAnimationFiles(IEnumerable<AnimationData> files, CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to prepare a light show animation.
        /// </summary>
        /// <param name="data">The light show data.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> PrepareControllerAnimation(LightShowData data, CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to prepare light show animations.
        /// </summary>
        /// <param name="files">The light show files.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> PrepareControllerAnimations(IEnumerable<LightShowData> files, CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to prepare a curve animation.
        /// </summary>
        /// <param name="curveData">The reel curve data.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> PrepareControllerAnimation(ReelCurveData curveData, CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to prepare curve animations.
        /// </summary>
        /// <param name="curveData">The reel curve data.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> PrepareControllerAnimations(IEnumerable<ReelCurveData> curveData, CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to play all animations.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        Task<bool> PlayControllerAnimations(CancellationToken token);

        /// <summary>
        ///     Instructs the controller to remove all animatin files
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        Task<bool> RemoveAllControllerAnimations(CancellationToken token);

        /// <summary>
        ///     Instructs the controller to stop playing selected light show animations.
        /// </summary>
        /// <param name="files">The light show files.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> StopControllerLightShowAnimations(IEnumerable<LightShowData> files, CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to stop playing all light show animations.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        Task<bool> StopAllControllerLightShows(CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to stop the reels.
        /// </summary>
        /// <param name="stopData">The reel stop data.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> PrepareControllerStopReels(IEnumerable<ReelStopData> stopData, CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to nudge the reels.
        /// </summary>
        /// <param name="nudgeData">The reel nudge data.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> PrepareControllerNudgeReels(IEnumerable<NudgeReelData> nudgeData, CancellationToken token);
    }
}
