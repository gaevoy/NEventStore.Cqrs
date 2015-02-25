using System;
using System.Collections.Generic;

namespace NEventStore.Cqrs
{
    public interface IDependencyResolver
    {
        IEnumerable<T> ResolveAll<T>();
        IDependencyResolver Register<T>(Func<IDependencyResolver, T> func);
    }
}
