using System;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;

namespace BouncingBubble
{
    public partial class MainWindow : Window
    {
        private BubbleWindow bubbleWindow;

        public MainWindow()
        {
            InitializeComponent();
            bubbleWindow = new BubbleWindow();
            bubbleWindow.Show();
        }
    }

    public class BubbleWindow : Window
    {
        private Ellipse bubble;
        private double xSpeed = 1.8, ySpeed = 1.8;
        private double xPos = 100, yPos = 100;
        private Random random = new Random();
        private double screenWidth, screenHeight;
        private bool isMoving = true;
        DispatcherTimer timer;
        private MouseHook mouseHook;
        private OpenedWindows openedWindows;

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int LWA_ALPHA = 0x2;

        public BubbleWindow()
        {
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;
            Topmost = true;
            Left = 0;
            Top = 0;

            Loaded += OnLoaded;

            screenWidth = SystemParameters.PrimaryScreenWidth;
            screenHeight = SystemParameters.PrimaryScreenHeight;

            CreateBubble();
            //InitializeSpeed();

            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            timer.Tick += UpdateBubble;
            timer.Start();

            // Add key listener
            //this.KeyDown += MainWindow_KeyDown;
            mouseHook = new MouseHook();
            mouseHook.OnMouseClick += MouseClicked;

            openedWindows = new OpenedWindows();
        }

        private void MouseClicked(int mouseX, int mouseY)
        {
            Dispatcher.Invoke(() =>
            {
                if (IsMouseOverBubble(mouseX, mouseY))
                {
                    ToggleMovement(); // Toggle movement if the bubble is clicked
                }
            });
        }

        private bool IsMouseOverBubble(int mouseX, int mouseY)
        {
            // Get the bubble's position relative to the screen
            double bubbleX = this.Left + xPos;
            double bubbleY = this.Top + yPos;
            double bubbleSize = bubble.Width; // Bubble is a circle, so Width = Height

            // Check if the mouse is inside the bubble area
            double centerX = bubbleX + (bubbleSize / 2);
            double centerY = bubbleY + (bubbleSize / 2);
            double radius = bubbleSize / 2;

            double distance = Math.Sqrt(Math.Pow(mouseX - centerX, 2) + Math.Pow(mouseY - centerY, 2));

            return distance <= radius; // If within radius, it's a click on the bubble
        }

        protected override void OnClosed(EventArgs e)
        {
            mouseHook.Unhook();
            base.OnClosed(e);
        }

        //protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    base.OnMouseLeftButtonDown(e);
        //    ToggleMovement();
        //}

        //private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        //{
        //    if (e.Key == System.Windows.Input.Key.Space) // Press Spacebar to toggle
        //    {
        //        ToggleMovement();
        //    }
        //}

        private void ToggleMovement()
        {
            isMoving = !isMoving; // Flip the movement state

            if (isMoving)
            {
                timer.Tick += UpdateBubble;
            }
            else
            {
                timer.Tick -= UpdateBubble; // Pause movement
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT);
        }

        private void CreateBubble()
        {
            RadialGradientBrush gradientBrush = new RadialGradientBrush();
            gradientBrush.GradientOrigin = new Point(.3, .3);
            gradientBrush.Center = new Point(.5, .5);
            gradientBrush.RadiusX = .5;
            gradientBrush.RadiusY = .5;
            gradientBrush.GradientStops.Add(new GradientStop(Colors.LightBlue, 0.0));  // Center color
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Blue, 0.8));      // Outer color
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0)); // Fading edge

            Canvas canvas = new Canvas();
            bubble = new Ellipse
            {
                Width = 80,
                Height = 80,
                Fill = gradientBrush,
                Opacity = 0.75,
                //Effect = new DropShadowEffect
                //{
                //    Color = Colors.Cyan,
                //    BlurRadius = 20,
                //    Opacity = 0.8
                //}
            };
            canvas.Children.Add(bubble);
            this.Content = canvas;
            //AnimateBubbleColor();
        }

        //private void AnimateBubbleColor()
        //{
        //    ColorAnimation colorAnimation = new ColorAnimation
        //    {
        //        From = Colors.LightBlue,
        //        To = Colors.Pink,
        //        Duration = TimeSpan.FromSeconds(2),
        //        AutoReverse = true,
        //        RepeatBehavior = RepeatBehavior.Forever
        //    };

        //    bubble.Fill.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        //}

        private void InitializeSpeed()
        {
            xSpeed = random.NextDouble() * 4 + 2;
            ySpeed = random.NextDouble() * 4 + 2;
            if (random.Next(2) == 0) xSpeed *= -1;
            if (random.Next(2) == 0) ySpeed *= -1;
        }

        private void UpdateBubble(object sender, EventArgs e)
        {
            if (!isMoving) return;
            screenWidth = SystemParameters.PrimaryScreenWidth;
            screenHeight = SystemParameters.PrimaryScreenHeight;

            // Predict next position before moving
            double nextX = xPos + xSpeed;
            double nextY = yPos + ySpeed;

            bool bounced = false; // Track if bounce happens

            // Screen boundary collision
            if (nextX <= 0 || nextX + bubble.Width >= screenWidth)
            {
                xSpeed *= -1;
                nextX = Math.Max(0, Math.Min(nextX, screenWidth - bubble.Width));
                bounced = true;
            }
            if (nextY <= 0 || nextY + bubble.Height >= screenHeight)
            {
                ySpeed *= -1;
                nextY = Math.Max(0, Math.Min(nextY, screenHeight - bubble.Height));
                bounced = true;
            }

            List<OpenedWindows.RECT> openWindows = openedWindows.GetOpenWindows();

            foreach (var window in openWindows)
            {
                if (nextX + bubble.Width > window.Left && nextX < window.Right &&
            nextY + bubble.Height > window.Top && nextY < window.Bottom)
                {
                    if (xPos + bubble.Width <= window.Left || xPos >= window.Right)
                        xSpeed *= -1; // Reverse X direction

                    if (yPos + bubble.Height <= window.Top || yPos >= window.Bottom)
                        ySpeed *= -1; // Reverse Y direction

                    break; // Exit loop to avoid double bouncing
                }
            }

            if (bounced)
                ChangeBubbleColor();

            xPos += xSpeed;
            yPos += ySpeed;

            Canvas.SetLeft(bubble, xPos);
            Canvas.SetTop(bubble, yPos);
        }

        // Random color function
        private void ChangeBubbleColor()
        {
            Color randomColor = Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
            Color randomColor2 = Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));

            RadialGradientBrush gradientBrush = new RadialGradientBrush();
            gradientBrush.GradientOrigin = new Point(.3, .3);
            gradientBrush.Center = new Point(.5, .5);
            gradientBrush.RadiusX = .5;
            gradientBrush.RadiusY = .5;
            gradientBrush.GradientStops.Add(new GradientStop(randomColor, 0.0));  // Center color
            gradientBrush.GradientStops.Add(new GradientStop(randomColor2, 0.8));      // Outer color
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0)); // Fading edge

            bubble.Fill = gradientBrush;
        }
    }
}