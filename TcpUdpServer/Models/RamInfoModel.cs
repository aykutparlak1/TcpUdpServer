using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpUdpServer.Models
{
    public class RamInfoModel
    {
        public string ItemType { get; set; }
        public string Manufacturer { get; set; }
        public double Capacity { get; set; }
        public int ClockSpeed { get; set; }    }
}
