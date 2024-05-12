namespace Corge;

public record Decision(DecisionOption[] Options) : IDialogueItem
{
    public Guid Id { get; } = Guid.NewGuid();
}
