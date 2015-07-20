using System;

namespace NEventStore.Cqrs.Tests.Mocks
{
    public class DownloaderMock : IDownloader
    {
        private Func<string, string> downloadString;

        public string DownloadString(string url)
        {
            return downloadString(url);
        }

        public DownloaderMock MockDownloadString(Func<string, string> body)
        {
            downloadString = body;

            return this;
        }
    }
}