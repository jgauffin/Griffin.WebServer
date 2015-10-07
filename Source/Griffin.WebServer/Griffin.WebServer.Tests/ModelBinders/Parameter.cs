using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Griffin.Net.Protocols.Http;

namespace Griffin.WebServer.Tests.ModelBinders
{
    public class Parameter : IParameter
    {
        public List<string> Values { get; set; }
        public Parameter(string name, string value)
        {
            Values = new List<string>(){value};
            Name = name;
        }
        public Parameter(string name, IEnumerable<string> values)
        {
            Values = values.ToList();
            Name = name;
        }

        public Parameter(string name, params string[] values)
        {
            Values = values.ToList();
            Name = name;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        public void Add(string value)
        {
            Values.Add(value);
        }

        public string Value { get { return Values.LastOrDefault(); } }
        public string Name { get; private set; }

        public string this[int index]
        {
            get { return Values[index]; }
        }

        public int Count { get { return Values.Count; } }
    }
}