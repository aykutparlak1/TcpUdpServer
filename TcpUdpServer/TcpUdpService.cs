using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TcpUdpServer.Features;

namespace TcpUdpServer
{
    public class TcpUdpService : ServiceBase
    {
        private TcpListener tcpListener;
        private CancellationTokenSource cancellationTokenSource;
        private Task tcpTask;
        private readonly IGetInformationService _getInformationService;

        public TcpUdpService()
        {
            this.ServiceName = "TcpUdpServerService";
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();
            _getInformationService = serviceProvider.GetRequiredService<IGetInformationService>();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IGetInformationService, GetInformationManager>();
        }

        protected override void OnStart(string[] args)
        {
            cancellationTokenSource = new CancellationTokenSource();

            tcpTask = Task.Run(() => StartTcpServer(cancellationTokenSource.Token));
            
        }

        protected override void OnStop()
        {
            cancellationTokenSource.Cancel();
            tcpListener?.Stop();
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
                        Task.Run(() => HandleTcpClientAsync(client, token));
                    }

                    Thread.Sleep(100);
                }
            }
            catch (Exception e)
            {
                // Hata ayıklama ve logging için burayı kullanın
            }
        }

        private async Task HandleTcpClientAsync(TcpClient client, CancellationToken token)
        {
            NetworkStream stream = client.GetStream();

            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string[] parts = data.Split(':');
                if (parts.Length == 4)
                {
                    string username = parts[0];
                    string password = parts[1];
                    string req = parts[2];
                    string parametre = parts[3];
                    if (IsUserAuthenticated(username, password))
                    {
                        string response;
                        switch (req)
                        {
                            case "BaseBoard":
                                response= await _getInformationService.GetBaseBoardInformation();
                                break;
                            case "NetworkInfo":
                                response = await _getInformationService.GetNetworkInformations();
                                break;
                            case "DiskDrive":
                                response = await _getInformationService.GetDiskDriveInformation();
                                break;
                            case "OperatingSystemInfo":
                                response = await _getInformationService.GetOperatingSystemInformation();
                                break;
                            case "ProcessorInfo":
                                response = await _getInformationService.GetProcessorInformation();
                                break;
                            case "RamInfo":
                                response = await _getInformationService.GetRamInformation();
                                break;
                            case "ServicesInfo":
                                response = await _getInformationService.GetService(parametre);
                                break;
                            default:
                                response = "InformationNotFound";
                                break;
                        }
                        
                        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                        stream.Write(responseBytes, 0, responseBytes.Length);
                    }
                    else
                    {
                        string responseMessage = "Authentication failed";
                        byte[] responseBytes = Encoding.UTF8.GetBytes(responseMessage);
                        stream.Write(responseBytes, 0, responseBytes.Length);
                    }
                }
                else
                {
                    string responseMessage = "Invalid data format";
                    byte[] responseBytes = Encoding.UTF8.GetBytes(responseMessage);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                }
            }
            catch (Exception ex)
            {
                // Hata ayıklama ve logging için burayı kullanın
            }
            finally
            {
                stream.Close();
                client.Close();
            }
        }

        private bool IsUserAuthenticated(string username, string password)
        {
            // Örnek olarak hardcoded bir kullanıcı adı ve şifre ile doğrulama yapalım
            // Gerçek uygulamada bu verileri veritabanından veya güvenli bir yerden almalısınız
            string expectedUsername = "user";
            string expectedPassword = "password";

            return username == expectedUsername && password == expectedPassword;
        }
        public void onDebug(){
            OnStart(null);
        }
    }
}
