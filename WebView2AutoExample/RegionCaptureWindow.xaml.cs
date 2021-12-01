using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace WebView2AutoExample
{
    /// <summary>
    /// Interaction logic for RegionCaptureWindow.xaml
    /// </summary>
    public partial class RegionCaptureWindow : Window
    {
        /// <summary>
        /// The start position of the Region Rectangle
        /// </summary>
        private Point Start;

        /// <summary>
        /// The end position of the Region Rectangle
        /// </summary>
        private Point Current;

        /// <summary>
        /// Determines weather the user is drawing region or not
        /// </summary>
        private bool isDrawing = false;

        /// <summary>
        /// Rectangle
        /// </summary>
        private double X, Y, W, H;

        public RegionCaptureWindow()
        {
            InitializeComponent();
        }

        #region Functions
        private void Grid1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;

            Start = Mouse.GetPosition(Canvas1);

            Canvas.SetLeft(Rect, Start.X);
            Canvas.SetTop(Rect, Start.Y);
        }

        private void Grid1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;

            // Calculate rectangle cords/size
            BitmapSource bSource = ScreenCapturer.CaptureRegion((int)X, (int)Y, (int)W, (int)H);

            ScreenCapturer.Save(bSource);

            this.Close();
        }

        private void Grid1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                // Get new position
                Current = Mouse.GetPosition(Canvas1);

                // Calculate rectangle cords/size
                X = Math.Min(Current.X, Start.X);
                Y = Math.Min(Current.Y, Start.Y);
                W = Math.Max(Current.X, Start.X) - X;
                H = Math.Max(Current.Y, Start.Y) - Y;

                Canvas.SetLeft(Rect, X);
                Canvas.SetTop(Rect, Y);

                // Update rectangle
                Rect.Width = W;
                Rect.Height = H;
                Rect.SetValue(Canvas.LeftProperty, X);
                Rect.SetValue(Canvas.TopProperty, Y);

                // Toogle visibility
                if (Rect.Visibility != Visibility.Visible)
                    Rect.Visibility = Visibility.Visible;
            }
        }
        #endregion
    }
}
