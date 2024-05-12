namespace Corge;

public record Sentence(string Text) : IDialogueItem
{
    public Guid Id { get; } = Guid.NewGuid();
}
