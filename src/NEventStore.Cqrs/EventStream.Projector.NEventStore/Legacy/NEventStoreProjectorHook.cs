using System.Linq;
using EventStream.Projector;

namespace NEventStore.Cqrs.EventStream.Projector.NEventStore.Legacy
{
    public class NEventStoreProjectorHook : PipelineHookBase
    {
        private readonly IProjector projector;

        public NEventStoreProjectorHook(IProjector projector)
        {
            this.projector = projector;
        }

        public override void PostCommit(ICommit committed)
        {
            if (committed.Headers.Keys.Any(s => s == "SagaType")) return;

            projector.Handle(new EventsSlice(NEventStoreStream.CommitToCheckpoint(committed), committed.Events.Select(e => e.Body)));
        }
    }
}