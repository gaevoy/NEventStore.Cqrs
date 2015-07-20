using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventStream.Projector.Impl;
using EventStream.Projector.Persistence;
using EventStream.Projector.Tests.Mocks;
using NUnit.Framework;

namespace EventStream.Projector.Tests.Impl
{
    [TestFixture]
    public class ProjectionRebuildTest
    {
        [Test]
        public void RebuildOnEmptyDb()
        {
            var history = new List<EventsSlice>(new[] { NewEvent(1), NewEvent(2), NewEvent(3), NewEvent(4), NewEvent(5) });

            // When
            NewRebuild().Start(new LocalEventStream(history), CancellationToken.None);

            // Then
            Assert.That(projection.Handled, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
        }

        [Test]
        public void RebuildOnEmptyDb2()
        {
            var history = new List<EventsSlice>(new[] { NewEvent(1), NewEvent(2), NewEvent(3), NewEvent(4), NewEvent(5) });
            versions.Save(new ProjectionInfo(projection, "changed", true));

            // When
            NewRebuild().Start(new LocalEventStream(history), CancellationToken.None);

            // Then
            Assert.That(projection.Handled, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
        }

        [Test, ExpectedException(typeof(Exception))]
        public void CheckpointIsAbsent()
        {
            var history = new List<EventsSlice>(new[] { NewEvent(1), NewEvent(2), NewEvent(3), NewEvent(4), NewEvent(5) });
            checkpoints.Save(new Checkpoint("6"), Checkpoint.Default);

            // When
            NewRebuild().Start(new LocalEventStream(history), CancellationToken.None);
        }

        [Test]
        public void ProjectionHasModified()
        {
            var history = new List<EventsSlice>(new[] { NewEvent(11), NewEvent(22), NewEvent(33), NewEvent(44), NewEvent(55) });
            versions.Save(new ProjectionInfo(projection, "changed", true));
            checkpoints.Save(new Checkpoint("33"), Checkpoint.Default);
            projection.Handled = new List<int> { 1, 1, 1, 1, 1, 1, 1 };

            // When
            NewRebuild().Start(new LocalEventStream(history), CancellationToken.None);

            // Then
            Assert.That(projection.Handled, Is.EqualTo(new[] { 11, 22, 33, 44, 55 }));
        }

        [Test]
        public void ProjectionHasFallenBehind()
        {
            var history = new List<EventsSlice>(new[] { NewEvent(111), NewEvent(222), NewEvent(333), NewEvent(444), NewEvent(555) });
            versions.Save(new ProjectionInfo(projection, "original", true));
            checkpoints.Save(new Checkpoint("333"), Checkpoint.Default);
            projection.Handled = new List<int> { 0, 0, 0 };

            // When
            NewRebuild().Start(new LocalEventStream(history), CancellationToken.None);

            // Then
            Assert.That(projection.Handled, Is.EqualTo(new[] { 0, 0, 0, 444, 555 }));
        }

        [Test]
        public void PauseDuringProjectionCatchUp()
        {
            var history = new List<EventsSlice>(new[] { NewEvent(1), NewEvent(2), NewEvent(3), NewEvent(4), NewEvent(5) });
            versions.Save(new ProjectionInfo(projection, "changed", true));
            checkpoints.Save(new Checkpoint("4"), Checkpoint.Default);
            var cancellation = new CancellationTokenSource();

            // When
            projection.OnHandled = val => { if (val == 2) Thread.Sleep(200); };
            var rebuilding = Task.Factory.StartNew(() => NewRebuild().Start(new LocalEventStream(history), cancellation.Token), TaskCreationOptions.LongRunning);
            Thread.Sleep(100);
            cancellation.Cancel();
            rebuilding.Wait();
            var actual1 = projection.Handled.ToArray();
            // And
            projection.Handled.Clear();
            NewRebuild().Start(new LocalEventStream(history), CancellationToken.None);
            var actual2 = projection.Handled.ToArray();

            // Then
            Assert.That(actual1, Is.EqualTo(new[] { 1, 2 }));
            Assert.That(actual2, Is.EqualTo(new[] { 3, 4, 5 }));
        }

        [Test]
        public void PauseDuringRegularCatchUp()
        {
            var history = new List<EventsSlice>(new[] { NewEvent(1), NewEvent(2), NewEvent(3), NewEvent(4), NewEvent(5) });
            versions.Save(new ProjectionInfo(projection, "original", true));
            checkpoints.Save(new Checkpoint("2"), Checkpoint.Default);
            projection.Handled = new List<int>();
            var cancellation = new CancellationTokenSource();

            // When
            projection.OnHandled = val => { if (val == 4) Thread.Sleep(200); };
            var rebuilding = Task.Factory.StartNew(() => NewRebuild().Start(new LocalEventStream(history), cancellation.Token), TaskCreationOptions.LongRunning);
            Thread.Sleep(100);
            cancellation.Cancel();
            rebuilding.Wait();
            var actual1 = projection.Handled.ToArray();
            // And
            projection.Handled.Clear();
            NewRebuild().Start(new LocalEventStream(history), CancellationToken.None);
            var actual2 = projection.Handled.ToArray();

            // Then
            Assert.That(actual1, Is.EqualTo(new[] { 3, 4 }));
            Assert.That(actual2, Is.EqualTo(new[] { 5 }));
        }

        [Test]
        public void FullRebuildDespiteCheckpoint()
        {
            var commit3 = NewEvent(3);
            var history = new List<EventsSlice>(new[] { NewEvent(1), NewEvent(2), commit3, NewEvent(4), NewEvent(5) });
            checkpoints.Save(new Checkpoint("3"), Checkpoint.Default);
            projection.Handled = new List<int> { 0, 0, 0 };

            // When
            var rebuild = NewRebuild();
            checkpoints.Save(null, Checkpoint.Default);
            rebuild.Start(new LocalEventStream(history), CancellationToken.None);

            // Then
            Assert.That(projection.Handled, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
        }

        MyProjection projection;
        ProjectionInfoStoreMock versions;
        CheckpointStoreMock checkpoints;

        [SetUp]
        public void SetUp()
        {
            projection = new MyProjection();
            versions = new ProjectionInfoStoreMock();
            checkpoints = new CheckpointStoreMock();
        }

        ProjectionRebuild NewRebuild()
        {
            return new ProjectionRebuild(new[] { projection }, versions, new ConsoleLog(), checkpoints);
        }

        EventsSlice NewEvent(int id)
        {
            var date = new DateTime(2000 + id, 1, 1);
            var evts = new[] { new SomethingHappenedEvent { Created = date, Value = id } };
            return new EventsSlice(new Checkpoint(id.ToString()), evts);
        }

        public class MyProjection : IProjection
        {
            public MyProjection()
            {
                Version = "original";
            }

            public string Version { get; set; }

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

        public class SomethingHappenedEvent
        {
            public int Value;
            public DateTime Created { get; set; }
        }
    }
}
