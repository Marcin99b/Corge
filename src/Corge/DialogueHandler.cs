using Spectre.Console;

namespace Corge;

public interface IDialogueHandler
{
    DecisionOption Ask(Decision decision);
    void Say(Actor actor, Sentence sentence);
}

public class DialogueHandler(GameStorage storage) : IDialogueHandler
{
    public void Say(Actor actor, Sentence sentence)
    {
        AnsiConsole.Markup($"[{actor.Color}]{actor.Name}: [/][grey84]{sentence.Text}[/]\n");
    }

    public DecisionOption Ask(Decision decision)
    {
        AnsiConsole.WriteLine();

        var optionsToShow = decision.Options
            .Where(x => !x.HideAfterUsed || !storage.UsedOptions.Contains(x.Id))
            .Select(x => x.Text);

        var response = optionsToShow.Count() switch
        {
            var x when x > 1 => AnsiConsole.Prompt(new SelectionPrompt<string>().AddChoices(optionsToShow)),
            1 => optionsToShow.First(),
            _ => throw new NotImplementedException(),
        };

        AnsiConsole.Markup($"[gold3_1]{storage.PlayerName}: [/][grey84]{response}[/]\n");
        var option = decision.Options.First(x => x.Text == response);
        return option;
    }
}
