namespace Corge.Events.Handlers;

internal class SentenceFinishedEventHandler(IEventBus bus, GameStorage storage) : IEventHandler<SentenceFinishedEvent>
{
    public void Execute(SentenceFinishedEvent evnt)
    {
        var relation = storage.SentenceRelations.First(s => s.FromId == evnt.SentenceId);
        bus.Publish(new DialogueItemSelectedEvent(relation.ActorId, relation.ContinueId));
    }
}
