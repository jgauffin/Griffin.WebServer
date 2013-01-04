using System;

namespace Griffin.WebServer.Modules
{
    /// <summary>
    /// A HTTP module which do something useful with the request.
    /// </summary>
    public interface IWorkerModule : IHttpModule
    {
        /// <summary>
        /// Handle the request.
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="callback">Invoked when the module has completed.</param>
        /// <returns><see cref="ModuleResult.Stop"/> will stop all processing except <see cref="IHttpModule.EndRequest"/>.</returns>
        /// <remarks>Invoked in turn for all modules unless you return <see cref="ModuleResult.Stop"/>.</remarks>
        void HandleRequestAsync(IHttpContext context, Action<IAsyncModuleResult> callback);
    }
}