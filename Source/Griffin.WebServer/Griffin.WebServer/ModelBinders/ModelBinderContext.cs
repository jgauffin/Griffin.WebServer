using System;
using Griffin.WebServer.ValueProviders;

namespace Griffin.WebServer.ModelBinders
{
    /// <summary>
    /// Default implementation
    /// </summary>
    public class ModelBinderContext : IModelBinderContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBinderContext" /> class.
        /// </summary>
        /// <param name="modelType">Type of the model (view model type).</param>
        /// <param name="modelName">Name of the model (i.e. property or argument name).</param>
        /// <param name="prefix">The prefix (if this is a nested field like "User.FirstName", prefix = "User.").</param>
        /// <param name="valueProvider">The value provider.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public ModelBinderContext(Type modelType, string modelName, string prefix, IValueProvider valueProvider)
        {
            if (modelType == null)
                throw new ArgumentNullException("modelType", "May not be null or empty.");
            if (valueProvider == null) throw new ArgumentNullException("valueProvider");
            if (string.IsNullOrEmpty(modelName))
                throw new ArgumentNullException("modelName", "May not be null or empty.");

            ValueProvider = valueProvider;
            Prefix = prefix;
            ModelName = modelName;
            ModelType = modelType;
        }


        /// <summary>
        /// Gets or sets prefix for this model in the list
        /// </summary>
        /// <remarks>Prefixes are used to be albe to load items which are deeper down in the graph.</remarks>
        /// <example>User[0].</example>
        public string Prefix { get; set; }


        /// <summary>
        /// Gets type of model which we are currently mapping (i.e. view model type)
        /// </summary>
        public Type ModelType { get; private set; }


        /// <summary>
        /// Gets name of the model. Corresponds to the property name
        /// </summary>
        /// <remarks>
        /// Empty =  root, otherwise the property to load.
        /// </remarks>
        public string ModelName { get;private set; }

        /// <summary>
        /// Gets provider used to load values (for instance from HTTP forms)
        /// </summary>
        public IValueProvider ValueProvider { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IModelBinder RootBinder { get; set; }

        /// <summary>
        /// Execute a child binding
        /// </summary>
        /// <param name="modelType">Child model type</param>
        /// <param name="prefix"></param>
        /// <param name="modelName"></param>
        /// <returns></returns>
        public object Execute(Type modelType, string prefix, string modelName)
        {
            if (RootBinder == null)
                throw new InvalidOperationException("Requires the property RootBinder to have been set first.");

            return RootBinder.Bind(CreateForChild(modelType, prefix, modelName));
        }

        /// <summary>
        /// Create a scope for a child model
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="modelName">Name of the model.</param>
        /// <returns></returns>
        private IModelBinderContext CreateForChild(Type type, string prefix, string modelName)
        {
            var context = new ModelBinderContext(type, modelName, prefix, ValueProvider);
            context.RootBinder = RootBinder;
            return context;
        }
    }
}