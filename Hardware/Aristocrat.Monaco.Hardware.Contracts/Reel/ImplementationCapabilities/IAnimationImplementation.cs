﻿namespace Aristocrat.Monaco.Hardware.Contracts.Reel.ImplementationCapabilities
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using ControlData;
    using Events;

    /// <summary>
    ///     The reel controller animation capability of an implementation
    /// </summary>
    [CLSCompliant(false)]
    public interface IAnimationImplementation : IReelImplementationCapability
    {
        /// <summary>
        ///     The event that occurs when the reel controller starts a light animation
        /// </summary>
        event EventHandler<LightAnimationEventArgs> LightAnimationStarted;

        /// <summary>
        ///     The event that occurs when the reel controller completes a light animation
        /// </summary>
        event EventHandler<LightAnimationEventArgs> LightAnimationCompleted;

        /// <summary>
        ///     The event that occurs when the reel controller starts a reel animation
        /// </summary>
        event EventHandler<LightAnimationEventArgs> ReelAnimationStarted;

        /// <summary>
        ///     The event that occurs when the reel controller completes a reel animation
        /// </summary>
        event EventHandler<LightAnimationEventArgs> ReelAnimationCompleted;

        /// <summary>
        /// Contains all the loaded animation files
        /// </summary>
        HashSet<AnimationData> AnimationFiles { get; }

        /// <summary>
        ///     Loads an animation data onto the controller.
        /// </summary>
        /// <param name="data">The animation data.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        Task<bool> LoadAnimationFile(AnimationData data, CancellationToken token);
        
        /// <summary>
        ///     Loads animation files onto the controller.
        /// </summary>
        /// <param name="files">The animation files.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> LoadAnimationFiles(IEnumerable<AnimationData> files, CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to prepare a light show animation.
        /// </summary>
        /// <param name="data">The light show data.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> PrepareAnimation(LightShowData data, CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to prepare light show animations.
        /// </summary>
        /// <param name="files">The light show files.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> PrepareAnimations(IEnumerable<LightShowData> files, CancellationToken token);
        
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
        ///     Instructs the controller to remove all animations
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        Task<bool> RemoveAllAnimations(CancellationToken token);
        
        /// <summary>
        ///     Instructs the controller to stop playing selected light show animations.
        /// </summary>
        /// <param name="files">The light show files.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> StopLightShowAnimations(IEnumerable<LightShowData> files, CancellationToken token);
        
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