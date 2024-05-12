namespace Corge.Configuration;

public class DialogueSetupHandler<T>(T prevObject, GameStorage storage, Actor actor, IDialogueItem previous)
{
    public GameStorage Decision(
        params Action<DecisionBuilder<DialogueSetupHandler<T>>>[] decisionConfigs)
    {
        var builder = new DecisionBuilder<DialogueSetupHandler<T>>(this, storage, actor, previous);
        foreach (var item in decisionConfigs)
        {
            item(builder);
        }

        return builder.Build();
    }

    public DialogueSetupHandler<DialogueSetupHandler<T>> Sentence(string text)
    {
        var newSentence = new Sentence(text);
        storage.DialogueItems.Add(newSentence);
        storage.SentenceRelations.Add(new ItemRelation(actor.Id, previous.Id, newSentence.Id));

        return new DialogueSetupHandler<DialogueSetupHandler<T>>(this, storage, actor, newSentence);
    }

    public T ExitDialogue()
    {
        var exit = new ExitDialogue();
        storage.DialogueItems.Add(exit);
        storage.SentenceRelations.Add(new ItemRelation(actor.Id, previous.Id, exit.Id));

        return this.Return();
    }

    public DialogueSetupHandler<DialogueSetupHandler<T>> MultiSentence(params string[] texts)
    {
        var prev = previous;

        foreach (var text in texts)
        {
            var newSentence = new Sentence(text);
            storage.DialogueItems.Add(newSentence);
            storage.SentenceRelations.Add(new ItemRelation(actor.Id, prev.Id, newSentence.Id));
            prev = newSentence;
        }

        return new DialogueSetupHandler<DialogueSetupHandler<T>>(this, storage, actor, prev);
    }

    public T Return()
    {
        return prevObject;
    }

    public GameStorage Build()
    {
        dynamic obj = this.Return();
        while (obj is not GameStorage)
        {
            obj = obj?.Return();
        }

        return obj;
    }
}
