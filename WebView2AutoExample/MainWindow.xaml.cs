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
        public Settings Settings { get; set; }
        /// <summary>
        /// The most recently captured cursor position.
        /// </summary>
        public PositionData Position { get; set; }
        public MockupData MockupData { get; set; }

        //InputSimulator input = new InputSimulator();
        private MouseSimulator underlyingMouseSimulator;
        private KeyboardSimulator underlyingKeyboardSimulator;
        public MainWindow()
        {
            InitializeComponent();
            Settings = new Settings();
            Position = new PositionData();
            MockupData = new MockupData();

            webView.NavigationCompleted += WebView_NavigationCompleted;
            webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
            webView.WebMessageReceived += WebView_WebMessageReceived;
            underlyingMouseSimulator = new MouseSimulator(new InputSimulator());
            underlyingKeyboardSimulator = new KeyboardSimulator(new InputSimulator());
            webView.ZoomFactor = 1;
  
        }

        private Point updatePosition() => updatePosition(new PositionData(new System.Drawing.Point((int)Mouse.GetPosition(this).X, (int)Mouse.GetPosition(this).Y)));

        /// <summary>
        /// The method that determines all wanted values and that is called in every tick.
        /// </summary>
        private Point updatePosition(PositionData positionData)
        {
            // Update position.
            Position = positionData;
            return new Point(Position.PhysicalPosition.X, Position.PhysicalPosition.Y);
        }

        private void WebView_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                JsonObject jsonObject = JsonConvert.DeserializeObject<JsonObject>(e.WebMessageAsJson);
                switch (jsonObject.Key)
                {
                    case "mousemove":
                        {
                            updatePosition(new PositionData(new System.Drawing.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y)));
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

                foreach (var item in MockupData.commands)
                {
                    if (item.Input == Input.OpenURL)
                    {
                        Dispatcher.Invoke(() => webView.Source = new Uri(item.Text));

                        Thread.Sleep(2000);
                    }
                    else if (item.Input == Input.LeftClick)
                    {
                        Point absolutePoint = ConvertPointToAbsolute(new Point(1385, 644));
                        underlyingMouseSimulator.MoveMouseTo(absolutePoint.X, absolutePoint.Y);
                        underlyingMouseSimulator.LeftButtonClick();
                    }
                    else if (item.Input == Input.Write)
                    {
                        Thread.Sleep(300);
                        underlyingKeyboardSimulator.TextEntry(item.Text);
                    }
                }
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
