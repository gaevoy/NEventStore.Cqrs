using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NEventStore.Cqrs.Messages;
using Newtonsoft.Json;
using NUnit.Framework;

namespace NEventStore.Cqrs.Tests
{
    public abstract class TestBase
    {
        protected abstract void ClearEvents();

        protected abstract List<T> GetUncommitted<T>() where T : IMessage;

        protected abstract ICollection GetUndispatchedMessages<TCommand>() where TCommand : DomainCommand;
        
        protected List<DomainEvent> GetUncommittedEvents()
        {
            return GetUncommitted<DomainEvent>();
        }

        protected T GetUncommittedEvent<T>() where T : DomainEvent
        {
            return GetUncommitted<T>().Single();
        }

        protected Guid? ToGuid(int? id)
        {
            return GuidUtils.ToGuid(id);
        }

        protected Guid ToGuid(int id)
        {
            return GuidUtils.ToGuid(id);
        }

        protected DateTime ToDateTime(int year, int month = 1, int day = 1)
        {
            return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
        }

        protected DateTime? ToDateTime(int? year, int month = 1, int day = 1)
        {
            return year.HasValue ? ToDateTime(year.Value, month, day) : (DateTime?)null;
        }

        protected void AssertIsSequenceEqual<T>(IList<T> evts, IList<T> enumerable)
        {
            IEnumerable<T> commonPart = evts.Intersect(enumerable);
            CollectionAssert.AreEqual(commonPart, enumerable);
        }

        protected void AssertUncommittedSequence(params object[] evts)
        {
            AssertIsSequenceEqual(GetUncommitted<IMessage>(), evts.Cast<IMessage>().ToList());
        }
        protected void AssertUncommittedSequence(params Type[] expectedEventTypes)
        {
            var actual = GetUncommittedEvents().Select(e => e.GetType()).Intersect(expectedEventTypes);
            CollectionAssert.AreEqual(expectedEventTypes, actual);
        }

        protected void AssertEvent<TEvent>(Action<TEvent> assert) where TEvent : DomainEvent
        {
            foreach (var evt in GetUncommitted<TEvent>())
            {
                assert(evt);
            }
        }
        protected void AssertEventIsNotRaised<TEvent>() where TEvent : DomainEvent
        {
            Assert.AreEqual(0, GetUncommittedEvents().OfType<TEvent>().Count());
        }
        protected void AssertCommandIsNotRaised<TCommand>() where TCommand : DomainCommand
        {
            Assert.AreEqual(0, GetUndispatchedMessages<TCommand>().OfType<TCommand>().Count());
        }

        protected virtual void AssertEvent<TEvent>(TEvent expected, Func<TEvent, object> unique) where TEvent : DomainEvent
        {
            TEvent actual = GetUncommitted<TEvent>().SingleOrDefault(e => unique(expected).Equals(unique(e)));
            if (actual == null)
            {
                Assert.Fail("Event {0} is not raised", typeof(TEvent).Name);
            }
            actual.Created = expected.Created;
            actual.IssuedBy = expected.IssuedBy;
            actual.Version = expected.Version;
            Assert.AreEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(actual));
        }

        protected virtual void AssertEvent<TEvent>(TEvent expected, Func<TEvent, bool> when = null) where TEvent : DomainEvent
        {
            TEvent actual = GetUncommitted<TEvent>().SingleOrDefault(e => when == null || when(e));
            if (actual == null)
            {
                Assert.Fail("Event {0} is not raised", typeof(TEvent).Name);
            }
            actual.Created = expected.Created;
            actual.IssuedBy = expected.IssuedBy;
            actual.Version = expected.Version;
            Assert.AreEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(actual));
        }
        protected virtual void AssertCommand<TCommand>(TCommand expected, Func<TCommand, bool> when = null) where TCommand : DomainCommand
        {
            TCommand actual = GetUncommitted<TCommand>().Single(e => when == null || when(e));
            actual.IssuedBy = expected.IssuedBy;
            actual.Version = expected.Version;
            actual.CommitId = expected.CommitId;
            Assert.AreEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(actual));
        }
    }
}
