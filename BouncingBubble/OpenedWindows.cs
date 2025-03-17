using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BouncingBubble
{
    public class OpenedWindows
    {
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect); 
        
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount); 
        
        //[DllImport("user32.dll")]
        //private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        //[DllImport("user32.dll")]
        //private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        //[StructLayout(LayoutKind.Sequential)]
        //public struct POINT
        //{
        //    public int X;
        //    public int Y;
        //}

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        public List<RECT> GetOpenWindows()
        {
            List<RECT> windows = new List<RECT>();

            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd)) return true;

                System.Text.StringBuilder windowText = new System.Text.StringBuilder(256);
                GetWindowText(hWnd, windowText, windowText.Capacity);

                if (string.IsNullOrWhiteSpace(windowText.ToString())) return true;

                if (GetWindowRect(hWnd, out RECT rect))
                {
                    if (rect.Right - rect.Left > 0 && rect.Bottom - rect.Top > 0)
                    {
                        windows.Add(rect);
                    }
                }
                return true; // Continue enumeration
            }, IntPtr.Zero);

            //RECT taskbarBounds = GetTaskbarBounds();
            //if (taskbarBounds.Right - taskbarBounds.Left > 0 && taskbarBounds.Bottom - taskbarBounds.Top > 0)
            //{
            //    windows.Add(taskbarBounds);
            //}

            return windows;
        }

        private RECT GetTaskbarBounds()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double workWidth = SystemParameters.WorkArea.Width;
            double workHeight = SystemParameters.WorkArea.Height;
            double workLeft = SystemParameters.WorkArea.Left;
            double workTop = SystemParameters.WorkArea.Top;

            RECT taskbarRect = new RECT();
            // Calculate taskbar position based on available work area
            if (workWidth < screenWidth) // Taskbar on left or right
            {
                if (workLeft > 0)
                    taskbarRect = new RECT { Left = 0, Top = 0, Right = (int)workLeft, Bottom = (int)screenHeight };  // Taskbar on the left
                else
                    taskbarRect = new RECT { Left = (int)workWidth, Top = 0, Right = (int)screenWidth, Bottom = (int)screenHeight }; // Taskbar on the right
            }
            else if (workHeight < screenHeight) // Taskbar on top or bottom
            {
                if (workTop > 0)
                    taskbarRect = new RECT { Left = 0, Top = 0, Right = (int)screenWidth, Bottom = (int)workTop }; // Top taskbar
                else
                    taskbarRect = new RECT { Left = 0, Top = (int)workHeight, Right = (int)screenWidth, Bottom = (int)screenHeight }; // Bottom taskbar
            }

            return taskbarRect; // Return calculated taskbar bounds
        }
    }
}
