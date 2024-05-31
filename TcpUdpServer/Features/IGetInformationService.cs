using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpUdpServer.Features
{
    public interface IGetInformationService
    {
        Task<string> GetBaseBoardInformation();
        Task<string> GetNetworkInformations();
        Task<string> GetDiskDriveInformation();
        Task<string> GetRamInformation();
        Task<string> GetProcessorInformation();
        Task<string> GetOperatingSystemInformation();
        Task<string> GetService(string serviceName);


    }
}
