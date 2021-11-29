using Indieteur.GlobalHooks;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
        public PositionData CurrentPoint { get; set; }
        public MockupData MockupData { get; set; }
        public bool IsRunning { get; private set; }

        public bool IsCapturing = false;
        public System.Drawing.Point StartCaptureImagePoint;
        System.Windows.Shapes.Rectangle RectCapture = new System.Windows.Shapes.Rectangle();
        private System.Windows.Point Start;
        private System.Windows.Point Current;
        private double X, Y, W, H;

        private MouseSimulator underlyingMouseSimulator;
        private KeyboardSimulator underlyingKeyboardSimulator;
        GlobalKeyHook globalKeyHook;
        GlobalMouseHook globalMouseHook;
        public MainWindow()
        {
            InitializeComponent();
            CurrentPoint = new PositionData();
            MockupData = new MockupData();

            underlyingMouseSimulator = new MouseSimulator(new InputSimulator());
            underlyingKeyboardSimulator = new KeyboardSimulator(new InputSimulator());
            this.MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;

            RectCapture.Stroke = new SolidColorBrush(Colors.Black);
            RectCapture.StrokeDashArray = new DoubleCollection { 4, 4 };
            RectCapture.SnapsToDevicePixels = true;
            RectCapture.StrokeThickness = 2;
            RectCapture.Fill = new SolidColorBrush(Colors.White);

            InitializeAsync();

            InitMacro();
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsCapturing = true;

            Start = Mouse.GetPosition(MainBrowser);

            Canvas.SetLeft(RectCapture, Start.X);
            Canvas.SetTop(RectCapture, Start.Y);
        }

        async void InitializeAsync()
        {
            await webView.EnsureCoreWebView2Async(null);

            //if (!IsRunning)
            //{
            //    string script = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\Scripts\PostMouse.js");
            //    await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script);
            //}
        }

        private async void InitMacro()
        {
            await Task.Run(() =>
            {
                foreach (var item in MockupData.commands)
                {
                    if (item.Input == Input.OpenURL)
                    {
                        Dispatcher.Invoke(() => webView.Source = new Uri(item.Text));

                        Thread.Sleep(2000);
                    }
                    else if (item.Input == Input.LeftClick)
                    {
                        System.Windows.Point absolutePoint = ConvertPointToAbsolute(new System.Windows.Point(1385, 644));
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

        private void updatePosition() => updatePosition(new PositionData(new System.Drawing.Point((int)Mouse.GetPosition(this).X, (int)Mouse.GetPosition(this).Y)));

        private void updatePosition(PositionData positionData)
        {
            CurrentPoint = positionData;
        }

        private System.Windows.Point ConvertPointToAbsolute(System.Windows.Point point)
        {
            return new System.Windows.Point((Convert.ToDouble(65535) * point.X) / Convert.ToDouble(Screen.PrimaryScreen.Bounds.Width),
                (Convert.ToDouble(65535) * point.Y) / Convert.ToDouble(Screen.PrimaryScreen.Bounds.Height));
        }

        private void Capture_Click(object sender, RoutedEventArgs e)
        {
            IsCapturing = true;

            if (globalMouseHook == null)
            {
                globalMouseHook = new GlobalMouseHook();
                globalMouseHook.OnButtonDown += GlobalMouseHook_OnButtonDown;
                globalMouseHook.OnButtonUp += GlobalMouseHook_OnButtonUp;
                globalMouseHook.OnMouseMove += GlobalMouseHook_OnMouseMove;
            }
            else
            {
                globalMouseHook.Dispose();
                globalMouseHook = null;
            }
        }

        private void GlobalMouseHook_OnButtonDown(object? sender, GlobalMouseEventArgs e)
        {
            System.Windows.Point p = new System.Windows.Point((double)System.Windows.Forms.Cursor.Position.X, (double)System.Windows.Forms.Cursor.Position.Y);

            Start = p;// ConvertPointToAbsolute(p);

            Canvas.SetLeft(RectCapture, Start.X);
            Canvas.SetTop(RectCapture, Start.Y);
        }

        private void GlobalMouseHook_OnMouseMove(object? sender, GlobalMouseEventArgs e)
        {
            if (IsCapturing)
            {
                // Get new position
                System.Windows.Point p = new System.Windows.Point((double)System.Windows.Forms.Cursor.Position.X, (double)System.Windows.Forms.Cursor.Position.Y);

                Current = p;// ConvertPointToAbsolute(p);

                // Calculate rectangle cords/size
                X = Math.Min(Current.X, Start.X);
                Y = Math.Min(Current.Y, Start.Y);
                W = Math.Max(Current.X, Start.X) - X;
                H = Math.Max(Current.Y, Start.Y) - Y;

                Canvas.SetLeft(RectCapture, X);
                Canvas.SetTop(RectCapture, Y);

                // Update rectangle
                RectCapture.Width = W;
                RectCapture.Height = H;
                RectCapture.SetValue(Canvas.LeftProperty, X);
                RectCapture.SetValue(Canvas.TopProperty, Y);

                // Toogle visibility
                if (RectCapture.Visibility != Visibility.Visible)
                    RectCapture.Visibility = Visibility.Visible;
            }
        }

        private void GlobalMouseHook_OnButtonUp(object? sender, GlobalMouseEventArgs e)
        {
            if (IsCapturing)
            {
                IsCapturing = false;

                // Calculate rectangle cords/size
                BitmapSource bSource = ScreenCapturer.CaptureRegion((int)X, (int)Y, (int)W, (int)H);

                ScreenCapturer.Save(bSource);
            }
        }
    }
}
