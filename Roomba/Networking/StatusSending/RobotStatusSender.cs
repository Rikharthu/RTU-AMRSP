using System;
using Microsoft.SPOT;
using Roomba.Roomba.Sensors;
using System.Threading;
using GHIElectronics.NETMF.Net.Sockets;
using GHIElectronics.NETMF.Net;
using Roomba.Utils;

namespace Roomba.Networking.StatusSending
{
    public class RobotStatusSender
    {
        private SensorController sensors;
        private Thread workerThread;

        public RobotStatusSender(SensorController sensors)
        {
            this.sensors = sensors;
        }

        public void Start()
        {
            this.workerThread = new Thread(this.DoWork);
            this.workerThread.Start();
        }

        private void DoWork()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // TODO set to PC's IP Address
            IPEndPoint endpoint = new IPEndPoint(new IPAddress(new byte[] { 192, 168, 17, 136 }), 666);
            // TODO set to router IP Address
            IPEndPoint routerEndpoint = new IPEndPoint(new IPAddress(new byte[] { 192, 168, 17, 1 }), 80);

            /* Izmantosim baitu masivu no 21 baita:
                    *  0..3 -> robota X koordinate
                    *  4..7 -> robota Y koordinate
                    *  8..11 -> robota virziens grados
                    *  12..15 -> robota attalums no sakuma punkta
                    *  16..19 -> robota akumulatora limenis procentos
                    *  20 -> dati no Bumps-Wheeldrops sensora
             */
            // 5 int vertibas + 1 baits (jo int ir 4 baiti)
            byte[] data = new byte[(4 * 5) + 1];

            int i = 0;

            while (true)
            {
                Array.Clear(data, 0, data.Length);

                // Sutam akumulatora uzlades limeni un BumpWheeldrops datus
                Common.GetByteArrayFromInt((int)this.sensors.BatteryPercentage, data, 16);
                // dati no Bumps-Wheeldrops sensora
                data[20] = this.sensors.BumpsWheeldropsData;

                socket.SendTo(data, endpoint);

                // Katru simto reizi aizsutam vieninieku routerim
                if(i++ % 100 == 0)
                {
                    socket.SendTo(new byte[] { 1 }, routerEndpoint);
                }
                Thread.Sleep(50);
            }
        }
    }
}
