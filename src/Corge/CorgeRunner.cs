using Autofac;
using Corge.Configuration;
using Corge.Events;
using Corge.Events.Handlers;
using Newtonsoft.Json;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Net.Sockets;
using System.Text;

namespace Corge;

public class CorgeRunner(GameStorage storage)
{
    public static CorgeRunner FromStorage(GameStorage storage) => new (storage);

    public void Run()
    {
        using var log = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.Logtube()
            .CreateLogger();
        Log.Logger = log;

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

public static class LogtubeExtensions
{
    public static LoggerConfiguration Logtube(this LoggerSinkConfiguration sinkConfiguration, LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose)
    {
        return sinkConfiguration.Sink(new LogtubeSink(), restrictedToMinimumLevel);
    }
}

public class LogtubeSink : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        var json = JsonConvert.SerializeObject(logEvent);

        var client = new TcpClient("127.0.0.1", 8080);
        var stream = client.GetStream();

        var bytesToSend = Encoding.UTF8.GetBytes(json);
        stream.Write(bytesToSend, 0, bytesToSend.Length);

        client.Close();
    }
}
