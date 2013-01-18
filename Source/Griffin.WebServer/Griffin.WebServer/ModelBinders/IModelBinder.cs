namespace Griffin.WebServer.ModelBinders
{
    /// <summary>
    /// Used to bind the string values (in the request) to a model of some sorts.
    /// </summary>
    public interface IModelBinder
    {
        /// <summary>
        /// Determines whether this instance can bind the specified model.
        /// </summary>
        /// <param name="context">Context infromation.</param>
        /// <returns>
        ///   <c>true</c> if this instance can handle the model; otherwise <c>false</c>.
        /// </returns>
        bool CanBind(IModelBinderContext context);

        /// <summary>
        /// Bind the model
        /// </summary>
        /// <param name="context">Context information</param>
        /// <returns>An object of the specified type (<seealso cref="IModelBinderContext.ModelType)"/></returns>
        object Bind(IModelBinderContext context);
    }
}