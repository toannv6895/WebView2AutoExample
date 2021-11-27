using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsInput;

namespace WebView2AutoExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out Point lpPoint);

        public Settings Settings { get; set; }
        /// <summary>
        /// The most recently captured cursor position.
        /// </summary>
        public PositionData Position { get; set; }

        //InputSimulator input = new InputSimulator();
        private MouseSimulator underlyingMouseSimulator;
        private KeyboardSimulator underlyingKeyboardSimulator;
        public MainWindow()
        {
            InitializeComponent();
            Settings = new Settings();
            Position = new PositionData();

            webView.NavigationCompleted += WebView_NavigationCompleted;
            webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
            webView.WebMessageReceived += WebView_WebMessageReceived;
            underlyingMouseSimulator = new MouseSimulator(new InputSimulator());
            underlyingKeyboardSimulator = new KeyboardSimulator(new InputSimulator());
            webView.ZoomFactor = 1;
  
        }

        private void updatePosition() => updatePosition(new PositionData(new System.Drawing.Point((int)Mouse.GetPosition(this).X, (int)Mouse.GetPosition(this).Y)));

        /// <summary>
        /// The method that determines all wanted values and that is called in every tick.
        /// </summary>
        private void updatePosition(PositionData positionData)
        {
            // Update position.
            Position = positionData;

            textBlock.Text = String.Format("X: {0} Y: {1}", Position.PhysicalPosition.X, Position.PhysicalPosition.Y);
        }

        private void WebView_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                JsonObject jsonObject = JsonConvert.DeserializeObject<JsonObject>(e.WebMessageAsJson);
                switch (jsonObject.Key)
                {
                    case "click":
                        {
                            textBlock2.Text = $"Clicked at x:{jsonObject.Value["X"].ToString()}, y:{jsonObject.Value["Y"].ToString()}";
                        }
                        break;
                    case "mousemove":
                        {
                            updatePosition(new PositionData(new System.Drawing.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y)));

                            textBlock3.Text = $"Mouse move at x:{jsonObject.Value["X"].ToString()}, y:{jsonObject.Value["Y"].ToString()}";
                        }
                        break;

                }
            }
            catch (Exception)
            {

            }
        }

        private async void WebView_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            string script = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\Scripts\Mouse.js");
            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script);
        }

        private async void WebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            await Task.Run(() =>
            {
                Thread.Sleep(2000);
 
                Point absolutePoint = ConvertPointToAbsolute(new Point(1490, 151));
                underlyingMouseSimulator.MoveMouseTo(absolutePoint.X, absolutePoint.Y);
                underlyingMouseSimulator.LeftButtonClick();

                string values = "office";
                foreach (var item in values)
                {
                    underlyingKeyboardSimulator.TextEntry(item);
                    Thread.Sleep(500);
                }
                underlyingKeyboardSimulator.KeyDown(WindowsInput.Native.VirtualKeyCode.DOWN);
                //Enter
                underlyingKeyboardSimulator.KeyDown(WindowsInput.Native.VirtualKeyCode.RETURN);
                Thread.Sleep(4000);
                underlyingMouseSimulator.HorizontalScroll(5);
            });
        }

        private Point ConvertPointToAbsolute(Point point)
        {
            return new Point((Convert.ToDouble(65535) * point.X) / Convert.ToDouble(Screen.PrimaryScreen.Bounds.Width),
                (Convert.ToDouble(65535) * point.Y) / Convert.ToDouble(Screen.PrimaryScreen.Bounds.Height));
        }
    }

    public struct JsonObject
    {
        public string Key;
        public Dictionary<string, int> Value;
    }
}
