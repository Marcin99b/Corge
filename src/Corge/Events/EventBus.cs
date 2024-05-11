using Autofac;
using Corge.Events.Handlers;

namespace Corge.Events;

public interface IEventBus
{
    void Publish(IEvent @event);
    void Subscribe<E, H>()
        where E : class, IEvent
        where H : class, IEventHandler<E>;
}

public class EventBus(ILifetimeScope scope) : IEventBus
{
    private readonly Dictionary<Type, List<Action<dynamic>>> subscriptions = [];

    public void Subscribe<E, H>()
        where E : class, IEvent
        where H : class, IEventHandler<E>
    {
        var t = typeof(E);
        var handler = scope.Resolve<H>();
        void dyn(dynamic x) => handler.Execute(x);
        if (this.subscriptions.TryGetValue(t, out var value))
        {
            value.Add(dyn);
        }
        else
        {
            this.subscriptions.Add(t, [dyn]);
        }
    }

    public void Publish(IEvent @event)
    {
        if (this.subscriptions.TryGetValue(@event.GetType(), out var list))
        {
            foreach (var item in list)
            {
                item(@event);
            }
        }
    }
}


