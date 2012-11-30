using System;
using System.Collections.Generic;
using System.Linq;

namespace Griffin.WebServer.Files
{
    /// <summary>
    /// Can serve files from multiple services.
    /// </summary>
    public class CompositeFileService : IFileService
    {
        private readonly IFileService[] _fileServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeFileService"/> class.
        /// </summary>
        /// <param name="fileServices">One or more file services.</param>
        public CompositeFileService(params IFileService[] fileServices)
        {
            _fileServices = fileServices;
        }

        #region IFileService Members

        /// <summary>
        /// Loops through all services and returns the first matching file.
        /// </summary>
        /// <param name="context">Context used to locate and return files</param>
        public virtual bool GetFile(FileContext context)
        {
            foreach (var fileService in _fileServices)
            {
                fileService.GetFile(context);
                if (context.IsFound)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets if the specified url corresponds to a directory serving files
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <returns></returns>
        public bool IsDirectory(Uri uri)
        {
            return _fileServices.Any(service => service.IsDirectory(uri));
        }

        /// <summary>
        /// Get all files that exists in the specified directory
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <returns></returns>
        /// <remarks>Will return all matching files from all services.</remarks>
        public IEnumerable<FileInformation> GetFiles(Uri uri)
        {
            return _fileServices.SelectMany(service => service.GetFiles(uri));
        }

        /// <summary>
        /// Gets a list of all sub directores
        /// </summary>
        /// <param name="uri">URI (as requested by the HTTP client) which should correspond to a directory.</param>
        /// <returns></returns>
        /// <remarks>Will return all matching directories from all inner services.</remarks>
        public IEnumerable<string> GetDirectories(Uri uri)
        {
            return _fileServices.SelectMany(service => service.GetDirectories(uri));
        }

        #endregion
    }
}
