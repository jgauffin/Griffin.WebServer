using System;

namespace Griffin.WebServer.Modules
{
    /// <summary>
    /// Implementation of <see cref="IAsyncModuleResult"/>
    /// </summary>
    public class AsyncModuleResult : IAsyncModuleResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncModuleResult" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public AsyncModuleResult(IHttpContext context, ModuleResult result)
        {
            Context = context;
            Result = result;
        }

        /// <summary>
        /// Gets HTTP context which the reply is for.
        /// </summary>
        public IHttpContext Context { get; private set; }

        /// <summary>
        /// Gets how the module thinks that the processing went.
        /// </summary>
        public ModuleResult Result { get; set; }

        /// <summary>
        /// Gets any exception which was caught during the async operation
        /// </summary>
        /// <remarks>It's prefered that the async op itself uses a try/catch to set this exception</remarks>
        public Exception Exception { get; set; }
    }
}