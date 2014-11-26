using System.Text;
using Griffin.Net.Protocols.Http;

namespace Griffin.WebServer
{
    /// <summary>
    /// Used to build error info.
    /// </summary>
    internal static class RequestExtensions
    {
        /// <summary>
        /// Generate error information string including query string, form and cookies.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string BuildErrorInfo(this IHttpRequest request)
        {
            var sb = new StringBuilder();
            sb.AppendLine("URI: " + request.Uri);

            sb.AppendLine("Querystring: " + request.Uri.Query);

            var withForm = request as IHttpMessageWithForm;
            if (withForm != null)
            {
                sb.AppendLine("Form");
                foreach (var kvp in withForm.Form)
                {
                    sb.AppendFormat("{0}: {1}\r\n", kvp, kvp.Value);
                }

            }

            //TODO: Create an interface.
            var req = request as HttpRequest;
            if (req != null)
            {
                sb.AppendLine("Cookies");
                foreach (var kvp in req.Cookies)
                {
                    sb.AppendFormat("{0}: {1}\r\n", kvp, kvp.Value);
                }
            }

            return sb.ToString();
        }
    }
}