namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using System;
using Cabinet.Contracts;
using Gaming.Contracts.InfoBar;

/// <summary>
///     Causes the InfoBar to become visible and display a static message
/// </summary>
/// <remarks>
///     Supports conformity to G2S Message Protocol v3.0, Appendix E, Section 4.2
/// </remarks>
public record InfoBarDisplayStaticMessageAction
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InfoBarDisplayStaticMessageEvent" /> record.
    /// </summary>
    /// <param name="ownerId">The owner ID.</param>
    /// <param name="message">The message to display on the InfoBar.</param>
    /// <param name="duration">The duration to display the message for.</param>
    /// <param name="textColor">Color of the text.</param>
    /// <param name="backgroundColor">Color of the bar background.</param>
    /// <param name="region">The region where the text will be displayed.</param>
    /// <param name="displayTarget">The display which this event is targeted at</param>
    public InfoBarDisplayStaticMessageAction(
        Guid ownerId,
        string message,
        TimeSpan duration,
        InfoBarColor textColor,
        InfoBarColor backgroundColor,
        InfoBarRegion region,
        DisplayRole displayTarget)
    {
        OwnerId = ownerId;
        Message = message;
        Duration = duration;
        TextColor = textColor;
        BackgroundColor = backgroundColor;
        Region = region;
        DisplayTarget = displayTarget;
    }

    public Guid OwnerId { get; }

    public string Message { get; }

    public TimeSpan Duration { get; }

    public InfoBarColor TextColor { get; }

    public InfoBarColor BackgroundColor { get; }

    public InfoBarRegion Region { get; }

    public DisplayRole DisplayTarget { get; }
}