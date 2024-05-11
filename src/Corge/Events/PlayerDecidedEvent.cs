namespace Corge.Events;

public record PlayerDecidedEvent(Guid OptionId) : IEvent;
