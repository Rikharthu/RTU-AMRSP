using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;
using GHIElectronics.NETMF.Net;
using GHIElectronics.NETMF.Net.NetworkInformation;
using GHIElectronics.NETMF.Net.Sockets;
using GHIElectronics.NETMF.FEZ;
using System.Text;

namespace Server
{
    public class Program
    {
        Thread pingThread;

        public void Execute()
        {
            enableNetworking();

            // недаём сборщику мусора освободить память
            Thread.Sleep(-1);
        }

        private void startServer()
        {
            Socket serverSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp); // TCP protokols
            IPEndPoint localEndpoint = new IPEndPoint(IPAddress.Any, 12345); // Ports

            serverSocket.Bind(localEndpoint);
            // Sakt klausities, atluat vienu klientu rindā
            serverSocket.Listen(1); // maximum number of incoming connections 

            while (true)
            {
                // Apkalpot klientu pieprasījumus
                Socket client = serverSocket.Accept();
                try
                {
                    ProcessRequest(client);
                }
                catch(Exception e)
                {
                    Debug.Print("Erorr processing client request: " + e.Message);
                }
                finally
                {
                    client.Close();
                }
            }
        }

        private void ProcessRequest(Socket client)
        {
            // soketu kontekstā laiku mēra mikrosekundēs
            const int microSecondsPerSecond = 1000000;

            // gaidit dauts no klienta 5 sekundes
            if (client.Poll(5 * microSecondsPerSecond, SelectMode.SelectRead))
            {
                // check if we have received any data
                if(client.Available > 0)
                {
                    // iedalīt datiem buferu
                    byte[] data = new byte[client.Available];
                    // ielasīt datus buferī
                    client.Receive(data, data.Length, SocketFlags.None);

                    // apstradat klienta datus
                    byte[] response = ProcessRequestData(data);
                    // sutit atbildi klientam
                    client.Send(response);
                }
            }
        }

        private byte[] ProcessRequestData(byte[] requestData)
        {
            String requestDataString = new string(Encoding.UTF8.GetChars(requestData));
            Debug.Print("Server received: '" + requestDataString + "'");

            byte[] response = Encoding.UTF8.GetBytes("Hello client!");
            return response;
        }

        private void enableNetworking()
        {
            WIZnet_W5100.Enable(SPI.SPI_module.SPI1,    // спецификация производителя
                (Cpu.Pin)FEZ_Pin.Digital.Di10,          // спецификация производителя
                (Cpu.Pin)FEZ_Pin.Digital.Di7,           // ???
                false);

            // TODO пояснить что и зачем
            byte[] ipAddress = new byte[] { 192, 168, 17, 36 };
            byte[] netmask = new byte[] { 255, 255, 255, 0 };
            byte[] gateway = new byte[] { 192, 168, 17, 1 };
            // set our own mac address for this device
            byte[] macAddress = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x36 };

            NetworkInterface.EnableStaticIP(ipAddress, netmask, gateway, macAddress);
        }

        public static void Main()
        {
            new Program().Execute();
        }

    }
}
