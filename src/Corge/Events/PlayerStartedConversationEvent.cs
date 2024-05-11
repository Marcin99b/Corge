namespace Corge.Events;

public record PlayerStartedConversationEvent(Guid ActorId) : IEvent;
