using System;
using System.Collections.Generic;
using NEventStore.Cqrs.Utils.History;
using NEventStore.Persistence;
using Newtonsoft.Json;

namespace NEventStore.Cqrs.Impl.Utils.History
{
    public class WebHistoryReader : IHistoryReader
    {
        private const string UrlPattern = "{0}?start={1}&end={2}&pageSize={3}";
        private readonly string endpointUrl;
        private readonly IDownloader downloader;
        private int pageSize;

        public WebHistoryReader(string endpointUrl)
            : this(endpointUrl, new WebDownloader())
        {
        }

        public WebHistoryReader(string endpointUrl, IDownloader downloader)
            : this(endpointUrl, downloader, 128 * 2)
        {
        }


        public WebHistoryReader(string endpointUrl, IDownloader downloader, int pageSize)
        {
            this.endpointUrl = endpointUrl;
            this.downloader = downloader;
            this.pageSize = pageSize;
        }

        public IEnumerable<Commit> Read(DateTime start, DateTime end)
        {
            DateTime pageStart = start;
            Guid lastCommitId = Guid.Empty;
            bool lastCommitFound = true;
            while (true)
            {
                var commits = ReadNextPage(pageStart, end);
                if (commits.Length == 0 || commits[commits.Length - 1].CommitId == lastCommitId) yield break;
                foreach (var commit in commits)
                {
                    if (lastCommitFound)
                    {
                        pageStart = commit.CommitStamp;
                        lastCommitId = commit.CommitId;
                        yield return commit;
                    }
                    else if (lastCommitId == commit.CommitId)
                    {
                        lastCommitFound = true;
                    }
                }
                lastCommitFound = false;
            }
        }

        private Commit[] ReadNextPage(DateTime start, DateTime end)
        {
            string url = string.Format(UrlPattern, endpointUrl, start.ToString("O"), end.ToString("O"), pageSize);
            string json = downloader.DownloadString(url);
            return JsonConvert.DeserializeObject<Commit[]>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                NullValueHandling = NullValueHandling.Ignore
            });
        }
    }
}
