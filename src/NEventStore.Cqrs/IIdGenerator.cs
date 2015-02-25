using System;

namespace NEventStore.Cqrs
{
    public interface IIdGenerator
    {
        Guid NewGuid();
    }
}
