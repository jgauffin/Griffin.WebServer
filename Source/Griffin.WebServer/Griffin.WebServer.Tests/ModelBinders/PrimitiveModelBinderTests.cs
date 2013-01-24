using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Griffin.WebServer.ModelBinders;
using Xunit;

namespace Griffin.WebServer.Tests.ModelBinders
{
    public class PrimitiveModelBinderTests
    {
        [Fact]
        public void Int()
        {
            var provider = new ValueProvider() { Parameters = { new Parameter("age", "3")}};
            var context = new ModelBinderContext(typeof(int), "age", "", provider);

            var binder = new PrimitiveModelBinder();
            var actual = (int)binder.Bind(context);

            Assert.Equal(3, actual);
        }

        [Fact]
        public void Double()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(1033);
            var provider = new ValueProvider() { Parameters = { new Parameter("age", "3.5") } };
            var context = new ModelBinderContext(typeof(double), "age", "", provider);

            var binder = new PrimitiveModelBinder();
            var actual = (double)binder.Bind(context);

            Assert.Equal(3.5, actual);
        }



        [Fact]
        public void String()
        {
            var provider = new ValueProvider() { Parameters = { new Parameter("age", "Why do you need to know???") } };
            var context = new ModelBinderContext(typeof(string), "age", "", provider);

            var binder = new PrimitiveModelBinder();
            var actual = (string)binder.Bind(context);

            Assert.Equal("Why do you need to know???", actual);
        }

    }
}
