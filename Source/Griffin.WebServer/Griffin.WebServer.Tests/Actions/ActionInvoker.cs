using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.WebServer.Tests.Actions
{
    class ActionInvoker
    {
    }

    [GetAction("/{controller:notslash}/{action:notslash}/")]
    public class MyAction
    {
        
    }

    public class GetActionAttribute : Attribute
    {
        public GetActionAttribute(string tmp)
        {
            
        }
    }
}
