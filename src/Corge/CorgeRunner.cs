using Autofac;
using Corge.Configuration;
using Corge.Events;
using Corge.Events.Handlers;

namespace Corge;

public class CorgeRunner(GameStorage storage)
{
    public static CorgeRunner FromStorage(GameStorage storage) => new (storage);

    public void Run()
    {
        var builder = new ContainerBuilder();

        _ = builder.RegisterType<DialogueHandler>()
            .As<IDialogueHandler>()
            .SingleInstance();

        _ = builder.RegisterType<EventBus>()
            .As<IEventBus>()
            .SingleInstance();

        _ = builder.RegisterInstance(storage)
            .AsSelf()
            .SingleInstance();

        _ = builder.RegisterType<PlayerStartedConversationEventHandler>().AsSelf().SingleInstance();
        _ = builder.RegisterType<SentenceFinishedEventHandler>().AsSelf().SingleInstance();
        _ = builder.RegisterType<PlayerDecidedEventHandler>().AsSelf().SingleInstance();
        _ = builder.RegisterType<DialogueItemSelectedEventHandler>().AsSelf().SingleInstance();

        var container = builder.Build();
        var bus = container.Resolve<IEventBus>();

        bus.Subscribe<PlayerStartedConversationEvent, PlayerStartedConversationEventHandler>();
        bus.Subscribe<SentenceFinishedEvent, SentenceFinishedEventHandler>();
        bus.Subscribe<PlayerDecidedEvent, PlayerDecidedEventHandler>();
        bus.Subscribe<DialogueItemSelectedEvent, DialogueItemSelectedEventHandler>();

        bus.Publish(new PlayerStartedConversationEvent(storage.Actors.First().Id));

        _ = Console.ReadKey();
    }
}

public record ItemRelation(Guid ActorId, Guid FromId, Guid ContinueId);

public interface IDialogueItem
{
    Guid Id { get; }
}

public record Sentence(string Text) : IDialogueItem
{
    public Guid Id { get; } = Guid.NewGuid();
}

public record Decision(DecisionOption[] Options) : IDialogueItem
{
    public Guid Id { get; } = Guid.NewGuid();
}

public record ExitDialogue : IDialogueItem
{
    public Guid Id { get; } = Guid.NewGuid();
}

public record DecisionOption(string Text, bool HideAfterUsed) : IDialogueItem
{
    public Guid Id { get; } = Guid.NewGuid();
}

public record Actor(string Name, string Color, Guid StartId)
{
    public Guid Id { get; } = Guid.NewGuid();
}
