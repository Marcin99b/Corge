using Spectre.Console;

namespace Corge;
public class CorgeRunner
{
    public void Run()
    {
        var bus = new EventBus();

        var sentence_1 = new Sentence("Cześć");
        var sentence_2 = new Sentence("Myślałem że nie żyjesz");

        var option_1 = new DecisionOption("Tak, też tak myślałem");
        var option_2 = new DecisionOption("Kim jesteś?");
        var option_3 = new DecisionOption("Gdzie ja jestem?");

        var decision_1 = new Decision(
            [
                option_1,
                option_2,
                option_3,
            ]);

        var dialogueItems = new IDialogueItem[] 
        {
            sentence_1,
            sentence_2,
            decision_1
        };

        var stefan = new Actor("Stefan", "darkorange3", sentence_1.Id);

        var sentenceRelations = new ItemRelation[] 
        {
            new (stefan.Id, sentence_1.Id, sentence_2.Id),
            new (stefan.Id, sentence_2.Id, decision_1.Id),
        };

        var actors = new Actor[]
        {
            stefan
        };

        var dialogHandler = new DialogueHandler(bus);

        bus.Subscribe<PlayerStartedConversationEvent>(x => 
        {
            var actor = actors.First(actor => actor.Id == x.ActorId);
            var dialogueItem = dialogueItems.First(d => d.Id == actor.StartId);
        });

        bus.Subscribe<SentenceFinishedEvent>(x => 
        {
            var relation = sentenceRelations.First(s => s.FromId == x.SentenceId);
            var actor = actors.First(a => a.Id == relation.ActorId);
            var nextItem = dialogueItems.First(i => i.Id == relation.ContinueId);
        });

        bus.Publish(new PlayerStartedConversationEvent(stefan.Id));

        Console.ReadKey();
    }
}

public record ItemRelation(Guid ActorId, Guid FromId, Guid ContinueId, Action? Action = null);

public interface IDialogueItem
{
    Guid Id { get; }
}

public record Sentence(string Text) : IDialogueItem
{
    public Guid Id { get; } = Guid.NewGuid();
}

public record Decision(DecisionOption[] Options) : IDialogueItem
{
    public Guid Id { get; } = Guid.NewGuid();
}

public record DecisionOption(string Text)
{
    public Guid Id { get; } = Guid.NewGuid();
}

public record Actor(string Name, string Color, Guid StartId)
{
    public Guid Id { get; } = Guid.NewGuid();
}

public class DialogueHandler(EventBus bus) 
{
    public void Say(Actor actor, Sentence sentence)
    {
        AnsiConsole.Markup($"[{actor.Color}]{actor.Name}: [/][grey84]{sentence.Text}[/]\n");
    }

    public string Ask(Decision decision)
    {
        var response = AnsiConsole.Prompt(new SelectionPrompt<string>().AddChoices(decision.Options.Select(x => x.Text)));
        var option = decision.Options.First(x => x.Text == response);
        bus.Publish(new PlayerDecidedEvent(decision.Id, option.Id));
        return response;
    }
}

public interface IEvent
{
}

public record PlayerStartedConversationEvent(Guid ActorId) : IEvent;
public record PlayerDecidedEvent(Guid DecisionId, Guid OptionId) : IEvent;
public record SentenceFinishedEvent(Guid SentenceId) : IEvent;
