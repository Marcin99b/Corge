namespace Corge.Events.Handlers;

internal class PlayerDecidedEventHandler(IEventBus bus, GameStorage storage) : IEventHandler<PlayerDecidedEvent>
{
    public void Execute(PlayerDecidedEvent evnt)
    {
        storage.UsedOptions.Add(evnt.OptionId);
        var relation = storage.SentenceRelations.First(s => s.FromId == evnt.OptionId);
        bus.Publish(new DialogueItemSelectedEvent(relation.ActorId, relation.ContinueId));
    }
}
