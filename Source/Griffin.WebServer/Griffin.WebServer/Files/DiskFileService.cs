using System;
using System.Collections.Generic;
using System.IO;

namespace Griffin.WebServer.Files
{
    /// <summary>
    ///     Serves files from the hard drive.
    /// </summary>
    public class DiskFileService : IFileService
    {
        private readonly string _basePath;
        private readonly string _rootUri;
        private readonly bool _substituteGzipFiles;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositeFileService" /> class.
        /// </summary>
        /// <param name="rootFilePath">Path to serve files from.</param>
        /// <param name="rootUri">Serve all files which are located under this URI</param>
        /// <param name="substituteGzipFiles">
        ///     When enabled, if a file is requested and a file with the same name + .gz exists, that
        ///     version will be served instead (so long as the client supports this)
        /// </param>
        /// <example>
        ///     <code>
        /// var diskFiles = new DiskFileService("/public/", @"C:\www\public\");
        /// var module = new FileModule(diskFiles);
        /// 
        /// var moduleManager = new ModuleManager();
        /// moduleManager.Add(module);
        /// </code>
        /// </example>
        public DiskFileService(string rootUri, string rootFilePath, bool substituteGzipFiles = false)
        {
            if (rootUri == null) throw new ArgumentNullException("rootUri");
            if (rootFilePath == null) throw new ArgumentNullException("rootFilePath");
            if (!Directory.Exists(rootFilePath))
                throw new ArgumentOutOfRangeException("rootFilePath", rootFilePath,
                    "Failed to find path " + rootFilePath);

            _rootUri = rootUri;
            _basePath = rootFilePath;
            _substituteGzipFiles = substituteGzipFiles;

            DefaultHtmlFile = "index.html";
        }

        /// <summary>
        ///     Default file to serve if none is specified in the context
        /// </summary>
        public string DefaultHtmlFile { get; set; }

        private string GetDefaultFile()
        {
            return Path.Combine(_basePath, DefaultHtmlFile);
        }

        #region IFileService Members

        /// <summary>
        ///     Get a file
        /// </summary>
        /// <param name="context">Context used to locate and return files</param>
        public virtual bool GetFile(FileContext context)
        {
            var fullPath = GetFullPath(context.Request.Uri);
            if (fullPath == null)
                return false;

            if (fullPath == _basePath)
                fullPath = GetDefaultFile();

            if (!File.Exists(fullPath))
                return false;

            var streamPath = fullPath;

            if (_substituteGzipFiles && context.Request.Headers["Accept-Encoding"] != null &&
                context.Request.Headers["Accept-Encoding"].Contains("gzip"))
            {
                var compressedPath = fullPath + ".gz";
                if (File.Exists(compressedPath))
                {
                    streamPath = compressedPath;
                    context.SetGzipSubstitute();
                }
            }

            var date = File.GetLastWriteTimeUtc(fullPath);

            // browser ignores second fractions.
            date = date.AddTicks(-(date.Ticks % TimeSpan.TicksPerSecond));

            if (date <= context.BrowserCacheDate)
            {
                context.SetNotModified(fullPath, date);
                return true;
            }

            var stream = new FileStream(streamPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            context.SetFile(fullPath, stream, date);
            return true;
        }

        private string GetFullPath(Uri uri)
        {
            if (!uri.AbsolutePath.StartsWith(_rootUri))
                return null;

            var relativeUri = Uri.UnescapeDataString(uri.AbsolutePath.Remove(0, _rootUri.Length));
            return Path.Combine(_basePath, relativeUri.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        }

        /// <summary>
        ///     Gets if the specified url corresponds to a directory serving files
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <returns></returns>
        public bool IsDirectory(Uri uri)
        {
            var path = GetFullPath(uri);
            return path != null && Directory.Exists(path);
        }

        public bool FileExists(Uri uri)
        {
            var fullPath = GetFullPath(uri);
            if (fullPath == null)
                return false;

            if (fullPath == _basePath)
                fullPath = GetDefaultFile();

            return File.Exists(fullPath);
        }

        /// <summary>
        ///     Get all files that exists in the specified directory
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <returns></returns>
        public IEnumerable<FileInformation> GetFiles(Uri uri)
        {
            var path = GetFullPath(uri);
            if (path == null)
                yield break;

            if (File.Exists(path))
            {
                var fi = new FileInfo(path);
                yield return new FileInformation
                {
                    LastModifiedAtUtc = fi.LastWriteTimeUtc,
                    Name = Path.GetFileName(path),
                    Size = (int) fi.Length
                };
                yield break;
            }

            if (!Directory.Exists(path))
                yield break;

            foreach (var file in Directory.GetFiles(path, "*.*"))
            {
                var mimeType = MimeTypeProvider.Instance.Get(Path.GetFileName(file));
                if (mimeType == null)
                    continue;

                var info = new FileInfo(file);
                yield return new FileInformation
                {
                    LastModifiedAtUtc = info.LastWriteTimeUtc,
                    Name = Path.GetFileName(file),
                    Size = (int) info.Length
                };
            }
        }

        /// <summary>
        ///     Gets a list of all sub directores
        /// </summary>
        /// <param name="uri">URI (as requested by the HTTP client) which should correspond to a directory.</param>
        /// <returns></returns>
        public IEnumerable<string> GetDirectories(Uri uri)
        {
            var path = GetFullPath(uri) + "\\"; //include a last slash since it's a directoy
            if (path == null || !Directory.Exists(path))
                yield break;

            foreach (var directory in Directory.GetDirectories(path))
            {
                if (directory.StartsWith("."))
                    continue;

                yield return directory.Remove(0, path.Length) + "\\";
            }
        }

        #endregion
    }
}