using System;
using System.Collections.Generic;

namespace NEventStore.Cqrs.Utils
{
    public interface IPersistHelper
    {
        List<Guid> GetIdsOfAggregates();
        void ClearSnapshots();
    }
}
