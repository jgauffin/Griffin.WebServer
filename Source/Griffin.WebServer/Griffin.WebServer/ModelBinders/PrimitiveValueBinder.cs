using System;

namespace Griffin.WebServer.ModelBinders
{
    /// <summary>
    /// Can bind primitives and string
    /// </summary>
    public class PrimitiveValueBinder : IModelBinder
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
            return context.ModelType.IsPrimitive || context.ModelType == typeof (string);
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
            var name = string.IsNullOrEmpty(context.Prefix)
                           ? context.ModelName
                           : context.Prefix + "." + context.ModelName;
            var parameter = context.ValueProvider.Get(name);
            if (parameter == null)
                return null;

            object value = parameter.Value;
            if (!context.ModelType.IsAssignableFrom(typeof(string)))
            {
                value = Convert.ChangeType(value, context.ModelType);
            }

            return value;
        }
    }
}