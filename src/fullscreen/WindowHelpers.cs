using System;
using System.Runtime.InteropServices;

namespace Vaettir.Personal.Utility.Launcher
{
    public class WindowHelpers
    {
        private const long WS_BORDER = 8388608;
        private const long WS_DLGFRAME = 4194304;
        private const long WS_CAPTION = WS_BORDER | WS_DLGFRAME;
        private const long WS_SYSMENU = 524288;
        private const long WS_THICKFRAME = 262144;
        private const long WS_MINIMIZE = 536870912;
        private const long WS_MAXIMIZEBOX = 65536;
        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;
        private const long WS_EX_DLGMODALFRAME = 0x1;
        private const long SWP_NOMOVE = 0x2;
        private const long SWP_NOSIZE = 0x1;
        private const long SWP_FRAMECHANGED = 0x20;
        private const uint MF_BYPOSITION = 0x400;
        private const uint MF_REMOVE = 0x1000;

        [DllImport("user32.dll", EntryPoint="GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint="GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        // This static method is required because Win32 does not support
        // GetWindowLongPtr directly
        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            else
                return GetWindowLongPtr32(hWnd, nIndex);
        }

        // This helper static method is required because the 32-bit version of user32.dll does not contain this API
        // (on any versions of Windows), so linking the method will fail at run-time. The bridge dispatches the request
        // to the correct function (GetWindowLong in 32-bit mode and GetWindowLongPtr in 64-bit mode)
        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [DllImport("user32.dll", EntryPoint="SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint="SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        public static void MakeExternalWindowBorderless(IntPtr hwnd)
        {
            long style = (long) GetWindowLongPtr(hwnd, GWL_STYLE);
            SetWindowLongPtr(hwnd, GWL_STYLE, new IntPtr(style & ~WS_CAPTION));
        }

        private const uint SW_RESTORE = 9;
        [DllImport("user32.dll", EntryPoint = "ShowWindow", PreserveSig = false)]
        private static extern void ShowWindow(IntPtr hWnd, uint nCmdShow);

        public static void RestoreWindow(IntPtr hWnd)
        {
            ShowWindow(hWnd, SW_RESTORE);
        }
    }
}