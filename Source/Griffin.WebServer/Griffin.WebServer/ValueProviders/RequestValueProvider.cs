using System.Collections.Generic;
using System.Linq;
using Griffin.Networking.Protocol.Http.Protocol;

namespace Griffin.WebServer.ValueProviders
{
    /// <summary>
    /// Can provide values from HTTP requests.
    /// </summary>
    public class RequestValueProvider : IValueProvider
    {
        private readonly IRequest _request;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestValueProvider" /> class.
        /// </summary>
        /// <param name="request">The request.</param>
        public RequestValueProvider(IRequest request)
        {
            _request = request;
        }

        /// <summary>
        /// Get a parameter
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <returns>
        /// Parameter if found; otherwise <c>null</c>.
        /// </returns>
        public IParameter Get(string fieldName)
        {
            return _request.Form.Get(fieldName);
        }

        /// <summary>
        /// Find all parameters which starts with the specified argument.
        /// </summary>
        /// <param name="prefix">Beginning of the field name</param>
        /// <returns>
        /// All matching parameters.
        /// </returns>
        public IEnumerable<IParameter> Find(string prefix)
        {
            return _request.Form.Where(x => x.Name.StartsWith(prefix));
        }
    }
}