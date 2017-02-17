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

        const int DELAY = 2000;

        static void Main(string[] args)
        {
            Program p = new Program();
            p.RunClient();
        }

        private void RunClient()
        {
            IPEndPoint serverEndpoint = new IPEndPoint(
                new IPAddress(new byte[] { 192, 168, 17, 36 }), // ierices (servera) adrese
                12345); // porta numurs
            Console.Write("Starting client...\n");
            while (true)
            {
                // TCP socket
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    // try to connect to the specified socket
                    client.Connect(serverEndpoint);

                    // write to socket
                    //SendRequestToServer(client);
                    SendRequestToServerV3(client);
                    // now read response from that socket
                    //ReadServerResponse(client);
                    ReadServerResponseV3(client);
                }
                catch (Exception e)
                {
                    Console.Write("Failed to connect to server: " + e.Message + "\n");
                }
                finally
                {
                    client.Close();
                    Thread.Sleep(DELAY);
                }
            }
        }

        private void ReadServerResponse(Socket client)
        {
            const int microSecondsPerSecond = 1000000;

            // gaidit dauts no servera 10 sekundes
            if (client.Poll(10 * microSecondsPerSecond,
                SelectMode.SelectRead) // READ mode
                && client.Available > 0)
            {
                byte[] response = new byte[client.Available];
                client.Receive(response, response.Length, SocketFlags.None);
                String responseString = new string(Encoding.UTF8.GetChars(response));
                Console.Write("Response from server: '" + responseString + "'\n");
            }
        }

        private void ReadServerResponseV2(Socket client)
        {
            const int microSecondsPerSecond = 1000000;

            // gaidit dauts no servera 10 sekundes
            if (client.Poll(10 * microSecondsPerSecond, SelectMode.SelectRead) && client.Available > 0)
            {
                byte[] response = new byte[client.Available];
                client.Receive(response, response.Length, SocketFlags.None);
                String responseByteString = "";
                for (int i = 0; i < response.Length; i++)
                {
                    responseByteString += response[i].ToString() + ", ";
                }
                Console.Write("Response from server: '" + responseByteString + "'\n");
            }
        }

        private void ReadServerResponseV3(Socket client)
        {
            const int microSecondsPerSecond = 1000000;

            // gaidit dauts no servera 10 sekundes 
            // parbaude - vai no si soketa ir ko lasit (gaidam 10 sek)
            if (client.Poll(10 * microSecondsPerSecond, SelectMode.SelectRead) && client.Available > 0)
            {
                byte[] response = new byte[client.Available];
                client.Receive(response, response.Length, SocketFlags.None);
                byte highByte = response[0];
                byte lowByte = response[1];

                // combine high and low bytes
                //int result = lowByte | (highByte << 8);
                // better use shorts, since it has exactly 2 bytes (int has 4 bytes=> needs two's complement)
                short result = (short)(lowByte | (highByte << 8));
                // check if highest byte is 1 (=> it is negative)
                //if (highByte >> 7 == 1)
                //{
                //    // highest byte is 1 => result is negative
                //    // perform a two's complement operation
                //    // invert bits and add 1
                //    result = -((result ^ 0xffff) + 1);
                //}

                //Console.Write("Answer: " + result+"\n");
                Console.Write(String.Format("Answer from server: {0} ({1}, {2})\n", result, highByte, lowByte));
            }
        }

        private void SendRequestToServer(Socket client)
        {
            byte[] dataToSend = Encoding.UTF8.GetBytes("Hello server!");
            client.Send(dataToSend);
        }

        private void SendRequestToServerV2(Socket client)
        {
            Random r = new Random();
            byte[] dataToSend = new byte[2];
            dataToSend[0] = (byte)r.Next(101);
            dataToSend[1] = (byte)r.Next(101);
            Console.Write("Sending to server: " + dataToSend[0] + ", " + dataToSend[1] + "\n");
            client.Send(dataToSend);
        }

        private void SendRequestToServerV3(Socket client)
        {
            Random r = new Random();
            byte[] dataToSend = new byte[3];
            dataToSend[0] = (byte)(r.Next(2) + 1);
            dataToSend[1] = (byte)r.Next(256);
            dataToSend[2] = (byte)r.Next(256);
            Console.Write(String.Format("Asking server: {0} {1} {2} = ?\n", dataToSend[1], dataToSend[0] == 1 ? '+' : '-', dataToSend[2]));
            client.Send(dataToSend);
        }
    }
}
