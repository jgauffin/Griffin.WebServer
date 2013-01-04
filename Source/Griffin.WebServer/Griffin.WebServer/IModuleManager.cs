using System;
using Griffin.WebServer.Modules;

namespace Griffin.WebServer
{
    /// <summary>
    /// Takes care of the module execution.
    /// </summary>
    /// <remarks>Will catch all exceptions and also log them including the request information. 
    /// 
    /// It will however not do anything with the exception. You either have to have an error module which checks <see cref="IHttpContext.LastException"/>
    /// in <c>EndRequest()</c> or override the server to handle the error in it.
    /// <para>Modules are invoked in the following order
    /// <list type="number">
    /// <item><see cref="IHttpModule.BeginRequest"/></item>
    /// <item><see cref="IRoutingModule"/></item>
    /// <item><see cref="IAuthenticationModule"/></item>
    /// <item><see cref="IAuthorizationModule"/></item>
    /// <item><see cref="IWorkerModule"/></item>
    /// <item><see cref="IHttpModule.EndRequest"/></item>
    /// </list>
    /// </para>
    /// </remarks>
    public interface IModuleManager
    {
        /// <summary>
        /// Add a HTTP module
        /// </summary>
        /// <param name="module">Module to include</param>
        /// <remarks>Modules are executed in the order they are added.</remarks>
        void Add(IHttpModule module);

        /// <summary>
        /// Handle the request asynchronously.
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="callback">Callback to invoke when the processing is complete </param>
        void InvokeAsync(IHttpContext context, Action<IAsyncModuleResult> callback);
    }
}