namespace Corge.Events.Handlers;

internal class DialogueItemSelectedEventHandler(
    IEventBus bus,
    IDialogueHandler dialogueHandler,
    Storage storage)
    : IEventHandler<DialogueItemSelectedEvent>
{
    public void Execute(DialogueItemSelectedEvent evnt)
    {
        var actor = storage.Actors.First(a => a.Id == evnt.ActorId);
        var nextItem = storage.DialogueItems.First(i => i.Id == evnt.ItemId);

        if (nextItem is Sentence sentence)
        {
            dialogueHandler.Say(actor, sentence);
            bus.Publish(new SentenceFinishedEvent(sentence.Id));
        }
        else if (nextItem is Decision decision)
        {
            var option = dialogueHandler.Ask(decision);
            bus.Publish(new PlayerDecidedEvent(option.Id));
        }
        else if (nextItem is ExitDialogue exit)
        {

        }
    }
}
