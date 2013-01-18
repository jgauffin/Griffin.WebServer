using System;

namespace Griffin.WebServer.ModelBinders
{
    /// <summary>
    /// Can bind enums
    /// </summary>
    public class EnumBinder : IModelBinder
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
            return context.ModelType.IsEnum;
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
            var parameter = context.ValueProvider.Get(context.Prefix + context.ModelName);
            if (parameter == null || string.IsNullOrEmpty(parameter.Value))
                return 0;


            return char.IsDigit(parameter.Value[0])
                       ? Convert.ChangeType(parameter.Value, context.ModelType.UnderlyingSystemType)
                       : Enum.Parse(context.ModelType, parameter.Value, true);
        }
    }
}