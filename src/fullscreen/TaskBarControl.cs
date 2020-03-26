using System;
using System.Runtime.InteropServices;

namespace Vaettir.Personal.Utility.Launcher
{
    internal class TaskBarControl
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("shell32.dll")]
        private static extern UInt32 SHAppBarMessage(UInt32 dwMessage, ref Appbardata pData);

        public enum AppBarMessages
        {
            New = 0x00,
            Remove = 0x01,
            QueryPos = 0x02,
            SetPos = 0x03,
            GetState = 0x04,
            GetTaskBarPos = 0x05,
            Activate = 0x06,
            GetAutoHideBar = 0x07,
            SetAutoHideBar = 0x08,
            WindowPosChanged = 0x09,
            SetState = 0x0a
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Appbardata
        {
            public int cbSize; // initialize this field using: Marshal.SizeOf(typeof(APPBARDATA));
            public IntPtr hWnd;
            public uint uCallbackMessage;
            public uint uEdge;
            public Rect rc;
            public int lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            private int Left;
            private int Top;
            private int Right;
            private int Bottom;

            private Rect(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            private Rect(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom)
            {
            }

            private int X
            {
                get { return Left; }
                set
                {
                    Right -= (Left - value);
                    Left = value;
                }
            }

            public int Y
            {
                get { return Top; }
                set
                {
                    Bottom -= (Top - value);
                    Top = value;
                }
            }

            public int Height
            {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }

            public int Width
            {
                get { return Right - Left; }
                set { Right = value + Left; }
            }

            public System.Drawing.Point Location
            {
                get { return new System.Drawing.Point(Left, Top); }
                set
                {
                    X = value.X;
                    Y = value.Y;
                }
            }

            public System.Drawing.Size Size
            {
                get { return new System.Drawing.Size(Width, Height); }
                set
                {
                    Width = value.Width;
                    Height = value.Height;
                }
            }

            public static implicit operator System.Drawing.Rectangle(Rect r)
            {
                return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
            }

            public static implicit operator Rect(System.Drawing.Rectangle r)
            {
                return new Rect(r);
            }

            public static bool operator ==(Rect r1, Rect r2)
            {
                return r1.Equals(r2);
            }

            public static bool operator !=(Rect r1, Rect r2)
            {
                return !r1.Equals(r2);
            }

            public bool Equals(Rect r)
            {
                return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
            }

            public override bool Equals(object obj)
            {
                if (obj is Rect)
                    return Equals((Rect) obj);
                else if (obj is System.Drawing.Rectangle)
                    return Equals(new Rect((System.Drawing.Rectangle) obj));
                return false;
            }

            public override int GetHashCode()
            {
                return ((System.Drawing.Rectangle) this).GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(
                    System.Globalization.CultureInfo.CurrentCulture,
                    "{{Left={0},Top={1},Right={2},Bottom={3}}}",
                    Left,
                    Top,
                    Right,
                    Bottom);
            }
        }


        public enum AppBarStates
        {
            AlwaysOnTop = 0x00,
            AutoHide = 0x01
        }

        /// <summary>
        /// Set the Taskbar State option
        /// </summary>
        /// <param name="option">AppBarState to activate</param>
        public static void SetTaskbarState(AppBarStates option)
        {
            Appbardata msgData = new Appbardata();
            msgData.cbSize = Marshal.SizeOf(msgData);
            msgData.hWnd = FindWindow("System_TrayWnd", null);
            msgData.lParam = (int) option;
            SHAppBarMessage((UInt32) AppBarMessages.SetState, ref msgData);
        }

        /// <summary>
        /// Gets the current Taskbar state
        /// </summary>
        /// <returns>current Taskbar state</returns>
        public static AppBarStates GetTaskbarState()
        {
            Appbardata msgData = new Appbardata();
            msgData.cbSize = Marshal.SizeOf(msgData);
            msgData.hWnd = FindWindow("System_TrayWnd", null);
            return (AppBarStates) SHAppBarMessage((UInt32) AppBarMessages.GetState, ref msgData);
        }

        private class Resetter : IDisposable
        {
            private readonly AppBarStates _oldState;

            public Resetter(AppBarStates oldState)
            {
                _oldState = oldState;
            }

            private void ReleaseUnmanagedResources()
            {
                SetTaskbarState(_oldState);
            }

            public void Dispose()
            {
                ReleaseUnmanagedResources();
                GC.SuppressFinalize(this);
            }

            ~Resetter()
            {
                ReleaseUnmanagedResources();
            }
        }

        public static IDisposable SetTemporaryState(AppBarStates state)
        {
            Resetter resetter = new Resetter(GetTaskbarState());
            SetTaskbarState(state);
            return resetter;
        }
    }
}
