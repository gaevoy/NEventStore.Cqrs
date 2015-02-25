using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NEventStore.Cqrs.Messages;

namespace NEventStore.Cqrs.Impl
{
    public static class MessageHandlerUtils
    {
        public static Dictionary<Type/*Handler type*/, Dictionary<Type/*Message type*/, MethodInfo>> Cache = new Dictionary<Type, Dictionary<Type, MethodInfo>>();

        public static void Handle(object handler, IMessage message)
        {
            Type messageType = message.GetType();
            var method = handler.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Single(e => e.Name == "Handle" && e.GetParameters().Count() == 1 && e.GetParameters()[0].ParameterType == messageType);
            method.Invoke(handler, new object[] { message });
        }

        public static void HandleIfPossible(object handler, IMessage message)
        {
            Type handerType = handler.GetType();
            Type messageType = message.GetType();
            var method = GetMethod(handerType, messageType);
            if (method != null)
            {
                method.Invoke(handler, new object[] { message });
            }
        }

        private static MethodInfo GetMethod(Type handlerType, Type messageType)
        {
            Dictionary<Type, MethodInfo> handlerMethods;
            if (!Cache.TryGetValue(handlerType, out handlerMethods))
            {
                handlerMethods = handlerType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(e => e.Name == "Handle")
                    .Select(e => new { Method = e, Parameters = e.GetParameters() })
                    .Where(e => e.Parameters.Length == 1)
                    .ToDictionary(e => e.Parameters[0].ParameterType, e => e.Method);
                Cache[handlerType] = handlerMethods;
            }

            MethodInfo method;
            handlerMethods.TryGetValue(messageType, out method);
            return method;
        }
    }
}
