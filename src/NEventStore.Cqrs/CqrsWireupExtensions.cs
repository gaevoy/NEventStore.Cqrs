using EventStream.Projector;

namespace NEventStore.Cqrs
{
    public static class CqrsWireupExtensions
    {
        public static CqrsWireup UsingCqrs(this Wireup wireup, IDependencyResolver ioc, bool enableWriteSide = true, bool enableReadSide = true)
        {
            return new CqrsWireup(wireup, ioc, enableWriteSide, enableReadSide);
        }
    }
}
