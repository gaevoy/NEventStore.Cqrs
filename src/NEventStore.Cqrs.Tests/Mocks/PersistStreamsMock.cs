using System;
using System.Collections.Generic;
using NEventStore.Persistence;

namespace NEventStore.Cqrs.Tests.Mocks
{
    public class PersistStreamsMock : IPersistStreams
    {
        private Func<DateTime, DateTime, IEnumerable<ICommit>> getFromTo;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ICommit> GetFrom(string bucketId, string streamId, int minRevision, int maxRevision)
        {
            throw new NotImplementedException();
        }

        public ICommit Commit(CommitAttempt attempt)
        {
            throw new NotImplementedException();
        }

        public ISnapshot GetSnapshot(string bucketId, string streamId, int maxRevision)
        {
            throw new NotImplementedException();
        }

        public bool AddSnapshot(ISnapshot snapshot)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IStreamHead> GetStreamsToSnapshot(string bucketId, int maxThreshold)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ICommit> GetFrom(string bucketId, DateTime start)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ICommit> GetFrom(string checkpointToken = null)
        {
            throw new NotImplementedException();
        }

        public ICheckpoint GetCheckpoint(string checkpointToken = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ICommit> GetFromTo(string bucketId, DateTime start, DateTime end)
        {
            return getFromTo(start, end);
        }

        public IEnumerable<ICommit> GetUndispatchedCommits()
        {
            throw new NotImplementedException();
        }

        public void MarkCommitAsDispatched(ICommit commit)
        {
            throw new NotImplementedException();
        }

        public void Purge()
        {
            throw new NotImplementedException();
        }

        public void Purge(string bucketId)
        {
            throw new NotImplementedException();
        }

        public void Drop()
        {
            throw new NotImplementedException();
        }

        public void DeleteStream(string bucketId, string streamId)
        {
            throw new NotImplementedException();
        }

        public bool IsDisposed { get; private set; }

        public PersistStreamsMock MockGetFromTo(Func<DateTime, DateTime, IEnumerable<ICommit>> body)
        {
            getFromTo = body;

            return this;
        }
    }
}