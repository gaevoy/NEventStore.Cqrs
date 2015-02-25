using System;
using System.Collections.Generic;
using System.Linq;
using NEventStore.Persistence;
using NEventStore.Persistence.InMemory;
using NEventStore.Serialization;

namespace NEventStore.Cqrs.Tests.Impl
{
    public class InMemoryPersistenceEngineWithSerialization : IPersistStreams
    {
        private readonly IPersistStreams underlying;
        private readonly ISerialize serializer;

        public InMemoryPersistenceEngineWithSerialization(ISerialize serializer)
        {
            this.underlying = new InMemoryPersistenceEngine();
            this.serializer = serializer;
        }

        public void Dispose()
        {
            underlying.Dispose();
        }

        public IEnumerable<ICommit> GetFrom(string bucketId, string streamId, int minRevision, int maxRevision)
        {
            return underlying.GetFrom(bucketId, streamId, minRevision, maxRevision);
        }

        public ICommit Commit(CommitAttempt attempt)
        {
            return underlying.Commit(EmulateSerializationDeserialization(attempt));
        }

        public ISnapshot GetSnapshot(string bucketId, string streamId, int maxRevision)
        {
            return underlying.GetSnapshot(bucketId, streamId, maxRevision);
        }

        public bool AddSnapshot(ISnapshot snapshot)
        {
            return underlying.AddSnapshot(snapshot);
        }

        public IEnumerable<IStreamHead> GetStreamsToSnapshot(string bucketId, int maxThreshold)
        {
            return underlying.GetStreamsToSnapshot(bucketId, maxThreshold);
        }

        public void Initialize()
        {
            underlying.Initialize();
        }

        public IEnumerable<ICommit> GetFrom(string bucketId, DateTime start)
        {
            return underlying.GetFrom(bucketId, start);
        }

        public IEnumerable<ICommit> GetFrom(string checkpointToken = null)
        {
            return underlying.GetFrom(checkpointToken);
        }

        public ICheckpoint GetCheckpoint(string checkpointToken = null)
        {
            return underlying.GetCheckpoint(checkpointToken);
        }

        public IEnumerable<ICommit> GetFromTo(string bucketId, DateTime start, DateTime end)
        {
            return underlying.GetFromTo(bucketId, start, end);
        }

        public IEnumerable<ICommit> GetUndispatchedCommits()
        {
            return underlying.GetUndispatchedCommits();
        }

        public void MarkCommitAsDispatched(ICommit commit)
        {
            underlying.MarkCommitAsDispatched(commit);
        }

        public void Purge()
        {
            underlying.Purge();
        }

        public void Purge(string bucketId)
        {
            underlying.Purge(bucketId);
        }

        public void Drop()
        {
            underlying.Drop();
        }

        public void DeleteStream(string bucketId, string streamId)
        {
            underlying.DeleteStream(bucketId, streamId);
        }

        public bool IsDisposed { get { return underlying.IsDisposed; } }

        private CommitAttempt EmulateSerializationDeserialization(CommitAttempt attempt)
        {
            // serialization
            var headersPayload = serializer.Serialize(attempt.Headers);
            var eventsPayload = serializer.Serialize(attempt.Events.ToList());
            // deserialization
            var headers = serializer.Deserialize<Dictionary<string, object>>(headersPayload);
            var events = serializer.Deserialize<List<EventMessage>>(eventsPayload);
            return new CommitAttempt(
                attempt.BucketId,
                attempt.StreamId,
                attempt.StreamRevision,
                attempt.CommitId,
                attempt.CommitSequence,
                attempt.CommitStamp,
                headers,
                events);
        }
    }
}