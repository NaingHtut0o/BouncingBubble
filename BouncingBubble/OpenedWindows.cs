using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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

                //if (GetWindowRect(hWnd, out RECT rect) && GetClientRect(hWnd, out RECT clientRect))
                //{
                //    POINT clientTopLeft = new POINT { X = clientRect.Left, Y = clientRect.Top };
                //    ClientToScreen(hWnd, ref clientTopLeft);

                //    RECT adjustedRect = new RECT
                //    {
                //        Left = clientTopLeft.X,
                //        Top = clientTopLeft.Y,
                //        Right = clientTopLeft.X + (clientRect.Right - clientRect.Left),
                //        Bottom = clientTopLeft.Y + (clientRect.Bottom - clientRect.Top)
                //    };

                //    if (adjustedRect.Right - adjustedRect.Left > 50 && adjustedRect.Bottom - adjustedRect.Top > 50)
                //    {
                //        windows.Add(adjustedRect);
                //    }
                //}
                return true; // Continue enumeration
            }, IntPtr.Zero);

            return windows;
        }
    }
}
