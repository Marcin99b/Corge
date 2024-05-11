namespace Corge.Events.Handlers;

public interface IEventHandler<T> where T : class, IEvent
{
    public void Execute(T evnt);
}
