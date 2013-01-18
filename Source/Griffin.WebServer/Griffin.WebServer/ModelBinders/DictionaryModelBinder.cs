using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Griffin.WebServer.ModelBinders
{
    /// <summary>
    /// Bind dictionaries.
    /// </summary>
    /// <remarks><para>The key must be of string type.</para>
    /// <para>Used to find arrays which string indexes (like "Users[jonas].FirstName"). You can also
    /// bind arrays with string indexes by using single quotes around them like "Users['1'].FirstName". Useful if you
    /// want to map using Ids and not array indices.</para>
    /// </remarks>
    /// <example>
    /// You can use string keys:
    /// <code>
    /// <![CDATA[
    /// <input type="User['USR0111'].FirstName" value="Arne" />
    /// <input type="User[USR0111].Age" value="32" />
    /// <input type="User[USR37].FirstName" value="Bertial" />
    /// <input type="User[USR37].Age" value="4" />
    /// ]]>
    /// </code>
    /// Or numbers:
    /// <code>
    /// <![CDATA[
    /// <input type="User['111'].FirstName" value="Arne" />
    /// <input type="User['111'].Age" value="32" />
    /// <input type="User['37'].FirstName" value="Bertial" />
    /// <input type="User['37'].Age" value="4" />
    /// ]]>
    /// </code>
    /// </example>
    public class DictionaryModelBinder : IModelBinder
    {
        /// <summary>
        /// Determines whether this instance can bind the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        ///   <c>true</c> if this instance is an dictionary with a string key; otherwise <c>false</c>.
        /// </returns>
        public bool CanBind(IModelBinderContext context)
        {
            if (context.ModelType.IsGenericType)
            {
                var modelType = context.ModelType.GetGenericTypeDefinition();
                if (context.ModelType.GetGenericArguments()[0] != typeof (string))
                    return false;

                return modelType.IsClass
                           ? typeof (Dictionary<,>).IsAssignableFrom(modelType)
                           : typeof (IDictionary<,>).IsAssignableFrom(modelType);
            }

            return false;
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
            var method = GetType().GetMethod("BindGeneric", BindingFlags.Instance | BindingFlags.NonPublic);
            var genMethod = method.MakeGenericMethod(context.ModelType.GetGenericArguments()[1]);
            return genMethod.Invoke(this, new object[] {context});
        }

        private object BindGeneric<TValueType>(IModelBinderContext context)
        {
            var fieldName = context.Prefix + context.ModelName + "[";
            var fields = new List<string>();
            foreach (var item in context.ValueProvider.Find(fieldName))
            {
                var pos = item.Name.IndexOf(']', fieldName.Length);
                if (pos == -1)
                    throw new FormatException("Expected to find ']' to mark end of array in " + item.Name);

                var name = item.Name.Substring(0, pos + 1);
                if (!fields.Contains(name))
                    fields.Add(name);
            }
            if (!fields.Any())
                return null;

            var model = new Dictionary<string, TValueType>();
            for (var i = 0; i < fields.Count; i++)
            {
                var value = (TValueType)context.Execute(typeof(TValueType), context.Prefix, fields[i]);
                var pos = fields[i].IndexOf('[');
                var indexer = fields[i].Substring(pos + 1).TrimEnd(']').Trim('\'');
                model.Add(indexer, value);
            }
            return model;

        }
    }
}