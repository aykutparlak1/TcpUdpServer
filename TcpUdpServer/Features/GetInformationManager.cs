using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TcpUdpServer.Features
{
    public class GetInformationManager : IGetInformationService
    {
        public async Task<string> GetBaseBoardInformation()
        {
            string script = @"
            $mainboard = Get-WmiObject -Class Win32_BaseBoard | Select-Object Manufacturer, Product,Caption
            $memorySlots = (Get-WmiObject -Class Win32_PhysicalMemoryArray).MemoryDevices

            $mainboard | Add-Member -MemberType NoteProperty -Name MemorySlots -Value $memorySlots -PassThru | ConvertTo-Json
        ";
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "powershell.exe";
            psi.Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"";
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;
            string output;
            using (Process powershell = Process.Start(psi))
            {
                using (System.IO.StreamReader reader = powershell.StandardOutput)
                {
                     output = await reader.ReadToEndAsync();
                }
            }
            return output;
        }

        public async Task<string> GetNetworkInformations()
        {
            string script = @"
            $netAdapters = Get-NetAdapter | Select-Object InterfaceAlias, MacAddress
            $ipAddresses = Get-NetIPAddress -AddressFamily IPv4 | Select-Object InterfaceAlias, IPAddress

            $results = foreach ($ip in $ipAddresses) {
                $adapter = $netAdapters | Where-Object { $_.InterfaceAlias -eq $ip.InterfaceAlias }
                [PSCustomObject]@{
                    InterfaceAlias = $ip.InterfaceAlias
                    IPAddress      = $ip.IPAddress
                    MacAddress     = $adapter.MacAddress
                }
            }

            $results | Select-Object InterfaceAlias, IPAddress, MacAddress | ConvertTo-Json
             ";

            string adapterInfos;
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "powershell.exe";
            psi.Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"";
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;

            using (Process powershell = Process.Start(psi))
            {
                using (System.IO.StreamReader reader = powershell.StandardOutput)
                {
                   adapterInfos = await reader.ReadToEndAsync();
                }
            }

            return adapterInfos;
        }
        public async Task<string> GetDiskDriveInformation()
        {
            // PowerShell komutunu tanımlama ve JSON formatına dönüştürme
            string script = @"
            Get-PhysicalDisk | 
            Select-Object FriendlyName, MediaType, Size | 
            ConvertTo-Json -Compress
        ";

            string diskDriveInfos;
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process powershell = Process.Start(psi))
            {
                using (System.IO.StreamReader reader = powershell.StandardOutput)
                {
                   diskDriveInfos = await reader.ReadToEndAsync();
                }
            }

            return diskDriveInfos;
        }
        public async Task<string> GetRamInformation()
        {
            // PowerShell komutunu tanımlama ve JSON formatına dönüştürme
            string script = @"
            Get-CimInstance -ClassName Win32_PhysicalMemory | 
            Select-Object Manufacturer, Capacity, Speed | 
            ConvertTo-Json -Compress
        ";

            string ramInfos;
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process powershell = Process.Start(psi))
            {
                using (System.IO.StreamReader reader = powershell.StandardOutput)
                {
                    ramInfos = await reader.ReadToEndAsync();
                }
            }

            return ramInfos;
        }

        public async Task<string> GetProcessorInformation()
        {
            // PowerShell komutunu tanımlama ve JSON formatına dönüştürme
            string script = @"
            Get-CimInstance -ClassName Win32_Processor | 
            Select-Object Name, Manufacturer, NumberOfCores, MaxClockSpeed | 
            ConvertTo-Json -Compress
        ";

            string processorInfos;
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process powershell = Process.Start(psi))
            {
                using (System.IO.StreamReader reader = powershell.StandardOutput)
                {
                    processorInfos = await reader.ReadToEndAsync();
                }
            }

            return processorInfos;
        }

        public async Task<string> GetOperatingSystemInformation()
        {
            string script = @"
            $os = Get-CimInstance -ClassName Win32_OperatingSystem
            [PSCustomObject]@{
                OperatingSystemName = $os.Caption
                OSArchitecture = $os.OSArchitecture
            } | ConvertTo-Json -Compress
        ";

            string osInfos;
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "powershell.exe";
            psi.Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"";
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;
            using (Process powershell = Process.Start(psi))
            {
                using (System.IO.StreamReader reader = powershell.StandardOutput)
                {
                    osInfos = await reader.ReadToEndAsync();
                }
            }
            return osInfos;
        }

        public async Task<string> GetService(string serviceName)
        {
            // PowerShell komutunu tanımlama ve JSON formatına dönüştürme
            string script = $@"
            Get-Service | Where-Object {{ $_.Name -eq '{serviceName}' }} | Select-Object DisplayName,Name, Status | ConvertTo-Json -Compress
        ";

            string singleService;
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process powershell = Process.Start(psi))
            {
                using (System.IO.StreamReader reader = powershell.StandardOutput)
                {
                    singleService = await reader.ReadToEndAsync();
                }
            }
            return singleService;
        }

    }
}

