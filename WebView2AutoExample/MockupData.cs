using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebView2AutoExample
{
    public class MockupData
    {
        public List<Command> commands;
        public MockupData()
        {
            commands = new List<Command>()
            {
                new Command()
                {Id =1,Input = Input.OpenURL, Text = "https://twitter.com/",
                 Position = new System.Drawing.Point(0,0), Sleep = 0 },
                new Command()
                {Id =2,Input = Input.LeftClick, Text = String.Empty,
                 Position = new System.Drawing.Point(1267,965), Sleep = 0 }
            };
        }
    }
}
