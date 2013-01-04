using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using Griffin.Networking.Protocol.Http.Implementation;
using Griffin.Networking.Protocol.Http.Protocol;
using Griffin.WebServer.Modules;

namespace Griffin.WebServer.Files
{
    /// <summary>
    /// Will serve static files
    /// </summary>
    /// <example>
    /// <code>
    /// // One of the available file services.
    /// var diskFiles = new DiskFileService("/public/", @"C:\www\public\");
    /// var module = new FileModule(diskFiles);
    /// 
    /// // the module manager is added to the HttpServer.
    /// var moduleManager = new ModuleManager();
    /// moduleManager.Add(module);
    /// </code>
    /// </example>
    public class FileModule : IWorkerModule
    {
        private readonly IFileService _fileService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileModule" /> class.
        /// </summary>
        /// <param name="fileService">The file service.</param>
        public FileModule(IFileService fileService)
        {
            if (fileService == null) throw new ArgumentNullException("fileService");
            _fileService = fileService;
        }

        /// <summary>
        /// Gets or sets if we should allow file listing
        /// </summary>
        public bool ListFiles { get; set; }

        #region IWorkerModule Members

        /// <summary>
        /// Invoked before anything else
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <remarks>
        /// <para>The first method that is exeucted in the pipeline.</para>
        /// Try to avoid throwing exceptions if you can. Let all modules have a chance to handle this method. You may break the processing in any other method than the Begin/EndRequest methods.
        /// <para>If you are going to handle the request, implement <see cref="IWorkerModule"/> and do it in the <see cref="IWorkerModule.HandleRequest"/> method.</para>
        /// </remarks>
        public void BeginRequest(IHttpContext context)
        {
        }

        /// <summary>
        /// End request is typically used for post processing. The response should already contain everything required.
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <remarks>
        /// <para>The last method that is executed in the pipeline.</para>
        /// Try to avoid throwing exceptions if you can. Let all modules have a chance to handle this method. You may break the processing in any other method than the Begin/EndRequest methods.</remarks>
        public void EndRequest(IHttpContext context)
        {
        }

        public void HandleRequestAsync(IHttpContext context, Action<IAsyncModuleResult> callback)
        {
            // just invoke the callback synchronously.
            callback(new AsyncModuleResult(context, HandleRequest(context)));
        }

        /// <summary>
        /// Handle the request.
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns><see cref="ModuleResult.Stop"/> will stop all processing except <see cref="IHttpModule.EndRequest"/>.</returns>
        /// <remarks>Invoked in turn for all modules unless you return <see cref="ModuleResult.Stop"/>.</remarks>
        public ModuleResult HandleRequest(IHttpContext context)
        {
            // only handle GET and HEAD
            if (!context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase)
                && !context.Request.Method.Equals("HEAD", StringComparison.OrdinalIgnoreCase))
                return ModuleResult.Continue;

            // serve a directory
            if (ListFiles)
            {
                if (TryGenerateDirectoryPage(context))
                    return ModuleResult.Stop;
            }

            var header = context.Request.Headers["If-Modified-Since"];
            var time = header != null
                           ? DateTime.ParseExact(header.Value, "R", CultureInfo.InvariantCulture)
                           : DateTime.MinValue;


            var fileContext = new FileContext(context.Request, time);
            _fileService.GetFile(fileContext);
            if (!fileContext.IsFound)
                return ModuleResult.Continue;

            if (!fileContext.IsModified)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                context.Response.StatusDescription = "Was last modified " + fileContext.LastModifiedAtUtc.ToString("R");
                return ModuleResult.Stop;
            }

            var mimeType = MimeTypeProvider.Instance.Get(fileContext.Filename);
            if (mimeType == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
                context.Response.StatusDescription = string.Format("File type '{0}' is not supported.",
                                                                   Path.GetExtension(fileContext.Filename));
                return ModuleResult.Stop;
            }

            context.Response.AddHeader("Last-Modified", fileContext.LastModifiedAtUtc.ToString("R"));
            context.Response.AddHeader("Accept-Ranges", "bytes");
            context.Response.AddHeader("Content-Disposition", "inline;filename=\"" + Path.GetFileName(fileContext.Filename) + "\"");
            context.Response.ContentType = mimeType;
            context.Response.ContentLength = (int)fileContext.FileStream.Length;

            // ranged/partial transfers
            var rangeHeader = context.Request.Headers["Range"];
            if (rangeHeader != null && !string.IsNullOrEmpty(rangeHeader.Value))
            {
                var ranges = new RangeCollection();
                ranges.Parse(rangeHeader.Value, (int)fileContext.FileStream.Length);
                context.Response.AddHeader("Content-Range", ranges.ToHtmlHeaderValue((int)fileContext.FileStream.Length));
                context.Response.Body = new ByteRangeStream(ranges, fileContext.FileStream);
                context.Response.ContentLength = ranges.TotalLength;
            }
            else
                context.Response.Body = fileContext.FileStream;

            // do not include a body when the client only want's to get content information.
            if (context.Request.Method.Equals("HEAD", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Body.Dispose();
                context.Response.Body = null;
            }

            return ModuleResult.Stop;
        }

        private bool TryGenerateDirectoryPage(IHttpContext context)
        {
            if (!_fileService.IsDirectory(context.Request.Uri))
                return false;

            int pos = ListFilesTemplate.IndexOf("{{Files}}");
            if (pos == -1)
                throw new InvalidOperationException("Failed to find '{{Files}}' in the ListFilesTemplate.");
            var newLine = ListFilesTemplate.LastIndexOf("\r\n", pos);
            var spaces = "".PadLeft(pos - newLine - 2); //exclude crlf;

            var sb = new StringBuilder();
            sb.Append(ListFilesTemplate.Substring(0, pos));
            foreach (var file in _fileService.GetFiles(context.Request.Uri))
            {
                var fileUri = context.Request.Uri.AbsolutePath + file.Name;
                sb.AppendFormat(@"{4}<tr><td><a href=""{0}"">{1}</a></td><td>{2}</td><td style=""text-align: right"">{3}</td></tr>", fileUri, file.Name,
                                file.LastModifiedAtUtc, file.Size, spaces);
                sb.AppendLine();
            }

            sb.Append(ListFilesTemplate.Substring(pos + 9));


            context.Response.Body = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
            return true;
        }

        public static string ListFilesTemplate = @"<html>
    <head>
        <title>Listing files</title>
    <head>
    <body>
        <table>
            <thead>
                <tr>
                    <th>Filename</th>
                    <th>Modified at</th>
                    <th>File size</th>
                </tr>
            <thead>
            <tbody>
                {{Files}}
            </tbody>
        </table>
    </body>
</html>";

        #endregion
    }
}