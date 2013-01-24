using System;
using Griffin.WebServer.ValueProviders;

namespace Griffin.WebServer.ModelBinders
{
    /// <summary>
    /// Context information for <seealso cref="IModelBinder"/>.
    /// </summary>
    public interface IModelBinderContext
    {
        /// <summary>
        /// Gets or sets prefix for this model in the list
        /// </summary>
        /// <remarks>Prefixes are used to be albe to load items which are deeper down in the graph.</remarks>
        /// <example>User[0].</example>
        string Prefix { get; }

        /// <summary>
        /// Gets type of model which we are currently mapping (i.e. view model type)
        /// </summary>
        Type ModelType { get; }

        /// <summary>
        /// Gets name of the model. Corresponds to the property or argument name
        /// </summary>
        /// <remarks>Empty =  root, otherwise the property to load.</remarks>
        string ModelName { get; }

        /// <summary>
        /// Gets provider used to load values (for instance from HTTP forms)
        /// </summary>
        IValueProvider ValueProvider { get; }

        /// <summary>
        /// Execute another binder
        /// </summary>
        /// <param name="modelType">Type that we want to get</param>
        /// <param name="prefix">Prefix (if this is a nested get)</param>
        /// <param name="modelName">Property/argument name</param>
        /// <returns>Created model</returns>
        object Execute(Type modelType, string prefix, string modelName);
    }
}