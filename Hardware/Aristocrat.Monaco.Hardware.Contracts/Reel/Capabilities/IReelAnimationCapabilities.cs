namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Capabilities
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using ControlData;

    /// <summary>
    ///     The public interface for reel controller animation capabilities
    /// </summary>
    public interface IReelAnimationCapabilities : IReelControllerCapability
    {
        // TODO: Remove "Controller" from all the method names.T
        // This was added to lower the % of duplicated lines SonarQube was seeing.T
        //  This interface should match IAnimationImplementation
        //  Once more code is added the % will be much lower.

        /// <summary>
        ///     Loads an animation file onto the controller.
        /// </summary>
        /// <param name="file">The animation file.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        Task<bool> LoadControllerAnimationFile(AnimationFile file, CancellationToken token);
        
        /// <summary>
        ///     Loads animation files onto the controller.
        /// </summary>
        /// <param name="files">The animation files.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> LoadControllerAnimationFiles(IEnumerable<AnimationFile> files, CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to prepare a light show animation.
        /// </summary>
        /// <param name="file">The light show file.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> PrepareControllerAnimation(LightShowFile file, CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to prepare light show animations.
        /// </summary>
        /// <param name="files">The light show files.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> PrepareControllerAnimations(IEnumerable<LightShowFile> files, CancellationToken token);
        
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
        ///     Instructs the controller to stop playing selected light show animations.
        /// </summary>
        /// <param name="files">The light show files.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> StopControllerLightShowAnimations(IEnumerable<LightShowFile> files, CancellationToken token);
        
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
