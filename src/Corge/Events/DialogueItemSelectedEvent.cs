namespace Corge.Events;

public record DialogueItemSelectedEvent(Guid ActorId, Guid ItemId) : IEvent;