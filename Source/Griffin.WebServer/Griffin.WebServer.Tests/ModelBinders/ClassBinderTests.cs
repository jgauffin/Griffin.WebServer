using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.WebServer.ModelBinders;
using Griffin.WebServer.Tests.ValueBinders;
using Xunit;

namespace Griffin.WebServer.Tests.ModelBinders
{
    public class ClassBinderTests
    {
        [Fact]
        public void InvalidFieldType()
        {
            var provider = new ValueProvider
            {
                Parameters =
                        {
                            new Parameter("user.FirstName", "jonas"),
                            new Parameter("user.Age", "arne")
                        }
            };
            var mapper = new ModelMapper();
            mapper.Clear();
            mapper.AddBinder(new PrimitiveModelBinder());
            var context = new ModelBinderContext(typeof(UserViewModel), "user", "", provider) { RootBinder = mapper };

            var binder = new ClassBinder();

            Assert.Throws<ModelBindingException>(() => binder.Bind(context));
        }

        [Fact]
        public void UnknownField()
        {
            var provider = new ValueProvider
            {
                Parameters =
                        {
                            new Parameter("user.FirstName", "jonas"),
                            new Parameter("user.NotInventedHere", "23")
                        }
            };
            var mapper = new ModelMapper();
            mapper.Clear();
            mapper.AddBinder(new PrimitiveModelBinder());
            var context = new ModelBinderContext(typeof(UserViewModel), "user", "", provider) { RootBinder = mapper };

            var binder = new ClassBinder();
            var actual = (UserViewModel)binder.Bind(context);

            Assert.Equal("jonas", actual.FirstName);
        }

        [Fact]
        public void NoPublicDefaultConstructor()
        {
            var provider = new ValueProvider
            {
                Parameters =
                        {
                            new Parameter("user.FirstName", "jonas"),
                        }
            };
            var mapper = new ModelMapper();
            mapper.Clear();
            mapper.AddBinder(new PrimitiveModelBinder());
            var context = new ModelBinderContext(typeof(Test), "user", "", provider) { RootBinder = mapper };

            var binder = new ClassBinder();

            Assert.Throws<ModelBindingException>(() => binder.Bind(context));
        }

        public class Test
        {
            public Test(int someValue)
            {
                
            }

            public string FirstName { get; set; }
        }
    }
}
