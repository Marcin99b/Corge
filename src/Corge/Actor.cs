namespace Corge;

public record Actor(string Name, string Color, Guid StartId)
{
    public Guid Id { get; } = Guid.NewGuid();
}
