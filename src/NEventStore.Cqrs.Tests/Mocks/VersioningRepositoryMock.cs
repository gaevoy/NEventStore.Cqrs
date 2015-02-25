using System;
using System.Collections.Generic;
using System.Linq;
using NEventStore.Cqrs.Projections;

namespace NEventStore.Cqrs.Tests.Mocks
{
    public class VersioningRepositoryMock : IVersioningRepository
    {
        readonly Dictionary<Type, string> modifiedReasons = new Dictionary<Type, string>();
        public IProjection[] SelectModified(params IProjection[] projections)
        {
            return projections
                .Where(e => modifiedReasons.ContainsKey(e.GetType()) && modifiedReasons[e.GetType()] != string.Empty)
                .ToArray();
        }

        public bool IsInitialized(IProjection[] projections)
        {
            return projections.Any(e => modifiedReasons.ContainsKey(e.GetType()));
        }

        public string GetModifiedReason(IProjection projection)
        {
            return modifiedReasons[projection.GetType()];
        }

        public void MarkAsUnmodified(IProjection projection)
        {
            modifiedReasons[projection.GetType()] = string.Empty;
        }

        public VersioningRepositoryMock MarkAsModified<T>(string modifiedReason) where T : IProjection
        {
            modifiedReasons[typeof(T)] = modifiedReason;
            return this;
        }

        public VersioningRepositoryMock MarkAsUnmodified<T>() where T : IProjection
        {
            modifiedReasons[typeof(T)] = string.Empty;
            return this;
        }
    }
}