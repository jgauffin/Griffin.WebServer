using System;
using System.Collections.Generic;
using System.Linq;

namespace Griffin.WebServer.ModelBinders
{
    /// <summary>
    /// Can convert simple and complex arrays to view models
    /// </summary>
    /// <remarks>
    /// <para>
    /// complex arrays mean that the actual type is a view model. Those models must an zero based indexer to be populated
    /// </para>
    /// <para>Simple arrays consistes of a single dimension and being primitive. These do not need an index</para>
    /// </remarks>
    /// <example>
    /// Simple array:
    /// <code>
    /// int[] ages
    /// </code>
    /// would be populated by:
    /// <code>
    /// <![CDATA[
    /// <input type="Age[]" value="10" />
    /// <input type="Age[]" value="8" />
    /// <input type="Age[]" value="32" />
    /// ]]>
    /// </code>
    /// Complex example:
    /// <code>User[] user</code>
    /// would be populated by:
    /// <code>
    /// <![CDATA[
    /// <input type="User[0].FirstName" value="Arne" />
    /// <input type="User[0].Age" value="32" />
    /// <input type="User[1].FirstName" value="Bertial" />
    /// <input type="User[1].Age" value="4" />
    /// ]]>
    /// The index is important so that we can map the correct grouped files together.
    /// </code>
    /// </example>
    /// <seealso cref="DictionaryModelBinder"/>
    public class ArrayModelBinder : IModelBinder
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
            if (context == null) throw new ArgumentNullException("context");
            return context.ModelType.IsArray;
        }

        /// <summary>
        /// Bind the model
        /// </summary>
        /// <param name="context">Context information</param>
        /// <returns>
        /// An object of the specified type (<seealso cref="IModelBinderContext.ModelType)" />
        /// </returns>
        /// <exception cref="System.FormatException"></exception>
        public object Bind(IModelBinderContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            var fieldName = context.Prefix + context.ModelName + "[";
            if (string.IsNullOrEmpty(fieldName))
                throw new ModelBindingException("Either IModelBinderContext.ModelName or IModelBinderContext.Prefix has to be specified");

            if (context.ModelType == null)
                throw new ModelBindingException("IModelBinderContext.ModelType has to be specified.");

            var arrayType = context.ModelType.GetElementType();
            if (arrayType == null)
                throw new ModelBindingException("Failed to find the element type.", context.ModelType.FullName, null);

            var singleField = context.ValueProvider.Get(fieldName + "]");
            if (singleField != null)
            {
                var array = Array.CreateInstance(arrayType, singleField.Count);
                for (var i = 0; i < array.Length; i++)
                {
                    array.SetValue(Convert.ChangeType(singleField[i], arrayType), i);
                }
                return array;
            }

            return BuildIndexedArray(context, fieldName, arrayType);
        }

        private static object BuildIndexedArray(IModelBinderContext context, string fieldName, Type arrayType)
        {
            var fields = new List<string>();
            var indexes = new List<int>();

            foreach (var item in context.ValueProvider.Find(fieldName))
            {
                var pos = item.Name.IndexOf(']', fieldName.Length);
                if (pos == -1)
                    throw new ModelBindingException("Expected to find ']' to mark end of array.", fieldName + "]", item.Name);

                var name = item.Name.Substring(0, pos + 1);
                if (!fields.Contains(name))
                {
                    fields.Add(name);

                    var index = ExtractIndex(name, fieldName);
                    indexes.Add(index);
                }
            }

            if (!fields.Any())
                return null;

            ValidateIndexes(indexes, fieldName);

            var array = Array.CreateInstance(arrayType, fields.Count);
            for (var i = 0; i < fields.Count; i++)
            {
                var index = indexes[i];

                //prefix already includes the field
                var value = context.Execute(arrayType, "", fields[i]);
                array.SetValue(value, index);
            }

            return array;
        }

        private static void ValidateIndexes(IEnumerable<int> indexes, string fieldName)
        {
            var sorted = indexes.OrderBy(x => x);
            var next = 1;
            sorted.All(x =>
                {
                    if (x + 1 != next)
                        throw new ModelBindingException("Gap in array: " + x, fieldName + "]", null);
                    next++;
                    return true;
                });
        }

        private static int ExtractIndex(string name, string fieldName)
        {
            var pos = name.IndexOf('[');
            if (pos == -1)
                throw new ModelBindingException("Failed to find '['.", fieldName + "]", name);

            name = name.Remove(0, pos + 1);
            pos = name.IndexOf(']');
            if (pos == -1)
                throw new ModelBindingException("Failed to find ']'.", fieldName + "]", name);

            name = name.Remove(pos);
            var index = 0;
            if (!int.TryParse(name, out index))
                throw new ModelBindingException("Failed to get indexer value.", name + "]", index);
            return index;
        }
    }
}