using System;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpUdpServer
{
    public class TcpUdpService : ServiceBase
    {
        private TcpListener tcpListener;
        private UdpClient udpClient;
        private CancellationTokenSource cancellationTokenSource;
        private Task tcpTask;
        private Task udpTask;

        public TcpUdpService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            cancellationTokenSource = new CancellationTokenSource();

            tcpTask = Task.Run(() => StartTcpServer(cancellationTokenSource.Token));
            //udpTask = Task.Run(() => StartUdpServer(cancellationTokenSource.Token));
        }

        protected override void OnStop()
        {
            cancellationTokenSource.Cancel();
            tcpListener?.Stop();
            //udpClient?.Close();
        }

        private void StartTcpServer(CancellationToken token)
        {
            try
            {
                Int32 port = 13000;
                IPAddress localAddr = IPAddress.Any;
                tcpListener = new TcpListener(localAddr, port);
                tcpListener.Start();

                while (!token.IsCancellationRequested)
                {
                    if (tcpListener.Pending())
                    {
                        TcpClient client = tcpListener.AcceptTcpClient();
                        Task.Run(() => HandleTcpClient(client, token));
                    }

                    Thread.Sleep(100); // CPU kullanımını düşürmek için
                }
            }
            catch (Exception e)
            {
                // Hata ayıklama ve logging için burayı kullanın
            }
        }

        private void HandleTcpClient(TcpClient client, CancellationToken token)
        {
            using (NetworkStream stream = client.GetStream())
            {
                // PowerShell komutunu gönder
                string command = "Get-Process";
                byte[] commandBytes = Encoding.ASCII.GetBytes(command);
                stream.Write(commandBytes, 0, commandBytes.Length);

                // Client'tan gelen sonucu oku
                byte[] data = new byte[1024];
                int bytesRead = stream.Read(data, 0, data.Length);
                string responseData = Encoding.ASCII.GetString(data, 0, bytesRead);

                client.Close();
            }
        }

        //private void StartUdpServer(CancellationToken token)
        //{
        //    try
        //    {
        //        Int32 port = 13001;
        //        udpClient = new UdpClient(port);

        //        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);

        //        while (!token.IsCancellationRequested)
        //        {
        //            if (udpClient.Available > 0)
        //            {
        //                byte[] data = udpClient.Receive(ref remoteEndPoint);
        //                string receivedData = Encoding.ASCII.GetString(data);

        //                // Gelen komutu çalıştır ve sonucu gönder
        //                string result = ExecutePowerShellCommand(receivedData);
        //                byte[] resultBytes = Encoding.ASCII.GetBytes(result);
        //                udpClient.Send(resultBytes, resultBytes.Length, remoteEndPoint);
        //            }

        //            Thread.Sleep(100); // CPU kullanımını düşürmek için
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        // Hata ayıklama ve logging için burayı kullanın
        //    }
        //}

        private string ExecutePowerShellCommand(string command)
        {
            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
            {
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.Arguments = $"-Command \"{command}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                return string.IsNullOrWhiteSpace(error) ? output : error;
            }
        }

        private void InitializeComponent()
        {
            this.ServiceName = "TcpUdpServerService";
        }
    }
}
