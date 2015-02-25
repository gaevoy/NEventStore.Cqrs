using NEventStore.Persistence;

namespace NEventStore.Cqrs.Tests.Mocks
{
    public class StoreEventsMock : IStoreEvents
    {
        public void Dispose()
        {
        }

        public IEventStream CreateStream(string bucketId, string streamId)
        {
            return null;
        }

        public IEventStream OpenStream(string bucketId, string streamId, int minRevision, int maxRevision)
        {
            return null;
        }

        public IEventStream OpenStream(ISnapshot snapshot, int maxRevision)
        {
            return null;
        }

        public void StartDispatchScheduler()
        {
        }

        public IPersistStreams Advanced { get; set; }

        public StoreEventsMock MockAdvanced(IPersistStreams advanced)
        {
            Advanced = advanced;
            return this;
        }
    }
}