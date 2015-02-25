using System;
using ServiceStack.DataAnnotations;

namespace NEventStore.Cqrs.MsSql.Projections
{
    public class ProjectionVersionDto
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        [Index(Unique = true)]
        public string ProjectionName { get; set; }
        
        public int Version { get; set; }

        public string Hash { get; set; }

        public DateTime? ChangeDate { get; set; }
    }
}
