namespace EventStream.Projector
{
    public class EventsSlice
    {
        public string Position { get; set; }
        public object[] Events { get; set; }
    }
}