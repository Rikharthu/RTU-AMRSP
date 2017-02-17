using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;
using GHIElectronics.NETMF.Net;
using GHIElectronics.NETMF.Net.NetworkInformation;
using GHIElectronics.NETMF.Net.Sockets;
using GHIElectronics.NETMF.FEZ;

namespace Pinger
{
    public class Program
    {
        Thread pingThread;

        public void Execute()
        {
            enableNetworking();
            startPing();

            // недаём сборщику мусора освободить память
            Thread.Sleep(-1);
        }

        // подготовить микроконтроллер для работы с интернетом
        private void enableNetworking()
        {
            // Настраиваем Wi-Fi модуль WIZNet W5100
            WIZnet_W5100.Enable(SPI.SPI_module.SPI1,    // спецификация производителя
                (Cpu.Pin)FEZ_Pin.Digital.Di10,          // спецификация производителя
                (Cpu.Pin)FEZ_Pin.Digital.Di7,           // ???
                false);

            // TODO пояснить что и зачем
            // ИП аддресс для нашего микроконтроллера
            byte[] ipAddress = new byte[] { 192, 168, 17, 36 };
            byte[] netmask = new byte[] { 255, 255, 255, 0 };
            byte[] gateway = new byte[] { 192, 168, 17, 1 };
            // set our own mac address for this device
            byte[] macAddress = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x36 };

            // enable microcontroller to use static IP
            NetworkInterface.EnableStaticIP(ipAddress, netmask, gateway, macAddress);
        }

        private void Ping()
        {
            // target endpoint
            IPEndPoint routerEndPoint = new IPEndPoint(
                new IPAddress(new byte[] { 192, 168, 17, 1 }), // ИП роутера
                80); // http порт

            while (true)
            {
                // открываем сокет для передачи информации
                Socket socket = new Socket(AddressFamily.InterNetwork, 
                    SocketType.Dgram, ProtocolType.Udp);    // UDP

                for (int i = 0; i < 30; i++)
                {
                    // шлём роутеру (reouterEndPoint) данные через наш сокет
                    socket.SendTo(new byte[] { 1 }, routerEndPoint);
                    socket.SendTo(new byte[] { 1,2 }, routerEndPoint);
                    socket.SendTo(new byte[] { 1,2,3 }, routerEndPoint);
                    socket.SendTo(new byte[] { 1,2,3,4 }, routerEndPoint);
                    socket.SendTo(new byte[] { 1,2,3,4,5 }, routerEndPoint);
                    Thread.Sleep(100);                    
                }

                // закрываем соединение
                socket.Close();
                Thread.Sleep(5000);
            }
        }

        private void startPing()
        {
            // работаем с сетью в отдельном потоке
            pingThread = new Thread(this.Ping);
            pingThread.Start();            
        }

        public static void Main()
        {
            new Program().Execute();
        }

    }
}
