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
        Thread serverThread;

        public void Execute()
        {
            enableNetworking();
            serverThread = new Thread(this.StartServer);
            serverThread.Start();

            // недаём сборщику мусора освободить память
            Thread.Sleep(-1);
        }

        private void StartServer()
        {
            // Configure socket
            Socket serverSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp); // TCP protokols
            // Address on which we will listen to: Klausisiemies no jebkuras adreses (Any) uz sada porta: 12345
            IPEndPoint localEndpoint = new IPEndPoint(IPAddress.Any, 12345); // Ports
            serverSocket.Bind(localEndpoint);
            // Sakt klausities, atluat vienu klientu rindā
            serverSocket.Listen(1); // maximum number of incoming connections 

            while (true)
            {
                // Apkalpot klientu pieprasījumus (Will stop here until accepted a connection)
                Socket client = serverSocket.Accept();
                try
                {
                    ProcessRequest(client);
                }
                catch(Exception e)
                {
                    Debug.Print("Error processing client request: " + e.Message);
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
                    //byte[] response = ProcessRequestData(data);
                    byte[] response = ProcessRequestDataV3(data);
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

        private byte[] ProcessRequestDataV2(byte[] requestData)
        {
            String byteString = "";
            for(int i = 0; i < requestData.Length; i++)
            {
                byteString += requestData[i].ToString() + ",";
            }
            Debug.Print("Server received the following bytes; " + byteString);
            byte[] response = new byte[] { 1 };
            Debug.Print("Response is: " + response[0]);
            return response;
        }

        // kalkulators
        private byte[] ProcessRequestDataV3(byte[] requestData)
        {
            byte operation = requestData[0];
            byte x = requestData[1];
            byte y = requestData[2];
            Debug.Print("Received: "+operation+", "+x+", "+y);
            int result;
            if (operation == 1)
            {
                result = x + y;
            }else if(operation == 2)
            {
                result = x - y;
            }else
            {
                // TODO throw error
                result = 0;
            }
            // split result into two bytes
            byte highByte = (byte)(result >> 8);
            byte lowByte = (byte)(result & 255); // 255 = 0000000 11111111
            // to get back: low|(high<<8)
            byte[] response = new byte[] { highByte, lowByte };
            return response;
        }

        /** Configure Wi-Fi shield and setup internet settings */
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
