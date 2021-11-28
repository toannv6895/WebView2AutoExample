using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebView2AutoExample
{
    public class Command
    {
        public int Id { get; set; }
        public Input Input { get; set; }
        public Point Position { get; set; }
        public string Text { get; set; }
        public int Sleep { get; set; }
    }
}
