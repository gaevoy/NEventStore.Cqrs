using System.Collections.Generic;
using System.Linq;
using EventStream.Projector.Logger;
using EventStream.Projector.Persistence;

namespace EventStream.Projector.Impl
{
    public class SimpleProjector : IProjector
    {
        internal readonly UntypedProjection[] projections;
        private readonly ICheckpointStore checkpoints;

        public SimpleProjector(IEnumerable<IProjection> projections, ICheckpointStore checkpoints = null, ILog log = null)
        {
            log = log ?? new NullLog();
            this.projections = projections.Select(e => new UntypedProjection(e, log)).ToArray();
            this.checkpoints = checkpoints;
        }

        public void Handle(EventsSlice evts)
        {
            foreach (var evt in evts.Events)
                foreach (var projection in projections)
                    projection.Handle(evt);
            if (checkpoints != null) checkpoints.Save(evts.Checkpoint, Checkpoint.Default);
        }
    }
}
