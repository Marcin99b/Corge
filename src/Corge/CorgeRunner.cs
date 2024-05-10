using Spectre.Console;

namespace Corge;
public class CorgeRunner
{
    public void Run()
    {
        var player = new Actor("Player", "gold3_1");
        var dialogHandler = new DialogHandler(player);
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

public class DialogHandler(Actor player)
{
    public void Say(Actor actor, string message)
    {
        AnsiConsole.Markup($"[{actor.Color}]{actor.Name}: [/][grey84]{message}[/]\n");
    }

    public string Ask(string[] decisions)
    {
        var response = AnsiConsole.Prompt(new SelectionPrompt<string>().AddChoices(decisions));
        this.Say(player, response);
        return response;
    }
}