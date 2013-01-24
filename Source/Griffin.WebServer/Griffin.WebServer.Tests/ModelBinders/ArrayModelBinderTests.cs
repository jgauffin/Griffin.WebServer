using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Griffin.Networking.Protocol.Http.Protocol;
using Griffin.WebServer.ModelBinders;
using Griffin.WebServer.Tests.ValueBinders;
using NSubstitute;
using Xunit;

namespace Griffin.WebServer.Tests.ModelBinders
{
    public class ArrayModelBinderTests
    {
        [Fact]
        public void CanBind_IntArray()
        {
            var context = Substitute.For<IModelBinderContext>();
            context.ModelType.Returns(typeof (int[]));
            var binder = new ArrayModelBinder();

            Assert.True(binder.CanBind(context));
        }

        [Fact]
        public void CanBind_Primitive()
        {
            var context = Substitute.For<IModelBinderContext>();
            context.ModelType.Returns(typeof(int));
            var binder = new ArrayModelBinder();

            Assert.False(binder.CanBind(context));
        }

        [Fact]
        public void CanBind_List()
        {
            var context = Substitute.For<IModelBinderContext>();
            context.ModelType.Returns(typeof(IEnumerable<int>));
            var binder = new ArrayModelBinder();

            Assert.False(binder.CanBind(context));
        }

        [Fact]
        public void Bind_IntArray_NoIndex()
        {
            var provider = new ValueProvider() {Parameters = {new Parameter("ages[]", "3", "20")}};
            var context = new ModelBinderContext(typeof(int[]), "ages", "", provider);

            var binder = new ArrayModelBinder();
            var actual = (int[])binder.Bind(context);

            Assert.NotNull(actual);
            Assert.Equal(3, actual[0]);
            Assert.Equal(20, actual[1]);
        }

        [Fact]
        public void Bind_IntArray_Index()
        {
            var provider = new ValueProvider() { Parameters = { new Parameter("ages[1]", "3"), new Parameter("ages[0]", "20") } };
            var context = new ModelBinderContext(typeof(int[]), "ages", "", provider);
            context.RootBinder = new PrimitiveModelBinder();
            var binder = new ArrayModelBinder();
            var actual = (int[])binder.Bind(context);

            Assert.NotNull(actual);
            Assert.Equal(3, actual[1]);
            Assert.Equal(20, actual[0]);
        }

        [Fact]
        public void Bind_IntArray_Gap()
        {
            var provider = new ValueProvider() { Parameters = { new Parameter("ages[2]", "3"), new Parameter("ages[0]", "20") } };
            var context = new ModelBinderContext(typeof(int[]), "ages", "", provider);
            context.RootBinder = new PrimitiveModelBinder();
            
            var binder = new ArrayModelBinder();
            
            Assert.Throws<ModelBindingException>(() => binder.Bind(context));
        }

        [Fact]
        public void Bind_ViewModelArray_Index()
        {
            var provider = new ValueProvider
                {
                    Parameters =
                        {
                            new Parameter("user[1].FirstName", "jonas"),
                            new Parameter("user[0].FirstName", "Arne"),
                            new Parameter("user[0].Age", "32"),
                            new Parameter("user[1].Age", "23")
                        }
                };
            var mapper = new ModelMapper();
            mapper.Clear();
            mapper.AddBinder(new PrimitiveModelBinder());
            mapper.AddBinder(new ClassBinder());
            var context = new ModelBinderContext(typeof(UserViewModel[]), "user", "", provider) {RootBinder = mapper};

            var binder = new ArrayModelBinder();
            var actual = (UserViewModel[])binder.Bind(context);

            Assert.NotNull(actual);
            Assert.Equal("jonas", actual[1].FirstName);
            Assert.Equal("Arne", actual[0].FirstName);
            Assert.Equal(23, actual[1].Age);
            Assert.Equal(32, actual[0].Age);
        }

        [Fact]
        public void Bind_ViewModelArray_Nested()
        {
            var provider = new ValueProvider
            {
                Parameters =
                        {
                            new Parameter("Rating.Readers[1].FirstName", "jonas"),
                            new Parameter("Rating.Readers[0].FirstName", "Arne"),
                            new Parameter("Rating.Readers[0].Age", "32"),
                            new Parameter("Rating.Readers[1].Age", "23")
                        }
            };
            var mapper = new ModelMapper();
            mapper.Clear();
            mapper.AddBinder(new PrimitiveModelBinder());
            mapper.AddBinder(new ClassBinder());
            var context = new ModelBinderContext(typeof(UserViewModel[]), "Readers", "Rating.", provider) { RootBinder = mapper };

            var binder = new ArrayModelBinder();
            var actual = (UserViewModel[])binder.Bind(context);

            Assert.NotNull(actual);
            Assert.Equal("jonas", actual[1].FirstName);
            Assert.Equal("Arne", actual[0].FirstName);
            Assert.Equal(23, actual[1].Age);
            Assert.Equal(32, actual[0].Age);
        }

        [Fact]
        public void Bind_ViewModelArray_Gap()
        {
            var provider = new ValueProvider
            {
                Parameters =
                        {
                            new Parameter("user[1].FirstName", "jonas"),
                            new Parameter("user[3].FirstName", "Arne"),
                            new Parameter("user[0].Age", "32"),
                            new Parameter("user[1].Age", "23")
                        }
            };
            var mapper = new ModelMapper();
            mapper.Clear();
            mapper.AddBinder(new PrimitiveModelBinder());
            mapper.AddBinder(new ClassBinder());
            var context = new ModelBinderContext(typeof(UserViewModel[]), "user", "", provider) { RootBinder = mapper };

            var binder = new ArrayModelBinder();

            Assert.Throws<ModelBindingException>(() => binder.Bind(context));
        }  
    }
}
