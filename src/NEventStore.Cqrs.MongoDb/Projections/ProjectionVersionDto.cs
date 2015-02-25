using System;
using MongoDB.Bson;

namespace NEventStore.Cqrs.MongoDb.Projections
{
    public class ProjectionVersionDto
    {
        public ObjectId Id { get; set; }

        public string ProjectionName { get; set; }
        
        public int Version { get; set; }

        public string Hash { get; set; }

        public DateTime? ChangeDate { get; set; }
    }
}
