using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Griffin.WebServer.Tests
{
    public class WebExTests
    {
        [Fact]
        public void Test()
        {
            WebEx x = new WebEx();
            x.CreateAlias("unsupported", "!./");

            x.Resolve("/{url:unsupported}");

        }
    }
}
