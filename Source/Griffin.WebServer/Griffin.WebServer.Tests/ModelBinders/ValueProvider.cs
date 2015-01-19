using System.Collections.Generic;
using System.Linq;
using Griffin.Net.Protocols.Http;
using Griffin.WebServer.ValueProviders;

namespace Griffin.WebServer.Tests.ModelBinders
{
    public class ValueProvider : IValueProvider
    {
        public ValueProvider()
        {
            Parameters = new List<IParameter>();
        }


        public List<IParameter> Parameters { get; set; }
        public IParameter Get(string fieldName)
        {
            return Parameters.FirstOrDefault(x => x.Name == fieldName);
        }

        public IEnumerable<IParameter> Find(string prefix)
        {
            return Parameters.Where(x => x.Name.StartsWith(prefix));
        }
    }
}