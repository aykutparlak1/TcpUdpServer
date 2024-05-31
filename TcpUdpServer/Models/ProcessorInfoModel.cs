using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpUdpServer.Models
{
    public class ProcessorInfoModel
    {
        public string ItemType { get; set; }
        public string Manufacturer { get; set; }
        public string Product { get; set; }
        public int Core { get; set; }
        public int MaxClockSpeed { get; set; }
    }
}
