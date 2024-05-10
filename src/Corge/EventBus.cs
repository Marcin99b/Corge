namespace Corge;

public class EventBus
{
    private readonly Dictionary<Type, List<Action<dynamic>>> subscriptions = new();

    public void Subscribe<T>(Action<T> action) 
        where T : class, IEvent
    {
        var t = typeof(T);
        Action<dynamic> dyn = x => action(x);
        if (this.subscriptions.ContainsKey(t))
        {
            this.subscriptions[t].Add(dyn);
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