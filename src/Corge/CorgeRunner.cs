using Spectre.Console;

namespace Corge;
public class CorgeRunner
{
    public void Run()
    {
        var bus = new EventBus();

        bus.Subscribe<PlayerDecidedEvent>(x => Console.WriteLine(x.Decision));
        bus.Subscribe<PlayerDecidedEvent>(x => Console.WriteLine(x.Decision));

        var player = new Actor("Player", "gold3_1");
        var dialogHandler = new DialogHandler(bus, player);
        var stefan = new Actor("Stefan", "darkorange3");

        dialogHandler.Say(stefan, "Cześć, przyjacielu");
        dialogHandler.Say(stefan, "pewnie się zastanawiasz, jak się tutaj znalazłeś");

        dialogHandler.Ask(["Kim jesteś?", "Co tutaj robie?", "Cześć"]);

        Console.ReadKey();
    }
}

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

public class EventBus
{
    private readonly Dictionary<Type, List<Action<dynamic>>> subscriptions = new();

    public void Subscribe<T>(Action<T> action) 
        where T : class, IEvent
    {
        var t = typeof(T);
        Action<dynamic> dyn = x => action(x);
        if (this.subscriptions.ContainsKey(t))
        {
            this.subscriptions[t].Add(dyn);
        } 
        else
        {
            this.subscriptions.Add(t, [dyn]);
        }
    }

    public void Publish(IEvent @event)
    {
        if (this.subscriptions.TryGetValue(@event.GetType(), out var list))
        {
            foreach (var item in list)
            {
                item(@event);
            }
        }
    }
}