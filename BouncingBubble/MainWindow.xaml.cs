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
        private Ellipse bubble2;
        private double xSpeed2 = -4, ySpeed2 = -4;
        private double xPos2 = 700, yPos2 = 700;
        private Ellipse bubble;
        private double xSpeed = 3.8, ySpeed = 3.8;
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
            Canvas canvas = new Canvas();

            RadialGradientBrush gradientBrush = new RadialGradientBrush();
            gradientBrush.GradientOrigin = new Point(.3, .3);
            gradientBrush.Center = new Point(.5, .5);
            gradientBrush.RadiusX = .5;
            gradientBrush.RadiusY = .5;
            gradientBrush.GradientStops.Add(new GradientStop(Colors.LightBlue, 0.0));  // Center color
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Blue, 0.8));      // Outer color
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0)); // Fading edge

            RadialGradientBrush gradientBrush2 = new RadialGradientBrush();
            gradientBrush2.GradientOrigin = new Point(.3, .3);
            gradientBrush2.Center = new Point(.5, .5);
            gradientBrush2.RadiusX = .5;
            gradientBrush2.RadiusY = .5;
            gradientBrush2.GradientStops.Add(new GradientStop(Colors.LightGreen, 0.0));  // Center color
            gradientBrush2.GradientStops.Add(new GradientStop(Colors.Green, 0.8));      // Outer color
            gradientBrush2.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0)); // Fading edge

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
            bubble2 = new Ellipse
            {
                Width = 50,
                Height = 50,
                Fill = gradientBrush2,
                Opacity = 0.75,
            };
            canvas.Children.Add(bubble);
            canvas.Children.Add(bubble2);
            this.Content = canvas;
        }

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

            MoveBubble(ref xPos, ref yPos, ref xSpeed, ref ySpeed, bubble);
            MoveBubble(ref xPos2, ref yPos2, ref xSpeed2, ref ySpeed2, bubble2);

            CheckBubbleCollision();
        }

        // Moves a single bubble and checks for screen boundaries
        private void MoveBubble(ref double x, ref double y, ref double xSpeed, ref double ySpeed, Ellipse bubble)
        {
            double nextX = x + xSpeed;
            double nextY = y + ySpeed;

            bool bounced = false;

            // Screen collision detection
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
                    if (x + bubble.Width <= window.Left || x >= window.Right)
                        xSpeed *= -1; // Reverse X direction

                    if (y + bubble.Height <= window.Top || y >= window.Bottom)
                        ySpeed *= -1; // Reverse Y direction

                    break; // Exit loop to avoid double bouncing
                }
            }

            Rect taskbarBounds = GetTaskbarBounds();
            if (taskbarBounds != Rect.Empty && taskbarBounds.IntersectsWith(new Rect(nextX, nextY, bubble.Width, bubble.Height)))
            {
                if (taskbarBounds.Height < taskbarBounds.Width) // Horizontal taskbar (top or bottom)
                    ySpeed *= -1;
                else // Vertical taskbar (left or right)
                    xSpeed *= -1;

                bounced = true;
            }

            if (bounced)
            {
                bubble.Fill = ChangeBubbleColor();
            }

            // Apply new position
            x += xSpeed;
            y += ySpeed;

            Canvas.SetLeft(bubble, x);
            Canvas.SetTop(bubble, y);
        }

        private void CheckBubbleCollision()
        {
            double dx = xPos2 - xPos;
            double dy = yPos2 - yPos;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance < bubble.Width) // Collision detected
            {
                // Swap speeds for a simple elastic collision effect
                double tempXSpeed = xSpeed;
                double tempYSpeed = ySpeed;
                xSpeed = xSpeed2;
                ySpeed = ySpeed2;
                xSpeed2 = tempXSpeed;
                ySpeed2 = tempYSpeed;

                // Change colors on collision
                bubble.Fill = ChangeBubbleColor();
                bubble2.Fill = ChangeBubbleColor();
            }
        }


        // Random color function
        private RadialGradientBrush ChangeBubbleColor()
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

            return gradientBrush;
        }

        private Rect GetTaskbarBounds()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double workWidth = SystemParameters.WorkArea.Width;
            double workHeight = SystemParameters.WorkArea.Height;
            double workLeft = SystemParameters.WorkArea.Left;
            double workTop = SystemParameters.WorkArea.Top;

            // Calculate taskbar position based on available work area
            if (workWidth < screenWidth) // Taskbar on left or right
            {
                if (workLeft > 0)
                    return new Rect(0, 0, workLeft, screenHeight);  // Taskbar on the left
                else
                    return new Rect(workWidth, 0, screenWidth - workWidth, screenHeight); // Taskbar on the right
            }
            else if (workHeight < screenHeight) // Taskbar on top or bottom
            {
                if (workTop > 0)
                    return new Rect(0, 0, screenWidth, workTop);  // Taskbar on the top
                else
                    return new Rect(0, workHeight, screenWidth, screenHeight - workHeight); // Taskbar on the bottom
            }

            return Rect.Empty; // No taskbar found (rare case)
        }
    }
}