namespace NEventStore.Cqrs.MongoDb.EventStream.Projector.MongoDb
{
    public class ProjectionCheckpointDto
    {
        public string Id { get; set; }
        public string Position { get; set; }
    }
}
