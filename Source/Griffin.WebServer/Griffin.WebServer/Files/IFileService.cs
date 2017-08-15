using System;
using System.Collections.Generic;

namespace Griffin.WebServer.Files
{
    /// <summary>
    ///     Serves files
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        ///     Gets a list of all sub directores
        /// </summary>
        /// <param name="uri">URI (as requested by the HTTP client) which should correspond to a directory.</param>
        /// <returns></returns>
        [Obsolete("Use GetDirectoryList")]
        IEnumerable<string> GetDirectories(Uri uri);

        /// <summary>
        ///     Gets a list of all sub directores
        /// </summary>
        /// <param name="uri">URI (as requested by the HTTP client) which should correspond to a directory.</param>
        /// <returns></returns>
        IEnumerable<string> GetDirectoryList(Uri uri);

        /// <summary>
        ///     Download a file.
        /// </summary>
        /// <param name="context">Context used to locate and return files</param>
        /// <remarks><c>true</c> if the file was attached to the response; otherwise false;</remarks>
        [Obsolete("Use DownloadFile")]
        bool GetFile(FileContext context);

        /// <summary>
        ///     Download a file.
        /// </summary>
        /// <param name="context">Context used to locate and return files</param>
        /// <remarks><c>true</c> if the file was attached to the response; otherwise false;</remarks>
        bool DownloadFile(FileContext context);

        /// <summary>
        ///     Get information about all files that exists in the specified directory.
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <returns></returns>
        [Obsolete("Use DownloadFile")]
        IEnumerable<FileInformation> GetFiles(Uri uri);

        /// <summary>
        ///     Get information about all files that exists in the specified directory.
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <returns></returns>
        IEnumerable<FileInformation> GetFileList(Uri uri);


        /// <summary>
        ///     Gets if the specified url corresponds to a directory serving files.
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <returns></returns>
        bool IsDirectory(Uri uri);

        /// <summary>
        /// Check if the file corresponds to a file.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        bool FileExists(Uri uri);
    }
}