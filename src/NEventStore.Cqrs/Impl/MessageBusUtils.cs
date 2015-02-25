using System;
using System.Reflection;
using NEventStore.Cqrs.Messages;

namespace NEventStore.Cqrs.Impl
{
    public static class MessageBusUtils
    {
        public static void Subscribe(ICommandBus bus, Type commandType, Action<IMessage> handler)
        {
            MethodInfo subscribeMethod = typeof(ICommandBus).GetMethod("Subscribe");
            MethodInfo genericSubscribeMethod = subscribeMethod.MakeGenericMethod(commandType);
            genericSubscribeMethod.Invoke(bus, new object[] { handler });
        }

        public static void Subscribe(IEventBus bus, Type eventType, Action<IMessage> handler)
        {
            MethodInfo subscribeMethod = typeof(IEventBus).GetMethod("Subscribe");
            MethodInfo genericSubscribeMethod = subscribeMethod.MakeGenericMethod(eventType);
            genericSubscribeMethod.Invoke(bus, new object[] { handler });
        }
    }
}
