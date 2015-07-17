using System.Collections.Generic;
using System.Linq;

namespace EventStream.Projector.Impl
{
    public class Projector : IProjector
    {
        internal readonly UntypedProjection[] projections;
        private readonly ICheckpointStore checkpoints;

        public Projector(IEnumerable<IProjection> projections, ILog log, ICheckpointStore checkpoints = null)
        {
            this.projections = projections.Select(e => new UntypedProjection(e, log)).ToArray();
            this.checkpoints = checkpoints;
        }

        public void Handle(EventsSlice evts)
        {
            foreach (var evt in evts.Events)
                foreach (var projection in projections)
                    projection.Handle(evt);
            if (checkpoints != null) checkpoints.Save(evts.Checkpoint, CheckpointScope.Default);
        }
    }
}
