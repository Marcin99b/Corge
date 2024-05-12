namespace Corge;

public record ExitDialogue : IDialogueItem
{
    public Guid Id { get; } = Guid.NewGuid();
}
