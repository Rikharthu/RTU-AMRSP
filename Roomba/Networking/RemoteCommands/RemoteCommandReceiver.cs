using System;
using Microsoft.SPOT;
using System.Threading;
using GHIElectronics.NETMF.Net.Sockets;
using GHIElectronics.NETMF.Net;
using Roomba.Utils;

namespace Roomba.Networking.RemoteCommands
{
    public class RemoteCommandReceiver
    {
        private bool stop;

        public delegate void RemoteCommandReceivedDelegate(RemoteCommand remoteCommand);
        public event RemoteCommandReceivedDelegate OnRemoteCommandReceived;

        private Thread workerThread;

        private const int MILLISECONDS_PER_MICROSECOND = 1000;

        public RemoteCommandReceiver()
        {
            this.stop = true;
        }

        public void Start()
        {
            if (this.stop)
            {
                this.stop = false;
                this.workerThread = new Thread(this.DoWork);
                this.workerThread.Start();
            }
        }

        public void Stop()
        {
            this.stop = true;
        }

        private void DoWork()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            // Lietotaja saskarsne suta uz robota IP adreses 12344 portu komandas
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 12344);

            socket.Bind(endpoint);
            // UI komanda ir 9 baitu virkne
            byte[] commandBuffer = new byte[1 + 4 * 2];

            EndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);

            while (!this.stop)
            {
                try
                {
                    if (socket.Poll(200 * MILLISECONDS_PER_MICROSECOND, SelectMode.SelectRead))
                    {
                        Array.Clear(commandBuffer, 0, commandBuffer.Length);
                        socket.ReceiveFrom(commandBuffer, ref remoteEndpoint);
                        RemoteCommand command = this.GetRemoteCommand(commandBuffer);

                        if (this.OnRemoteCommandReceived != null)
                        {
                            this.OnRemoteCommandReceived(command);
                        }
                    }
                }catch(Exception ex)
                {
                    Debug.Print("Error receiving command: " + ex.Message);
                }
            }
        }

        private RemoteCommand GetRemoteCommand(byte[] commandBuffer)
        {
            RemoteCommand command = new RemoteCommand();

            command.CommandType = (RemoteCommandType)commandBuffer[0];
            // 1..4
            command.FirstParam = Common.GetIntFromByteArray(commandBuffer, 1);
            // 5..9
            command.SecondParam = Common.GetIntFromByteArray(commandBuffer, 1 + 4);

            return command;
        }
    }
}
