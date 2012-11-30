using System;

namespace Griffin.WebServer.Files
{
    /// <summary>
    /// Small DTO for files
    /// </summary>
    public class FileInformation
    {
        /// <summary>
        /// Gets file name including extension but no path
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets when the file was modified
        /// </summary>
        public DateTime LastModifiedAtUtc { get; set; }

        /// <summary>
        /// Gets file size in bytes.
        /// </summary>
        public int Size { get; set; }
    }
}