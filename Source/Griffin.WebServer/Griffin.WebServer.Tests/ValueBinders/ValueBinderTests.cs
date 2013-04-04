using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Griffin.Networking.Protocol.Http.Implementation;
using Griffin.WebServer.ModelBinders;
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
            var actual = modelMapper.Bind<UserViewModel>(request, "");

            Assert.Equal("Jonas", actual.UserName);
            Assert.Equal("Arne", actual.FirstName);
        }

        [Fact]
        public void Nested()
        {
            var request = new HttpRequest();
            request.Form.Add("Author.UserName", "Jonas");
            request.Form.Add("Author.FirstName", "Arne");
            request.Form.Add("Rating", "22");

            var modelMapper = new ModelMapper();
            var actual = modelMapper.Bind<RatingViewModel>(request, "");

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
            var actual = modelMapper.Bind<SimpleArrayViewModel>(request, "");

            Assert.Equal(8, actual.Ages[0]);
            Assert.Equal(32, actual.Ages[1]);
        }

        [Fact]
        public void ViewModelWithArray_Indexed()
        {
            var request = new HttpRequest();
            request.Form.Add("Users[0].FirstName", "Hobbe");
            request.Form.Add("Users[0].Age", "32");
            request.Form.Add("Users[1].FirstName", "Kalle");
            request.Form.Add("Users[2].Age", "10");

            var modelMapper = new ModelMapper();
            var actual = modelMapper.Bind<UsersViewModel>(request, "");

            Assert.Equal("Hobbe", actual.Users[0].FirstName);
            Assert.Equal("Kalle", actual.Users[1].FirstName);
        }

        [Fact]
        public void Array_Indexed()
        {
            var request = new HttpRequest();
            request.Form.Add("users[0].FirstName", "Hobbe");
            request.Form.Add("users[0].Age", "32");
            request.Form.Add("users[1].FirstName", "Kalle");
            request.Form.Add("users[2].Age", "10");

            var modelMapper = new ModelMapper();
            var actual = modelMapper.Bind<UserViewModel[]>(request, "users");

            Assert.Equal("Hobbe", actual[0].FirstName);
            Assert.Equal("Kalle", actual[1].FirstName);
        }



        [Fact]
        public void Array_Associative()
        {
            var request = new HttpRequest();
            request.Form.Add("Users[Jonas].FirstName", "Hobbe");
            request.Form.Add("Users[Jonas].Age", "32");
            request.Form.Add("Users[Arne].FirstName", "Kalle");
            request.Form.Add("Users[Arne].Age", "10");

            var modelMapper = new ModelMapper();
            var actual = modelMapper.Bind<Dictionary<string, UserViewModel>>(request, "Users");

            Assert.Equal("Hobbe", actual["Jonas"].FirstName);
            Assert.Equal("Kalle", actual["Arne"].FirstName);
        }

        [Fact]
        public void Array_AssociativeNumeric()
        {
            var request = new HttpRequest();
            request.Form.Add("Users['0'].FirstName", "Hobbe");
            request.Form.Add("Users['0'].Age", "32");
            request.Form.Add("Users['1'].FirstName", "Kalle");
            request.Form.Add("Users['1'].Age", "10");

            var modelMapper = new ModelMapper();
            var actual = modelMapper.Bind<SimpleAssocArrayViewModel>(request, "");

            Assert.Equal("Hobbe", actual.Users["0"].FirstName);
            Assert.Equal("Kalle", actual.Users["1"].FirstName);
        }
    }

    public class SimpleArrayViewModel
    {
        public int[] Ages { get; private set; }
    }

    public class SimpleAssocArrayViewModel
    {
        public IDictionary<string, UserViewModel> Users { get; private set; }
    }


    public class UsersViewModel
    {
        public UserViewModel[] Users { get; set; }
    }
    public class UserViewModel
    {
        public string UserName { get; private set; }
        public string FirstName { get; private set; }
        public int Age { get; set; }
    }

    public class RatingViewModel
    {
        public UserViewModel Author { get; set; }
        public UsersViewModel Readers { get; set; }
        public int Rating { get; private set; }
    }
}
