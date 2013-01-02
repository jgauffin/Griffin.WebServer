using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;

namespace Griffin.WebServer.Files
{
    /// <summary>
    /// Serves files from the hard drive.
    /// </summary>
    public class DiskFileService : IFileService
    {
        private readonly string _basePath;
        private readonly string _rootUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeFileService"/> class.
        /// </summary>
        /// <param name="rootFilePath">Path to serve files from.</param>
        /// <param name="rootUri">Serve all files which are located under this URI</param>
        /// <example>
        /// <code>
        /// var diskFiles = new DiskFileService("/public/", @"C:\www\public\");
        /// var module = new FileModule(diskFiles);
        /// 
        /// var moduleManager = new ModuleManager();
        /// moduleManager.Add(module);
        /// </code>
        /// </example>
        public DiskFileService(string rootUri, string rootFilePath)
        {
            if (rootUri == null) throw new ArgumentNullException("rootUri");
            if (rootFilePath == null) throw new ArgumentNullException("rootFilePath");
            if (!Directory.Exists(rootFilePath))
                throw new ArgumentOutOfRangeException("rootFilePath", rootFilePath,
                                                      "Failed to find path " + rootFilePath);

            _rootUri = rootUri;
            _basePath = rootFilePath;
        }

        #region IFileService Members

        /// <summary>
        /// Get a file
        /// </summary>
        /// <param name="context">Context used to locate and return files</param>
        public virtual bool GetFile(FileContext context)
        {
            var fullPath = GetFullPath(context.Request.Uri);
            if (fullPath == null || !File.Exists(fullPath))
                return false;


            var date = File.GetLastWriteTimeUtc(fullPath);
            if (date <= context.BrowserCacheDate)
            {
                context.SetNotModified(fullPath, date);
                return true;
            }

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            context.SetFile(fullPath, stream, date);
            return true;
        }

        private string GetFullPath(Uri uri)
        {
            if (!uri.AbsolutePath.StartsWith(_rootUri))
                return null;

            var relativeUri = Uri.UnescapeDataString(uri.AbsolutePath.Remove(0, _rootUri.Length));
            return Path.Combine(_basePath, relativeUri.TrimStart('/').Replace('/', '\\'));
        }

        /// <summary>
        /// Gets if the specified url corresponds to a directory serving files
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <returns></returns>
        public bool IsDirectory(Uri uri)
        {
            var path = GetFullPath(uri);
            return path != null && Directory.Exists(path);

        }

        /// <summary>
        /// Get all files that exists in the specified directory
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <returns></returns>
        public IEnumerable<FileInformation> GetFiles(Uri uri)
        {
            var path = GetFullPath(uri);
            if (path == null || !Directory.Exists(path))
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
                        Size = (int)info.Length
                    };
            }
        }

        /// <summary>
        /// Gets a list of all sub directores
        /// </summary>
        /// <param name="uri">URI (as requested by the HTTP client) which should correspond to a directory.</param>
        /// <returns></returns>
        public IEnumerable<string> GetDirectories(Uri uri)
        {
            var path = GetFullPath(uri);
            if (path == null || !Directory.Exists(path))
                yield break;

            yield return "..";
            foreach (var directory in Directory.GetDirectories(path))
            {
                if (directory.StartsWith("."))
                    continue;

                yield return directory.Remove(0, path.Length);
            }
        }

        #endregion
    }
}