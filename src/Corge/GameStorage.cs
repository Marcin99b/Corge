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