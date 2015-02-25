using System;
using System.Collections.Generic;
using Autofac;
using NEventStore.Cqrs;

namespace PetProject.Books.Host.Impl
{
    public class AutofacDependencyResolver : IDependencyResolver
    {
        private readonly IContainer container;

        public AutofacDependencyResolver(IContainer container)
        {
            this.container = container;
        }

        public T Resolve<T>()
        {
            return container.Resolve<T>();
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return container.Resolve<IEnumerable<T>>();
        }

        public IDependencyResolver Register<T>(Func<IDependencyResolver, T> func)
        {
            var newBuilder = new ContainerBuilder();
            newBuilder.Register(c => func(this)).As<T>().ExternallyOwned();
            newBuilder.Update(container);

            return this;
        }

        public IDependencyResolver RegisterSingleton<T>(Func<IDependencyResolver, T> func)
        {
            var newBuilder = new ContainerBuilder();
            newBuilder.Register(c => func(this)).As<T>().SingleInstance().ExternallyOwned();
            newBuilder.Update(container);

            return this;
        }
    }
}
