using Nancy;

namespace PetProject.Books.Host.Endpoints
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            string startScriptUrl = "Content/start";
#if !DEBUG
            startScriptUrl += ".min";
#endif
            Get["/"] = _ => View["index", new { StartScriptUrl = startScriptUrl }];
        }
    }
}
