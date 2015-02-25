using NEventStore.Cqrs.Messages;

namespace NEventStore.Cqrs
{
    public interface IHandler<in T> where T : class, IMessage
    {
        void Handle(T msg);
    }
}
