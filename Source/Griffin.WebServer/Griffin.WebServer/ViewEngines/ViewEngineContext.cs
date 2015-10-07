using Griffin.WebServer.Files;

namespace Griffin.WebServer.ViewEngines
{
    public class ViewEngineContext
    {
        public ViewContext ViewContext { get; set; }
        public IFileService FileService { get; set; }
    }
}