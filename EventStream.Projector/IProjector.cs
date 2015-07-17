namespace EventStream.Projector
{
    public interface IProjector
    {
        void Handle(EventsSlice evt);
    }
}
