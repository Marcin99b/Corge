namespace Corge.Configuration;

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

        return (GameStorage)obj;
    }
}