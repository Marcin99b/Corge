using Autofac;
using Corge.Events;

namespace Corge;
public class CorgeRunner
{
    public void Run()
    {
        var bus = new EventBus();

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

        var storage = new Storage()
        {
            Actors = actors,
            DialogueItems = dialogueItems,
            SentenceRelations = sentenceRelations,
            UsedOptions = usedOptions
        };

        var dialogHandler = new DialogueHandler(bus, usedOptions);

        var builder = new ContainerBuilder();

        builder.RegisterType<DialogueHandler>()
            .As<IDialogueHandler>()
            .SingleInstance();

        builder.RegisterType<EventBus>()
            .As<IEventBus>()
            .SingleInstance();

        builder.RegisterType<Storage>()
            .AsSelf()
            .SingleInstance();

        bus.Subscribe<PlayerStartedConversationEvent>(x => 
        {
            var actor = actors.First(actor => actor.Id == x.ActorId);
            bus.Publish(new DialogueItemSelectedEvent(actor.Id, actor.StartId));
        });

        bus.Subscribe<SentenceFinishedEvent>(x => 
        {
            var relation = sentenceRelations.First(s => s.FromId == x.SentenceId);
            bus.Publish(new DialogueItemSelectedEvent(relation.ActorId, relation.ContinueId));
        });

        bus.Subscribe<PlayerDecidedEvent>(x => 
        {
            usedOptions.Add(x.OptionId);
            var relation = sentenceRelations.First(s => s.FromId == x.OptionId);
            bus.Publish(new DialogueItemSelectedEvent(relation.ActorId, relation.ContinueId));
        });

        bus.Subscribe<DialogueItemSelectedEvent>(x => 
        {
            var actor = actors.First(a => a.Id == x.ActorId);
            var nextItem = dialogueItems.First(i => i.Id == x.ItemId);

            if (nextItem is Sentence sentence)
            {
                dialogHandler.Say(actor, sentence);
                bus.Publish(new SentenceFinishedEvent(sentence.Id));
            }
            else if (nextItem is Decision decision)
            {
                var option = dialogHandler.Ask(decision);
                bus.Publish(new PlayerDecidedEvent(option.Id));
            }
        });

        bus.Publish(new PlayerStartedConversationEvent(stefan.Id));

        Console.ReadKey();
    }
}

public record ItemRelation(Guid ActorId, Guid FromId, Guid ContinueId, Action? Action = null);


public class Storage
{
    public IDialogueItem[] DialogueItems { get; set; }
    public List<Guid> UsedOptions { get; set; }
    public ItemRelation[] SentenceRelations { get; set; }
    public Actor[] Actors { get; set; }
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
