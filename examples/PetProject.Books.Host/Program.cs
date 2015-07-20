using Autofac;
using EventStream.Projector;
using Mono.Unix;
using Mono.Unix.Native;
using Nancy.Hosting.Self;
using NEventStore;
using NEventStore.Cqrs;
using NEventStore.Cqrs.EventStream.Projector.NEventStore;
using NEventStore.Cqrs.Impl;
using NEventStore.Cqrs.MongoDb;
using NEventStore.Cqrs.MongoDb.EventStream.Projector.MongoDb;
using NEventStore.Serialization;
using PetProject.Books.Host.Impl;
using PetProject.Books.Host.Impl.ProjectorIntegration;
using PetProject.Books.Projections;
using PetProject.Books.Shared.Events;
using System;
using System.Linq;
using System.Threading;

namespace PetProject.Books.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            string writeConnectionStringName = "WriteDb";
            string readConnectionStringName = "ReadDb";
            var appAssemblies = new[] { typeof(BookRegistered).Assembly, typeof(IBookProjection).Assembly };

            var builder = new ContainerBuilder();
            builder.RegisterModule(new ConventionBasedRegistration(appAssemblies, readConnectionStringName));
            builder.RegisterModule(new ProjectorRegistration(readConnectionStringName));
            var ioc = builder.Build();

            var es = Wireup.Init()
                .UsingMongoPersistence(writeConnectionStringName, new DocumentObjectSerializer())
                .InitializeStorageEngine()
                .UsingJsonSerialization()
                .UsingEventUpconversion()
                .WithConvertersFrom(appAssemblies)
                .UsingAsynchronousDispatchScheduler()
                //.UsingSynchronousDispatchScheduler()
                .UsingCqrs(new AutofacDependencyResolver(ioc), ioc.Resolve<IProjector>())
                .WithAggregateFactory(c => new AggregateFactoryHeaderBased(appAssemblies))
                .WithLogger(_ => new Log4NetLogger())
                .WithMongo(writeConnectionStringName, appAssemblies)
                .Build();

            ioc.Resolve<IProjectionRebuild>().Start(new NEventStoreStream(es), CancellationToken.None);

            var uri = "http://localhost:8080";
            Console.WriteLine(uri);
            // initialize an instance of NancyHost (found in the Nancy.Hosting.Self package)
            var host = new NancyHost(new Uri(uri), new NancyBootstrapper(ioc));
            host.Start();  // start hosting

            //Under mono if you daemonize a process a Console.ReadLine will cause an EOF 
            //so we need to block another way
            if (args.Any(s => s.Equals("-d", StringComparison.CurrentCultureIgnoreCase)))
            {
                Thread.Sleep(Timeout.Infinite);
            }
            else
            {
                Console.ReadKey();

                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    var signals = new[] {
						new UnixSignal (Signum.SIGINT),
						new UnixSignal (Signum.SIGTERM),
					};

                    // Wait for a unix signal
                    for (bool exit = false; !exit; )
                    {
                        int id = UnixSignal.WaitAny(signals);

                        if (id >= 0 && id < signals.Length)
                        {
                            if (signals[id].IsSet)
                                exit = true;
                        }
                    }
                }
            }

            es.Dispose();
            host.Stop();  // stop hosting
        }
    }
}