﻿namespace Aristocrat.Monaco.Sas.Storage.Models;

using Common.Storage;

/// <summary>
///     The exception queue entity
/// </summary>
public class ExceptionQueue : BaseEntity
{
    /// <summary>
    ///     Gets or sets the client id
    /// </summary>
    public int ClientId { get; set; }

    /// <summary>
    ///     Gets or sets the queue
    /// </summary>
    public string Queue { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the priority queue
    /// </summary>
    public int PriorityQueue { get; set; }
}