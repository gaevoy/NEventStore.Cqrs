using System;
using System.Linq;
using NEventStore.Persistence;
using Newtonsoft.Json;

namespace NEventStore.Cqrs.Impl.Utils.History
{
    public class WebHistoryEndpoint
    {
        private readonly IStoreEvents es;

        public WebHistoryEndpoint(IStoreEvents es)
        {
            this.es = es;
        }

        public string Read(string start, string end, string pageSize)
        {
            start = start.Trim().Replace(" ", "+");
            end = end.Trim().Replace(" ", "+");
            DateTime startDate = DateTime.Parse(start).ToUniversalTime();
            DateTime endDate = DateTime.Parse(end).ToUniversalTime();
            int take = int.Parse(pageSize);

            var commits = es.Advanced.GetFromTo(startDate, endDate).Take(take).Cast<Commit>().ToArray();
            return JsonConvert.SerializeObject(commits, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                NullValueHandling = NullValueHandling.Ignore
            });
        }
    }
}
