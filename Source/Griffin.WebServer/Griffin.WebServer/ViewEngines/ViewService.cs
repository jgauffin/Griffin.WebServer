using System;
using System.Collections.Generic;
using System.Linq;
using Griffin.WebServer.Files;

namespace Griffin.WebServer.ViewEngines
{
    /// <summary>
    /// Used to render views.
    /// </summary>
    public class ViewService
    {
        private readonly IFileService _fileService;
        private readonly List<IViewEngine> _viewEngines = new List<IViewEngine>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewService" /> class.
        /// </summary>
        /// <param name="fileService">Used to locate all views.</param>
        public ViewService(IFileService fileService)
        {
            _fileService = fileService;
        }

        /// <summary>
        /// Register a view engine.
        /// </summary>
        /// <param name="viewEngine"></param>
        public void Register(IViewEngine viewEngine)
        {
            if (viewEngine == null) throw new ArgumentNullException("viewEngine");
            _viewEngines.Add(viewEngine);
        }

        /// <summary>
        /// Render a view
        /// </summary>
        /// <param name="context"></param>
        public void Render(ViewContext context)
        {
            var veContext = new ViewEngineContext
                {
                    ViewContext = context,
                    FileService = _fileService
                };

            if (_viewEngines.Any(viewEngine => viewEngine.Render(veContext)))
                return;

            throw new ViewNotFoundException(context.ViewPath);
        }
    }

    public class ViewEngineContext
    {
        public ViewContext ViewContext { get; set; }
        public IFileService FileService { get; set; }
    }
}