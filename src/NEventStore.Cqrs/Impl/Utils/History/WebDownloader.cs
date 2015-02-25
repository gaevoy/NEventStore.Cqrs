using System.IO;
using System.Net;
using System.Text;

namespace NEventStore.Cqrs.Impl.Utils.History
{
    public class WebDownloader : IDownloader
    {
        public string DownloadString(string url)
        {
            var response = WebRequest.Create(url).GetResponse();
            
            using (var responseStream = response.GetResponseStream())
            {
                var responceStreamReader = new StreamReader(responseStream, Encoding.GetEncoding("UTF-8"));
                return responceStreamReader.ReadToEnd();
            }
        }
    }
}
