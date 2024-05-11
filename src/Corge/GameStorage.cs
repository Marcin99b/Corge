namespace Corge;

public class GameStorage
{
    internal string PlayerName { get; set; } = "You";
    internal List<IDialogueItem> DialogueItems { get; set; } = [];
    internal List<Guid> UsedOptions { get; set; } = [];
    internal List<ItemRelation> SentenceRelations { get; set; } = [];
    internal List<Actor> Actors { get; set; } = [];

    public DialogueSetupHandler<GameStorage> Actor(string name, string color, string defaultSentence)
    {
        var startSentence = new Sentence(defaultSentence);
        var actor = new Actor(name, color, startSentence.Id);

        this.Actors.Add(actor);
        this.DialogueItems.Add(startSentence);

        return new DialogueSetupHandler<GameStorage>(this, this, actor, startSentence);
    }
}

public class DialogueSetupHandler<T>(T prevObject, GameStorage storage, Actor actor, IDialogueItem previous)
{
    public DecisionBuilder<DialogueSetupHandler<T>> Decision()
    {
        return new DecisionBuilder<DialogueSetupHandler<T>>(this, storage, actor, previous);
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

public class DecisionBuilder<T>(T prevObject, GameStorage storage, Actor actor, IDialogueItem previous)
{
    private readonly List<DecisionOption> options = new();

    public DialogueSetupHandler<DecisionBuilder<T>> AddOption(string text, bool hideAfterUser)
    {
        var option = new DecisionOption(text, hideAfterUser);
        this.options.Add(option);
        return new DialogueSetupHandler<DecisionBuilder<T>>(this, storage, actor, option);
    }

    public T Return()
    {
        var decision = new Decision(this.options.ToArray());
        storage.DialogueItems.Add(decision);
        storage.SentenceRelations.Add(new ItemRelation(actor.Id, previous.Id, decision.Id));

        foreach (var answerId in this.FindNotEndingDialoguesStartedFromCurrentDecision(decision))
        {
            storage.SentenceRelations.Add(new ItemRelation(actor.Id, answerId, decision.Id));
        }


        return prevObject;
    }

    private IEnumerable<Guid> FindNotEndingDialoguesStartedFromCurrentDecision(Decision decision)
    {
        var isExit = (Guid x) => storage.DialogueItems.First(i => i.Id == x) is ExitDialogue;

        var sentencesStartedFromCurrentOptions = storage
            .SentenceRelations
            .Where(x => this.options.Any(c => c.Id == x.FromId))
            .ToArray();
        foreach (var sentence in sentencesStartedFromCurrentOptions)
        {
            var history = new List<Guid>() { decision.Id };

            var current = sentence.ContinueId;
            while (true)
            {
                if (history.Contains(current))
                {
                    break;
                }

                history.Add(current);
                if (isExit(current))
                {
                    break;
                }

                var nextItem = storage.SentenceRelations.FirstOrDefault(x => x.FromId == current);
                if (nextItem == null)
                {
                    yield return current;
                    break;
                }
                else
                {
                    current = nextItem!.ContinueId;
                }
            }
        }
    }

    public GameStorage Build()
    {
        dynamic obj = this.Return()!;
        while (obj is not GameStorage) 
        {
            obj = obj!.Return();
        }

        return (GameStorage) obj;
    }
}