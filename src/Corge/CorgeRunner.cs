using Spectre.Console;

namespace Corge;
public class CorgeRunner
{
    public void Run()
    {
        var bus = new EventBus();

        bus.Subscribe<PlayerDecidedEvent>(x => Console.WriteLine(x.Decision));

        var player = new Actor("Player", "gold3_1");
        var dialogHandler = new DialogHandler(bus, player);


        var stefan = new Actor("Stefan", "darkorange3");

        var sentence_1 = new Sentence(stefan, "Cześć");
        var sentence_2 = new Sentence(stefan, "Myślałem że nie żyjesz");

        var option_1 = new DecisionOption("Tak, też tak myślałem");
        var option_2 = new DecisionOption("Kim jesteś?");
        var option_3 = new DecisionOption("Gdzie ja jestem?");

        var decision_1 = new Decision(
            [
                option_1,
                option_2,
                option_3,
            ]);

        var sentenceRelations = new ItemRelation[] 
        {
            new (sentence_1.Id, sentence_2.Id),
            new (sentence_2.Id, decision_1.Id),
        };

        dialogHandler.Say(stefan, "Cześć, przyjacielu");
        dialogHandler.Say(stefan, "pewnie się zastanawiasz, jak się tutaj znalazłeś");

        dialogHandler.Ask(["Kim jesteś?", "Co tutaj robie?", "Cześć"]);

        Console.ReadKey();
    }
}

public record ItemRelation(Guid FromId, Guid ContinueId, Action? Action = null);

public interface IDialogueItem
{
    Guid Id { get; }
}

public record Sentence(Actor Actor, string Text) : IDialogueItem
{
    public Guid Id { get; } = Guid.NewGuid();
}

public record Decision(DecisionOption[] Options) : IDialogueItem
{
    public Guid Id { get; } = Guid.NewGuid();
}

public record DecisionOption(string Text);

public class Actor(string name, string color)
{
    public string Name { get; } = name;
    public string Color { get; } = color;
}

public class DialogHandler(EventBus bus, Actor player)
{
    public void Say(Actor actor, string message)
    {
        AnsiConsole.Markup($"[{actor.Color}]{actor.Name}: [/][grey84]{message}[/]\n");
    }

    public string Ask(string[] decisions)
    {
        var response = AnsiConsole.Prompt(new SelectionPrompt<string>().AddChoices(decisions));
        this.Say(player, response);
        bus.Publish(new PlayerDecidedEvent(response));
        return response;
    }
}

public interface IEvent
{
}

public record PlayerDecidedEvent(string Decision) : IEvent;
