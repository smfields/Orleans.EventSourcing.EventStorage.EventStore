namespace Orleans.EventSourcing.EventStorage.EventStore.Options;

/// <summary>
/// Exception for throwing from Redis event storage.
/// </summary>
[GenerateSerializer]
public class EventStoreEventStorageException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="EventStoreEventStorageException"/>.
    /// </summary>
    public EventStoreEventStorageException()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="EventStoreEventStorageException"/>.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public EventStoreEventStorageException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="EventStoreEventStorageException"/>.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public EventStoreEventStorageException(string message, Exception inner) : base(message, inner)
    {
    }
}