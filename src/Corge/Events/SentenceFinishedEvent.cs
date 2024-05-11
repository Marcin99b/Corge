namespace Corge.Events;

public record SentenceFinishedEvent(Guid SentenceId) : IEvent;
