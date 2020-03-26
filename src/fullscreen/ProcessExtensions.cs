using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;

namespace Vaettir.Personal.Utility.Launcher
{
    public static class ProcessExtensions
    {
        public static IEnumerable<Process> GetChildProcesses(this Process process)
        {
            List<Process> children = new List<Process>();
            ManagementObjectSearcher mos = new ManagementObjectSearcher(
                $"Select * From Win32_Process Where ParentProcessID={process.Id}");

            foreach (ManagementObject mo in mos.Get())
            {
                children.Add(Process.GetProcessById(Convert.ToInt32(mo["ProcessID"])));
            }

            return children;
        }
    }
}