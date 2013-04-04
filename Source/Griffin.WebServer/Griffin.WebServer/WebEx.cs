using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Griffin.WebServer
{
    /// <summary>
    /// Regular expression bindings for URIs
    /// </summary>
    /// <remarks>The purpose of this class is to map generic regular expressions to simplify url routing</remarks>
    public class WebEx
    {
        Dictionary<string, string>  _aliases = new Dictionary<string, string>();
        private readonly Regex TranslateRegEx = new Regex(@"/(\/)(\{(\w+)([\:\w]+)\})");

        public WebEx()
        {
            CreateAlias("notslash", "[^/]+");
        }
        public void CreateAlias(string name, string regExPattern)
        {

            var ex = new Regex(regExPattern);
            try
            {
                ex.Match("a");
            }
            catch (Exception err)
            {
                
            }

            _aliases[name] = regExPattern;
        }

        public Regex Resolve(string uriPattern)
        {
            var matches = TranslateRegEx.Replace(uriPattern, DoReplacements);
            return null;
        }

        private string DoReplacements(Match match)
        {
            return "";

        }
    }
}
