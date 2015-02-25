using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NEventStore.Cqrs.Tests.Mocks
{
    public class DependencyResolverMock : IDependencyResolver
    {
        readonly ConcurrentDictionary<Type, List<Func<object>>> constructors = new ConcurrentDictionary<Type, List<Func<object>>>();
        public T Resolve<T>()
        {
            return constructors[typeof(T)].Select(ctor => ctor()).Cast<T>().First();
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return constructors[typeof(T)].Select(ctor => ctor()).Cast<T>();
        }

        public IDependencyResolver Register<T>(Func<IDependencyResolver, T> func)
        {
            var type = typeof(T);
            if (!constructors.ContainsKey(type)) constructors[type] = new List<Func<object>>();
            constructors[type].Add(() => func(this));

            return this;
        }

        public IDependencyResolver RegisterSingleton<T>(Func<IDependencyResolver, T> func)
        {
            throw new NotImplementedException();
        }

        public List<Type> GetRegisteredCommandHendlers()
        {
            throw new NotImplementedException();
        }

        public List<Type> GetRegisteredEventHendlers()
        {
            throw new NotImplementedException();
        }
    }
}