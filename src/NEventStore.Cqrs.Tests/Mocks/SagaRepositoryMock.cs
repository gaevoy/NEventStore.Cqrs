using System;
using System.Collections.Generic;
using CommonDomain;
using CommonDomain.Persistence;

namespace NEventStore.Cqrs.Tests.Mocks
{
    public class SagaRepositoryMock : ISagaRepository
    {
        public ISaga SavedSaga { get; set; }

        private Dictionary<Guid, ISaga> SagasToGet = new Dictionary<Guid, ISaga>();
        public TSaga GetById<TSaga>(Guid sagaId) where TSaga : class, ISaga, new()
        {
            ISaga saga;
            if (SagasToGet.TryGetValue(sagaId, out saga))
            {
                if (saga is TSaga)
                {
                    return (TSaga)saga;
                }
                else
                {
                    throw new Exception("Wrong type to get");
                }
            }
            else
            {
                return new TSaga();
            }
        }

        public void Save(ISaga saga, Guid commitId, Action<IDictionary<string, object>> updateHeaders)
        {
            SagasToGet[new Guid(saga.Id)] = saga;

            SavedSaga = saga;
        }

        public SagaRepositoryMock MockGetById(Guid id, ISaga saga)
        {
            SagasToGet.Add(id, saga);

            return this;
        }

        public TSaga GetById<TSaga>(string bucketId, string sagaId) where TSaga : class, ISaga, new()
        {
            return GetById<TSaga>(new Guid(sagaId));
        }

        public void Save(string bucketId, ISaga saga, Guid commitId, Action<IDictionary<string, object>> updateHeaders)
        {
            Save(saga, commitId, updateHeaders);
        }
    }
}
