namespace EventStream.Projector
{
    public struct EventsSlice
    {
        public Checkpoint Checkpoint { get; set; }
        public object[] Events { get; set; }
    }
}