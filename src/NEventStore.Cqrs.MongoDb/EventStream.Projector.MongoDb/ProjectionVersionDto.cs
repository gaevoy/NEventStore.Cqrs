namespace NEventStore.Cqrs.MongoDb.EventStream.Projector.MongoDb
{
    public class ProjectionVersionDto
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public bool IsExist { get; set; }
    }
}
