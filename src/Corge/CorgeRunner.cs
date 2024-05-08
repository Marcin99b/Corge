namespace Corge;
public class CorgeRunner
{
    public void Run()
    {
        var dialogHandler = new DialogHandler();
        var startActor = new Actor("Stefan");

        dialogHandler.Say(startActor, "Cześć, przyjacielu");
        dialogHandler.Say(startActor, "pewnie się zastanawiasz jak się tutaj znalazłeś");

        dialogHandler.ShowDecision(["Kim jesteś?", "Co tutaj robie?", "Cześć"]);

        Console.ReadKey();
    }
}

public class Actor(string name)
{
    public string Name { get; } = name;
}

public class DialogHandler
{
    public void Say(Actor actor, string message)
    {
        ConsoleWriter.WriteActorLine(actor, message);
    }

    public void ShowDecision(string[] decisions)
    {
        ConsoleWriter.WriteDecisions(decisions);
    }

    public void OnReply(Actor actor)
    {

    }
}

public static class ConsoleWriter
{
    public static void WriteActorLine(Actor actor, string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        SlowWrite($"{actor.Name}: ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        SlowWrite(message + "\n");
    }

    public static void WriteDecisions(string[] decisions)
    {
        Console.ForegroundColor = ConsoleColor.White;
        for (var i = 0; i < decisions.Length; i++)
        {
            SlowWrite($"[{i+1}] {decisions[i]}\n");
        }
    }

    private static void SlowWrite(string message)
    {
        foreach (var letter in message)
        {
            Console.Write(letter);
            Thread.Sleep(15);
        }
    }
}