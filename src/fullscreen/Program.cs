using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;

namespace Vaettir.Personal.Utility.Launcher
{
    internal class SafeNativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);
    }


    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {

            string path;
            if (args.Length > 1)
            {
                path = args[0];

                if (!File.Exists(path))
                {
                    MessageBox.Show(
                        $"Could not find program '{path}', check command line arguments",
                        "Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return 1;
                }
            }
            else
            {
                var b = new ConfigurationBuilder();
                b.AddJsonFile("config.json");
                var config = b.Build();
                path = config["Path"];
                if (!File.Exists(path))
                {
                    MessageBox.Show(
                        $"Could not find program '{path}', check config.json",
                        "Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return 1;
                }
            }

            using (TaskBarControl.SetTemporaryState(TaskBarControl.AppBarStates.AutoHide))
            {
                TaskCompletionSource<int> exiting = new TaskCompletionSource<int>();

                var parentProcess = new Process
                {
                    EnableRaisingEvents = true,
                    StartInfo =
                    {
                        FileName = path,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Minimized,
                        UseShellExecute = false,
                    },
                };
                parentProcess.Exited += (p, __) => exiting.SetResult(((Process) p).ExitCode);
                parentProcess.Start();

                bool anyProcessRunning;
                do
                {
                    await Task.Delay(500);
                    List<Process> allProcesses = new List<Process>{parentProcess};
                    List<Process> newChildren = allProcesses;
                    do
                    {
                        newChildren = newChildren.SelectMany(p => p.GetChildProcesses()).ToList();
                        allProcesses.AddRange(newChildren);
                    } while (newChildren.Count != 0);
                    Rectangle rect = GetAllScreenSize();
                    anyProcessRunning = false;
                    var borderSize = new Size(7,7);
                    rect.X -= borderSize.Width;
                    rect.Width += borderSize.Width * 2;
                    rect.Y -= borderSize.Height;
                    rect.Height += borderSize.Height * 2;
                    foreach (var proc in allProcesses)
                    {
                        anyProcessRunning |= !proc.HasExited;
                        try
                        {
                            IntPtr ptr = proc.MainWindowHandle;

                            if (ptr != IntPtr.Zero)
                            {
                                WindowHelpers.MakeExternalWindowBorderless(ptr);
                                SafeNativeMethods.MoveWindow(ptr, rect.X, rect.Y, rect.Width, rect.Height, true);
                            }
                        }
                        catch
                        {
                        }
                    }
                }while (anyProcessRunning);

                var exitCode = await exiting.Task;
                return exitCode;
            }
        }

        private static Rectangle GetAllScreenSize()
        {
            Screen[] screens = Screen.AllScreens;
            var big = screens[0].WorkingArea;
            for (int i = 1; i < screens.Length; i++)
            {
                Rectangle s = screens[i].WorkingArea;
                if (s.Left < big.Left)
                {
                    big.Width += big.Left - s.Left;
                    big.X = s.X;
                }

                if (s.Right > big.Right)
                {
                    big.Width += s.Right - big.Right;
                }

                if (s.Top < big.Top)
                {
                    big.Height += big.Top - s.Top;
                    big.Y = s.Y;
                }

                if (s.Bottom > big.Bottom)
                {
                    big.Height += s.Bottom - big.Bottom;
                }
            }

            return big;
        }
    }
}
