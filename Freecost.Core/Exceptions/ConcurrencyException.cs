using Freecost.Core.Models;
using System;

namespace Freecost.Core.Services;

/// <summary>
/// Exception thrown when a concurrency conflict is detected during an update operation
/// </summary>
public class ConcurrencyException : Exception
{
    /// <summary>
    /// The entity that was being updated when the conflict occurred
    /// </summary>
    public BaseEntity? ConflictingEntity { get; }

    /// <summary>
    /// The original EF Core concurrency exception
    /// </summary>
    public Exception? InnerConcurrencyException { get; }

    public ConcurrencyException(string message, BaseEntity? entity = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ConflictingEntity = entity;
        InnerConcurrencyException = innerException;
    }

    public ConcurrencyException(string message)
        : base(message)
    {
    }

    public ConcurrencyException(string message, Exception innerException)
        : base(message, innerException)
    {
        InnerConcurrencyException = innerException;
    }
}
