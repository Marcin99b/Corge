using Autofac;
using Corge.Events;
using Corge.Events.Handlers;

namespace Corge;

public class CorgeBuilder
{
    private readonly Storage storage = new ();

    public ActorDialogueBuilder SetupActorDialogue(string name, string color)
    {
        return new ActorDialogueBuilder(this.storage, this, name, color);
    }

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

        public DecisionBuilder ContinueWithDecision()
        {
            return new DecisionBuilder(s, b, a);
        }

        public ActorDialogueBuilder Build() => b;
    }

    public class DecisionBuilder(Storage s, ActorDialogueBuilder b, Actor a)
    {
        public OptionBuilder AddOption(string text, bool hideAfterUsed)
        {
            var option = new DecisionOption(text, hideAfterUsed);
            return new OptionBuilder(s, this, option, a);
        }

        public ActorDialogueBuilder Build() => b;
    }

    public class OptionBuilder(Storage s, DecisionBuilder b, DecisionOption option, Actor a)
    {
        public SentenceAnswerCreatedBuilder SetAnswer(string text)
        {
            var sentence = new Sentence(text);
            return new SentenceAnswerCreatedBuilder(s, b, a, sentence);
        }
    }

    public class SentenceAnswerCreatedBuilder(Storage s, DecisionBuilder b, Actor a, Sentence sentence)
    {
        public SentenceAnswerCreatedBuilder ContinueWithSentence(string text)
        {
            var sentence = new Sentence(text);
            return new SentenceAnswerCreatedBuilder(s, b, a, sentence);
        }

        public DecisionBuilder Build() => b;
    }
}

public class CorgeRunner
{
    public void Run()
    {
        var sentence_1 = new Sentence("Cześć");
        var sentence_2 = new Sentence("Myślałem że nie żyjesz");

        var option_1 = new DecisionOption("Tak, też tak myślałem", true);
        var sentence_3 = new Sentence("Długo tu leżałeś");

        var option_2 = new DecisionOption("Kim jesteś?", false);
        var sentence_4 = new Sentence("Na razie nie mogę ci powiedzieć");

        var option_3 = new DecisionOption("Gdzie ja jestem?", true);
        var sentence_5 = new Sentence("Nawet gdybym ci powiedział, to byś nie zrozumiał");

        var decision_1 = new Decision(
            [
                option_1,
                option_2,
                option_3,
            ]);

        var dialogueItems = new IDialogueItem[]
        {
            sentence_1,
            sentence_2,
            decision_1,
            sentence_3,
            sentence_4,
            sentence_5,
        };

        var usedOptions = new List<Guid>();

        var stefan = new Actor("Stefan", "darkorange3", sentence_1.Id);

        var sentenceRelations = new ItemRelation[]
        {
            new (stefan.Id, sentence_1.Id, sentence_2.Id),
            new (stefan.Id, sentence_2.Id, decision_1.Id),

            new(stefan.Id, option_1.Id, sentence_3.Id),
            new(stefan.Id, option_2.Id, sentence_4.Id),
            new(stefan.Id, option_3.Id, sentence_5.Id),

            new(stefan.Id, sentence_3.Id, decision_1.Id),
            new(stefan.Id, sentence_4.Id, decision_1.Id),
            new(stefan.Id, sentence_5.Id, decision_1.Id),
        };

        var actors = new Actor[]
        {
            stefan
        };

        var builder = new ContainerBuilder();

        _ = builder.RegisterType<DialogueHandler>()
            .As<IDialogueHandler>()
            .SingleInstance();

        _ = builder.RegisterType<EventBus>()
            .As<IEventBus>()
            .SingleInstance();

        _ = builder.RegisterType<Storage>()
            .AsSelf()
            .SingleInstance();

        _ = builder.RegisterType<PlayerStartedConversationEventHandler>().AsSelf().SingleInstance();
        _ = builder.RegisterType<SentenceFinishedEventHandler>().AsSelf().SingleInstance();
        _ = builder.RegisterType<PlayerDecidedEventHandler>().AsSelf().SingleInstance();
        _ = builder.RegisterType<DialogueItemSelectedEventHandler>().AsSelf().SingleInstance();

        var container = builder.Build();

        var storage = container.Resolve<Storage>();
        storage.Actors = actors.ToList();
        storage.DialogueItems = dialogueItems.ToList();
        storage.SentenceRelations = sentenceRelations.ToList();
        storage.UsedOptions = usedOptions;

        var bus = container.Resolve<IEventBus>();

        bus.Subscribe<PlayerStartedConversationEvent, PlayerStartedConversationEventHandler>();
        bus.Subscribe<SentenceFinishedEvent, SentenceFinishedEventHandler>();
        bus.Subscribe<PlayerDecidedEvent, PlayerDecidedEventHandler>();
        bus.Subscribe<DialogueItemSelectedEvent, DialogueItemSelectedEventHandler>();

        bus.Publish(new PlayerStartedConversationEvent(stefan.Id));

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
