using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NEventStore.Cqrs.Tests.Impl
{
    public class DictionaryBasedDependencyResolver : IDependencyResolver
    {
        private readonly ConcurrentDictionary<Type, Func<object>> container = new ConcurrentDictionary<Type, Func<object>>();
        private readonly ConcurrentDictionary<Type, List<Func<object>>> listContainer = new ConcurrentDictionary<Type, List<Func<object>>>();

        public T Resolve<T>()
        {
            Func<object> ctor;
            return container.TryGetValue(typeof(T), out ctor) ? (T)ctor() : default(T);
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            List<Func<object>> list;
            return listContainer.TryGetValue(typeof(T), out list) ? list.Select(e => (T)e()).ToArray() : new T[0];
        }

        public IDependencyResolver Register<T>(Func<IDependencyResolver, T> func)
        {
            container[typeof(T)] = () => func(this);

            return this;
        }

        public IDependencyResolver RegisterSingleton<T>(Func<IDependencyResolver, T> func)
        {
            object sync = new object();
            object instance = null;
            container[typeof(T)] = () =>
            {
                lock (sync)
                    instance = instance ?? func(this);
                return (T)instance;
            };

            return this;
        }

        public Type[] GetRegisteredCommandHandlers()
        {
            return new Type[0];
        }

        public Type[] GetRegisteredEventHandlers(bool excludeProjections = true)
        {
            return new Type[0];
        }

        public DictionaryBasedDependencyResolver RegisterAll<T>(Func<T> handler, Type of) where T : class
        {
            var handlerTypes = typeof(T).GetInterfaces().Where(e => e.GetGenericTypeDefinition() == of).ToArray();

            foreach (var handlerType in handlerTypes)
                lock (listContainer)
                {
                    List<Func<object>> list;
                    if (!listContainer.TryGetValue(handlerType, out list))
                    {
                        list = new List<Func<object>>();
                        listContainer[handlerType] = list;
                    }
                    list.Add(handler);
                }

            return this;
        }
    }
}
