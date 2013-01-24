using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Griffin.WebServer.ModelBinders;
using Xunit;

namespace Griffin.WebServer.Tests.ModelBinders
{
    public class EnumModelBinderTests
    {
        [Fact]
        public void Number_Ok()
        {
            var provider = new ValueProvider() { Parameters = { new Parameter("code", "404") } };
            var context = new ModelBinderContext(typeof(HttpStatusCode), "code", "", provider);

            var binder = new EnumModelBinder();
            var actual = (HttpStatusCode)binder.Bind(context);

            Assert.Equal(HttpStatusCode.NotFound, actual);
        }

        [Fact]
        public void String_Ok()
        {
            var provider = new ValueProvider() { Parameters = { new Parameter("code", "Forbidden") } };
            var context = new ModelBinderContext(typeof(HttpStatusCode), "code", "", provider);

            var binder = new EnumModelBinder();
            var actual = (HttpStatusCode)binder.Bind(context);

            Assert.Equal(HttpStatusCode.Forbidden, actual);
        }
    }
}
