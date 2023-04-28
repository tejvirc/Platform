﻿namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Capabilities
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
        /// <summary>
        ///     Loads an animation file onto the controller.
        /// </summary>
        /// <param name="file">The animation file.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        Task<bool> LoadAnimationFile(AnimationFile file, CancellationToken token);
        
        /// <summary>
        ///     Loads animation files onto the controller.
        /// </summary>
        /// <param name="files">The animation files.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> LoadAnimationFiles(IEnumerable<AnimationFile> files, CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to prepare a light show animation.
        /// </summary>
        /// <param name="file">The light show file.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> PrepareAnimation(LightShowFile file, CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to prepare light show animations.
        /// </summary>
        /// <param name="files">The light show files.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> PrepareAnimations(IEnumerable<LightShowFile> files, CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to prepare a curve animation.
        /// </summary>
        /// <param name="curveData">The reel curve data.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> PrepareAnimation(ReelCurveData curveData, CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to prepare curve animations.
        /// </summary>
        /// <param name="curveData">The reel curve data.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> PrepareAnimations(IEnumerable<ReelCurveData> curveData, CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to play all animations.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        Task<bool> PlayAnimations(CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to stop playing selected light show animations.
        /// </summary>
        /// <param name="files">The light show files.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> StopLightShowAnimations(IEnumerable<LightShowFile> files, CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to stop playing all light show animations.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        Task<bool> StopAllLightShows(CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to stop the reels.
        /// </summary>
        /// <param name="stopData">The reel stop data.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> PrepareStopReels(IEnumerable<ReelStopData> stopData, CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to nudge the reels.
        /// </summary>
        /// <param name="nudgeData">The reel nudge data.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> PrepareNudgeReels(IEnumerable<NudgeReelData> nudgeData, CancellationToken token);
    }
}
