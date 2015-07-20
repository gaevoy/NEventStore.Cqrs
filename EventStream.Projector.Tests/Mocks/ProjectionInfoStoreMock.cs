using System.Collections.Concurrent;
using System.Linq;
using EventStream.Projector.Persistence;

namespace EventStream.Projector.Tests.Mocks
{
    public class ProjectionInfoStoreMock : IProjectionInfoStore
    {
        readonly ConcurrentDictionary<IProjection, ProjectionInfo> db = new ConcurrentDictionary<IProjection, ProjectionInfo>();

        public void Save(params ProjectionInfo[] projection)
        {
            foreach (var info in projection)
                db[info.Projection] = info;
        }

        public ProjectionInfo[] Restore(params IProjection[] projections)
        {
            return projections
                .Select(e => db.ContainsKey(e) ? db[e] : new ProjectionInfo(e, null, false))
                .ToArray();
        }
    }
}