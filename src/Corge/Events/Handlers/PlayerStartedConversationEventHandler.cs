namespace Corge.Events.Handlers;

internal class PlayerStartedConversationEventHandler(IEventBus bus, Storage storage) : IEventHandler<PlayerStartedConversationEvent>
{
    public void Execute(PlayerStartedConversationEvent evnt)
    {
        var actor = storage.Actors.First(actor => actor.Id == evnt.ActorId);
        bus.Publish(new DialogueItemSelectedEvent(actor.Id, actor.StartId));
    }
}
