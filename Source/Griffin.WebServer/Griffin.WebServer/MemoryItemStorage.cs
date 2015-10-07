using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Griffin.WebServer
{
    /// <summary>
    /// Uses a Dictionary to store all items
    /// </summary>
    public class MemoryItemStorage : IItemStorage
    {
        private readonly ConcurrentDictionary<string, object> _dictionary =
            new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        #region IItemStorage Members

        /// <summary>
        /// Get or set an item
        /// </summary>
        /// <param name="name">Item name</param>
        /// <returns>Item if found; otherwise <c>null</c>.</returns>
        public object this[string name]
        {
            get
            {
                if (name == null)
                    throw new ArgumentNullException("name");
                object value;
                return _dictionary.TryGetValue(name, out value) ? value : null;
            }
            set
            {
                if (name == null)
                    throw new ArgumentNullException("name");

                object val;
                if (value == null)
                    _dictionary.TryRemove(name, out val);
                else
                    _dictionary[name] = value;
            }
        }

        #endregion
    }
}