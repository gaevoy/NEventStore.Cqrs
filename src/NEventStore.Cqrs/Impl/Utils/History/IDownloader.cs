namespace NEventStore.Cqrs.Impl.Utils.History
{
    public interface IDownloader
    {
        string DownloadString(string url);
    }
}
