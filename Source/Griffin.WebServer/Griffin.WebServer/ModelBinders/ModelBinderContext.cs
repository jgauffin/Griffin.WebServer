using System;
using Griffin.WebServer.ValueProviders;

namespace Griffin.WebServer.ModelBinders
{
    public class ModelBinderContext : IModelBinderContext
    {
        public ModelBinderContext(Type modelType, string modelName, string prefix, IValueProvider valueProvider)
        {
            ValueProvider = valueProvider;
            Prefix = prefix;
            ModelName = modelName;
            ModelType = modelType;
        }

        /// <summary>
        /// Gets or sets prefix for this model in the list
        /// </summary>
        public string Prefix { get; set; }

        public Type ModelType { get; set; }

        public string ModelName { get; set; }

        public IValueProvider ValueProvider { get; set; }

        public IModelBinder RootBinder { get; set; }

        public object Execute(Type modelType, string prefix, string modelName)
        {
            return RootBinder.Bind(CreateForChild(modelType, prefix, modelName));
        }

        public IModelBinderContext CreateForChild(Type type, string prefix, string modelName)
        {
            var context = new ModelBinderContext(type, modelName, prefix, ValueProvider);
            context.RootBinder = RootBinder;
            return context;
        }
    }
}