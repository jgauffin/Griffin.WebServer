using System.Collections.Generic;
using Griffin.Networking.Protocol.Http.Protocol;

namespace Griffin.WebServer.ValueProviders
{
    /// <summary>
    /// Used to load values from the data source
    /// </summary>
    /// <remarks>Fields with exact form name should all be pushed to the same parameter (so that it got several values)</remarks>
    public interface IValueProvider
    {
        /// <summary>
        /// Get a parameter
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <returns>Parameter if found; otherwise <c>null</c>.</returns>
        IParameter Get(string fieldName);

        /// <summary>
        /// Find all parameters which starts with the specified argument.
        /// </summary>
        /// <param name="prefix">Beginning of the field name</param>
        /// <returns>All matching parameters.</returns>
        IEnumerable<IParameter> Find(string prefix);
    }
}