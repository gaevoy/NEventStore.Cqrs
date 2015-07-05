using System;
using EventStream.Projector.Impl;

namespace EventStream.Projector
{
    public class Wireup
    {
        public static Wireup Init(IProjection[] projections)
        {
            var wireup = new Wireup
                {
                    Projections = projections,
                    CheckpointCtor = () => { throw new NotSupportedException("Checkpoint isn't configured yet"); },
                    LogCtor = () => new NullLog(),
                };
            wireup.ProjectorCtor = () => new Impl.Projector(projections, wireup.LogCtor(), wireup.CheckpointCtor());
            wireup.RebuildTaskCtor = () => new Impl.RebuildTask(projections, wireup.ProjectionVersioningCtor(), wireup.CheckpointCtor());
            return wireup;
        }

        protected IProjection[] Projections;
        protected Func<ICheckpoint> CheckpointCtor;
        protected Func<ILog> LogCtor;
        protected Func<IProjector> ProjectorCtor;
        protected Func<IProjectionVersioning> ProjectionVersioningCtor;
        protected Func<IRebuildTask> RebuildTaskCtor;

        public IProjector NewProjector()
        {
            return ProjectorCtor();
        }

        public IRebuildTask Rebuild()
        {
            return RebuildTaskCtor();
        }
    }
}
