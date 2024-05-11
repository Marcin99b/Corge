using Autofac;
using Corge.Events;
using Corge.Events.Handlers;

namespace Corge;

public class CorgeBuilder
{
    private readonly Storage storage = new();

    public ActorDialogueBuilder SetupActorDialogue(string name, string color) => new(this.storage, this, name, color);

    public Storage Build() => this.storage;

    public class ActorDialogueBuilder(Storage s, CorgeBuilder b, string name, string color)
    {
        public SentenceCreatedBuilder StartFromSentence(string text)
        {
            var sentence = new Sentence(text);
            var actor = new Actor(name, color, sentence.Id);
            s.Actors.Add(actor);
            s.DialogueItems.Add(sentence);

            return new SentenceCreatedBuilder(s, this, actor, sentence);
        }

        public CorgeBuilder Build() => b;
    }

    public class SentenceCreatedBuilder(Storage s, ActorDialogueBuilder b, Actor a, Sentence sentence)
    {
        public SentenceCreatedBuilder ContinueWithSentence(string text)
        {
            var newSentence = new Sentence(text);
            s.DialogueItems.Add(newSentence);
            s.SentenceRelations.Add(new ItemRelation(a.Id, sentence.Id, newSentence.Id));

            return new SentenceCreatedBuilder(s, b, a, newSentence);
        }

        public DecisionBuilder ContinueWithDecision() => new(s, b, a, sentence, []);

        public ActorDialogueBuilder Build() => b;
    }

    public class DecisionBuilder(Storage s, ActorDialogueBuilder b, Actor a, Sentence fromSentence, List<DecisionOption> currentOptions)
    {
        public OptionBuilder AddOption(string text, bool hideAfterUsed)
        {
            var option = new DecisionOption(text, hideAfterUsed);
            currentOptions.Add(option);
            return new OptionBuilder(s, this, option, a);
        }

        public ActorDialogueBuilder Build()
        {
            var d = new Decision(currentOptions.ToArray());
            s.DialogueItems.Add(d);
            s.SentenceRelations.Add(new ItemRelation(a.Id, fromSentence.Id, d.Id));

            var answersWithoutContinuation = s.SentenceRelations
                .Where(x => currentOptions.Any(c => c.Id == x.FromId))
                .Select(x => x.ContinueId)
                .Where(x => !s.SentenceRelations.Any(r => r.FromId == x))
                .ToArray();
            foreach (var answerId in answersWithoutContinuation)
            {
                s.SentenceRelations.Add(new ItemRelation(a.Id, answerId, d.Id));
            }

            return b;
        }
    }

    public class OptionBuilder(Storage s, DecisionBuilder b, DecisionOption option, Actor a)
    {
        public SentenceAnswerCreatedBuilder SetAnswer(string text)
        {
            var newSentence = new Sentence(text);
            s.DialogueItems.Add(newSentence);
            s.SentenceRelations.Add(new ItemRelation(a.Id, option.Id, newSentence.Id));
            return new SentenceAnswerCreatedBuilder(s, b, a, newSentence);
        }
    }

    public class SentenceAnswerCreatedBuilder(Storage s, DecisionBuilder b, Actor a, Sentence sentence)
    {
        public SentenceAnswerCreatedBuilder ContinueWithSentence(string text)
        {
            var newSentence = new Sentence(text);
            s.DialogueItems.Add(newSentence);
            s.SentenceRelations.Add(new ItemRelation(a.Id, sentence.Id, newSentence.Id));
            return new SentenceAnswerCreatedBuilder(s, b, a, newSentence);
        }

        public DecisionBuilder Build() => b;
    }
}

public class CorgeRunner(Storage storage)
{
    public static CorgeRunner FromStorage(Storage storage) => new (storage);

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


public class Storage
{
    public List<IDialogueItem> DialogueItems { get; set; } = [];
    public List<Guid> UsedOptions { get; set; } = [];
    public List<ItemRelation> SentenceRelations { get; set; } = [];
    public List<Actor> Actors { get; set; } = [];
}

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

public record DecisionOption(string Text, bool HideAfterUsed)
{
    public Guid Id { get; } = Guid.NewGuid();
}

public record Actor(string Name, string Color, Guid StartId)
{
    public Guid Id { get; } = Guid.NewGuid();
}
