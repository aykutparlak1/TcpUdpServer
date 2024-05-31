using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcpUdpServer.Models;

namespace TcpUdpServer.Features
{
    public interface IGetInformationService
    {
        Task<BaseBoardInfoModel> GetBaseBoardInformation();
        Task<List<NetworkInfoModel>> GetNetworkInformations();
       
    }
}
