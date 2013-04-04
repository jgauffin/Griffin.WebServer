using System;

namespace Griffin.WebServer.ModelBinders
{
    /// <summary>
    /// Can bind classes which are not abstract or generic.
    /// </summary>
    public class ClassBinder : IModelBinder
    {
        /// <summary>
        /// Determines whether this instance can bind the specified model.
        /// </summary>
        /// <param name="context">Context infromation.</param>
        /// <returns>
        ///   <c>true</c> if this instance can handle the model; otherwise <c>false</c>.
        /// </returns>
        public bool CanBind(IModelBinderContext context)
        {
            return context.ModelType.IsClass 
                && !context.ModelType.IsAbstract 
                && !context.ModelType.IsGenericType;
        }

        /// <summary>
        /// Bind the model
        /// </summary>
        /// <param name="context">Context information</param>
        /// <returns>
        /// An object of the specified type (<seealso cref="IModelBinderContext.ModelType)" />
        /// </returns>
        public object Bind(IModelBinderContext context)
        {
            if (context.ModelType.GetConstructor(new Type[0]) == null)
                throw new ModelBindingException("Do not have a default constructor.", context.ModelName, context.ModelType);

            var model = Activator.CreateInstance(context.ModelType);
            var prefix = string.IsNullOrEmpty(context.Prefix) ? context.ModelName : context.Prefix + "." + context.ModelName;
            foreach (var property in context.ModelType.GetProperties())
            {
                if (!property.CanWrite)
                    continue;

                var value = context.Execute(property.PropertyType, prefix, property.Name);

                
                property.SetValue(model, value, null);

            }

            return model;
        }
    }
}