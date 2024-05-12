namespace Corge;

public record DecisionOption(string Text, bool HideAfterUsed) : IDialogueItem
{
    public Guid Id { get; } = Guid.NewGuid();
}
