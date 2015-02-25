using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NEventStore.Cqrs.Impl;
using NEventStore.Cqrs.Impl.Utils.Tasks;
using NEventStore.Cqrs.Messages;
using NEventStore.Cqrs.Projections;
using NEventStore.Cqrs.Tests.Mocks;
using NEventStore.Persistence;
using NUnit.Framework;

namespace NEventStore.Cqrs.Tests.Utils.Tasks
{
    [TestFixture]
    class RebuildTaskTest
    {
        [Test]
        public void RebuildOnEmptyDb()
        {
            Given();
            history.AddRange(new[] { NewCommit(1), NewCommit(2), NewCommit(3), NewCommit(4), NewCommit(5) });

            // When
            NewRebuildTask().Run();

            // Then
            Assert.That(projection.Handled, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
        }

        [Test]
        public void RebuildOnEmptyDb2()
        {
            Given();
            history.AddRange(new[] { NewCommit(1), NewCommit(2), NewCommit(3), NewCommit(4), NewCommit(5) });
            versioningRepo.MarkAsModified<MyProjection>("Probably structure has been changed");

            // When
            NewRebuildTask().Run();

            // Then
            Assert.That(projection.Handled, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
        }

        [Test, ExpectedException(typeof(Exception))]
        public void CheckpointIsAbsent()
        {
            Given();
            history.AddRange(new[] { NewCommit(1), NewCommit(2), NewCommit(3), NewCommit(4), NewCommit(5) });
            checkpointRepo
                .MockLoad(Checkpoint.REGULAR, new Checkpoint(Checkpoint.REGULAR, ToGuid(6), new DateTime(2000, 1, 1)));

            // When
            NewRebuildTask().Run();
        }

        [Test]
        public void ProjectionHasModified()
        {
            Given();
            var commit33 = NewCommit(33);
            history.AddRange(new[] { NewCommit(11), NewCommit(22), commit33, NewCommit(44), NewCommit(55) });
            versioningRepo.MarkAsModified<MyProjection>("Probably structure has been changed");
            checkpointRepo
                .MockLoad(Checkpoint.REGULAR, new Checkpoint(Checkpoint.REGULAR, commit33));
            projection.Handled = new List<int> { 1, 1, 1, 1, 1, 1, 1 };

            // When
            NewRebuildTask().Run();

            // Then
            Assert.That(projection.Handled, Is.EqualTo(new[] { 11, 22, 33, 44, 55 }));
        }

        [Test]
        public void ProjectionHasFallenBehind()
        {
            Given();
            var commit333 = NewCommit(333);
            history.AddRange(new[] { NewCommit(111), NewCommit(222), commit333, NewCommit(444), NewCommit(555) });
            checkpointRepo
                .MockLoad(Checkpoint.REGULAR, new Checkpoint(Checkpoint.REGULAR, commit333));
            projection.Handled = new List<int> { 0, 0, 0 };

            // When
            NewRebuildTask().Run();

            // Then
            Assert.That(projection.Handled, Is.EqualTo(new[] { 0, 0, 0, 444, 555 }));
        }

        [Test]
        public void PauseDuringProjectionCatchUp()
        {
            Given();
            var commit4 = NewCommit(4);
            history.AddRange(new[] { NewCommit(1), NewCommit(2), NewCommit(3), commit4, NewCommit(5) });
            versioningRepo.MarkAsModified<MyProjection>("Probably structure has been changed");
            checkpointRepo.MockLoad(Checkpoint.REGULAR, new Checkpoint(Checkpoint.REGULAR, commit4));

            // When
            projection.OnHandled = val => { if (val == 2) Thread.Sleep(100); };
            var rebuilding = Task.Factory.StartNew(NewRebuildTask().Run);
            Thread.Sleep(50);
            cancellation.Cancel();
            rebuilding.Wait();
            var actual1 = projection.Handled.ToArray();
            // And
            projection.Handled.Clear();
            NewRebuildTask().Run();
            var actual2 = projection.Handled.ToArray();

            // Then
            Assert.That(actual1, Is.EqualTo(new[] { 1, 2 }));
            Assert.That(actual2, Is.EqualTo(new[] { 3, 4, 5 }));
        }

        [Test]
        public void PauseDuringRegularCatchUp()
        {
            Given();
            var commit2 = NewCommit(2);
            history.AddRange(new[] { NewCommit(1), commit2, NewCommit(3), NewCommit(4), NewCommit(5) });
            checkpointRepo.MockLoad(Checkpoint.REGULAR, new Checkpoint(Checkpoint.REGULAR, commit2));
            projection.Handled = new List<int>();

            // When
            projection.OnHandled = val => { if (val == 4) Thread.Sleep(100); };
            var rebuilding = Task.Factory.StartNew(NewRebuildTask().Run);
            Thread.Sleep(50);
            cancellation.Cancel();
            rebuilding.Wait();
            var actual1 = projection.Handled.ToArray();
            // And
            projection.Handled.Clear();
            NewRebuildTask().Run();
            var actual2 = projection.Handled.ToArray();

            // Then
            Assert.That(actual1, Is.EqualTo(new[] { 3, 4 }));
            Assert.That(actual2, Is.EqualTo(new[] { 5 }));
        }

        [Test]
        public void FullRebuildDespiteCheckpoint()
        {
            Given();
            var commit3 = NewCommit(3);
            history.AddRange(new[] { NewCommit(1), NewCommit(2), commit3, NewCommit(4), NewCommit(5) });
            checkpointRepo
                .MockLoad(Checkpoint.REGULAR, new Checkpoint(Checkpoint.REGULAR, commit3));
            projection.Handled = new List<int> { 0, 0, 0 };

            // When
            var rebuild = NewRebuildTask();
            rebuild.ResetCheckpoints();
            rebuild.Run();

            // Then
            Assert.That(projection.Handled, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
        }

        MyProjection projection;
        List<Commit> history;
        VersioningRepositoryMock versioningRepo;
        CheckpointStoreMock checkpointRepo;
        CancellationTokenSource cancellation;

        void Given()
        {
            projection = new MyProjection();
            history = new List<Commit>();
            versioningRepo = new VersioningRepositoryMock();
            checkpointRepo = new CheckpointStoreMock();
        }

        RebuildTask NewRebuildTask()
        {
            cancellation = new CancellationTokenSource();
            var ioc = new DependencyResolverMock().Register<IProjection>(_ => projection);
            var reader = new HistoryReaderMock().MockRead(history);
            return new RebuildTask(new ConsoleLogger(), ioc, reader, versioningRepo, checkpointRepo, cancellation.Token);
        }

        Commit NewCommit(int id)
        {
            var date = new DateTime(2000 + id, 1, 1);
            var eventMessages = new[] { new EventMessage { Body = new SomethingHappenedEvent { Created = date, Value = id } } };
            return new Commit("", "", 0, ToGuid(id), 0, date, "", null, eventMessages);
        }

        Guid ToGuid(int id)
        {
            return GuidUtils.ToGuid(id);
        }

        public class MyProjection : IProjection, IHandler<SomethingHappenedEvent>
        {
            public int Version { get; set; }

            public List<int> Handled;
            public Action<int> OnHandled;

            public void Clear()
            {
                Handled = new List<int>();
            }

            public void Handle(SomethingHappenedEvent evt)
            {
                Handled.Add(evt.Value);
                if (OnHandled != null)
                    OnHandled(evt.Value);
            }
        }

        public class SomethingHappenedEvent : DomainEvent
        {
            public int Value;
        }
    }
}
