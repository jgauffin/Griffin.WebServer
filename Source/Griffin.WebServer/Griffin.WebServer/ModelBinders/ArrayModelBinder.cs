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
        public bool CanBind(IModelBinderContext context)
        {
            return context.ModelType.IsArray;
        }

        public object Bind(IModelBinderContext context)
        {
            var fieldName = context.Prefix + context.ModelName + "[";
            var arrayType = context.ModelType.GetElementType();

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
            else
            {
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

                var array = Array.CreateInstance(arrayType, fields.Count);
                for (var i = 0; i < fields.Count; i++)
                {
                    var value = context.Execute(arrayType, context.Prefix, fields[i]);
                    array.SetValue(value, i);
                }

                return array;
            }
        }
    }
}