using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using TcpUdpServer.Models;

namespace TcpUdpServer.Features
{
    public class GetInformationManager : IGetInformationService
    {
        public async Task<BaseBoardInfoModel> GetBaseBoardInformation()
        {
            string script = @"
            $mainboard = Get-WmiObject -Class Win32_BaseBoard | Select-Object Manufacturer, Product,Caption
            $memorySlots = (Get-WmiObject -Class Win32_PhysicalMemoryArray).MemoryDevices

            $mainboard | Add-Member -MemberType NoteProperty -Name MemorySlots -Value $memorySlots -PassThru | ConvertTo-Json
        ";

            BaseBoardInfoModel mainboardInfo = new BaseBoardInfoModel();
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
                    string output = await reader.ReadToEndAsync();
                    mainboardInfo = JsonConvert.DeserializeObject<BaseBoardInfoModel>(output);
                }
            }
            return mainboardInfo;
        }

        public async Task<List<NetworkInfoModel>> GetNetworkInformations()
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

            List<NetworkInfoModel> adapterInfos = new List<NetworkInfoModel>();
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
                    string output = await reader.ReadToEndAsync();
                    adapterInfos = JsonConvert.DeserializeObject<List<NetworkInfoModel>>(output);
                }
            }

            return adapterInfos;
        }
    }
}

