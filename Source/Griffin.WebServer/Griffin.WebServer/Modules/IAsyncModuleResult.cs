using System;

namespace Griffin.WebServer.Modules
{
    /// <summary>
    /// Response from modules for async operations.
    /// </summary>
    public interface IAsyncModuleResult
    {
        /// <summary>
        /// Gets HTTP context which the reply is for.
        /// </summary>
        IHttpContext Context { get; }

        /// <summary>
        /// Gets how the module thinks that the processing went.
        /// </summary>
        ModuleResult Result { get; set; }

        /// <summary>
        /// Gets any exception which was caught during the async operation
        /// </summary>
        /// <remarks>It's prefered that the async op itself uses a try/catch to set this exception</remarks>
        Exception Exception { get; set; }
    }
}