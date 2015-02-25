using System;
using System.Collections.Generic;
using NEventStore.Cqrs.Utils;

namespace NEventStore.Cqrs.MongoDb
{
    public class PersistHelper : IPersistHelper
    {
        private readonly string connectionString;

        public PersistHelper(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<Guid> GetIdsOfAggregates()
        {
            throw new NotImplementedException();
        }
        public void ClearSnapshots()
        {
            throw new NotImplementedException();
        }
    }
}
