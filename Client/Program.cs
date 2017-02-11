using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        private void RunClient()
        {
            IPEndPoint serverEndpoint = new IPEndPoint(
                new IPAddress(new byte[] { 192, 168, 1, 10 }), // ierices (servera) adrese
                12345); // porta numurs

            while (true)
            {
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    client.Connect(serverEndpoint);

                    SendRequestToServer(client);
                    ReadServerResponse(client);
                }
                catch (Exception e)
                {
                    Debug.Print("Failed to connect to server: " + e.Message);
                }
                finally
                {
                    client.Close();
                    Thread.Sleep(1000);
                }
            }
        }

        private void ReadServerResponse(Socket client)
        {
            const int microSecondsPerSecond = 1000000;

            // gaidit dauts no servera 10 sekundes
            if (client.Poll(10 * microSecondsPerSecond, SelectMode.SelectRead) && client.Available>0)
            {
                byte[] response
            }
        }

        private void SendRequestToServer(Socket client)
        {
            byte[] dataToSend = Encoding.UTF8.GetBytes("Hello server!");
            client.Send(dataToSend);
        }
    }
}
