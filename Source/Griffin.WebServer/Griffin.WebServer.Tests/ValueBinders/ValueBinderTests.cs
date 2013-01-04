using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.Networking.Protocol.Http.Implementation;
using Griffin.Networking.Protocol.Http.Protocol;
using Xunit;

namespace Griffin.WebServer.Tests.ValueBinders
{
    /*
     * UserName  -> Model directory
     * User.UserName -> nested model
     * User[0].UserName -> Assoc array, Model must be an IEnumerable/
     * User.Messages[0].Name => Model -> AssocArray -> Model
     * 
     * 
     * */
    public class ValueBinderTests
    {
        [Fact]
        public void SimpleMapping()
        {
            var request = new HttpRequest();
            request.Form.Add("UserName", "Jonas");
            request.Form.Add("FirstName", "Arne");

            var modelMapper = new ModelMapper();
            var user = (UserViewModel)modelMapper.Map(typeof(UserViewModel), request, "");

            Assert.Equal("Jonas", user.UserName);
            Assert.Equal("Arne", user.FirstName);
        }

        [Fact]
        public void Nested()
        {
            var request = new HttpRequest();
            request.Form.Add("Author.UserName", "Jonas");
            request.Form.Add("Author.FirstName", "Arne");
            request.Form.Add("Rating", "22");

            var modelMapper = new ModelMapper();
            var actual = (RatingViewModel)modelMapper.Map(typeof(RatingViewModel), request, "");

            Assert.Equal("Jonas", actual.Author.UserName);
            Assert.Equal("Arne", actual.Author.FirstName);
        }

        [Fact]
        public void SimpleArray()
        {
            var request = new HttpRequest();
            request.Form.Add("Ages[]", "8");
            request.Form.Add("Ages[]", "32");

            var modelMapper = new ModelMapper();
            var actual = (SimpleArrayViewModel)modelMapper.Map(typeof(SimpleArrayViewModel), request, "");

            Assert.Equal(8, actual.Ages[0]);
            Assert.Equal(32, actual.Ages[1]);
        }
    }

    public class ModelMapper
    {
        public object Map(Type type, IRequest request, string prefix)
        {
            var model = Activator.CreateInstance(type);
            foreach (var property in model.GetType().GetProperties())
            {
                object value = null;

                // todo: Create a value converter for all .NET types.
                if (!property.PropertyType.IsPrimitive && property.PropertyType != typeof(string))
                {
                    prefix += property.Name + ".";
                    value = Map(property.PropertyType, request, prefix);
                }
                else if (property.PropertyType.IsArray)
                {
                    request.Form.Any(x => x.Value.StartsWith(prefix));
                }
                else
                {
                    var parameter = request.Form.Get(prefix + property.Name);
                    if (parameter == null)
                        continue;

                    value = parameter.Value;
                    if (!property.PropertyType.IsAssignableFrom(typeof(string)))
                    {
                        value = Convert.ChangeType(value, property.PropertyType);
                    }
                }

                property.SetValue(model, value);
            }

            return model;
        }

    }

    public class SimpleArrayViewModel
    {
        public int[] Ages { get; private set; }
    }


    public class UserViewModel
    {
        public string UserName { get; private set; }
        public string FirstName { get; private set; }
    }

    public class RatingViewModel
    {
        public UserViewModel Author { get; set; }
        public int Rating { get; private set; }
    }
}
