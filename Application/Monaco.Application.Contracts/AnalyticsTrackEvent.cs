namespace Aristocrat.Monaco.Application.Contracts
{
    using Kernel;

    /// <summary>
    ///     An event to capture track-able analytics data, bucketed by an action and category.
    /// </summary>
    public class AnalyticsTrackEvent : BaseEvent
    {
        /// <summary>
        ///     The type of action to track.
        ///     This can be a dynamic list of values, but actions of similar type must conform to the same label.
        ///     Common examples could be configuration or gamePlayed.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        ///     The type of category to track.
        ///     This field is meant to add context to what part of the EGM the information is coming from.
        ///     Common examples could be platform or game.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        ///     A JSON data blob of arbitrary data to be tracked
        /// </summary>
        public string Traits { get; set; }

        /// <summary>
        ///     Constructs an analytics event for tracking to pick up and record.
        ///     The action and category serve as bucketing and categorization, and the
        ///     traits represent the actual data to be tracked. 
        /// </summary>
        /// <param name="action">The type of action to track.</param>
        /// <param name="category">The type of category to track.</param>
        /// <param name="traits">A JSON data blob of arbitrary data to be tracked</param>
        public AnalyticsTrackEvent(string action, string category, string traits)
        {
            Action = action;
            Category = category;
            Traits = traits;
        }
    }
}
