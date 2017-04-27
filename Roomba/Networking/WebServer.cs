using System;
using Microsoft.SPOT;
using System.Text;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.Net;
using GHIElectronics.NETMF.Net.NetworkInformation;
using GHIElectronics.NETMF.FEZ;
using System.Threading;
using GHIElectronics.NETMF.Net.Sockets;

namespace Roomba.Networking
{
    public class WebServer
    {
        public const int STATUS_DRIVING = 5;
        public const int STATUS_STOPPED = 6;
        public const int CODE_START = 1;
        public const int CODE_STOP = 2;

        private IRoombaWebController roombaWebCtrl;

        Thread pingThread;

        private void StartWebServer()
        {
            // izvietot HttpListener objektu uz 80. porta
            HttpListener httpListener = new HttpListener("http", 80);
            // sakt klausities
            httpListener.Start();

            HttpListenerContext httpContext = null;

            while (true)
            {
                try
                {
                    // wait for incoming request
                    // iegut context'u kas satur pieprasijumu
                    // wait for the incoming request
                    httpContext = httpListener.GetContext();
                    if (httpContext == null) continue;

                    // sagatabot atbildi
                    byte[] response = this.PrepareResponseV3LED(httpContext);
                    // nosutit atbildi
                    this.SendResponse(httpContext, response, 202); // 202 - ACCEPTED
                }
                catch (Exception e)
                {
                    Debug.Print("Error processing listener context: " + e.Message);
                }
                finally
                {
                    if (httpContext != null)
                    {
                        httpContext.Close();
                    }
                }
            }
        }

        public void setOnWebInterractionListener(IRoombaWebController rmbWebCtrl)
        {
            roombaWebCtrl = rmbWebCtrl;
        }

        /**  Control a Roomba */
        private byte[] PrepareResponseV3LED(HttpListenerContext context)
        {
            bool isStartButtonPressed = false;
            bool isStopButtonPressed = false;


            if (context.Request.HttpMethod == "POST")
            {
                // from submitted form we get "buttonTwo=Button+Two+%3A%28" if second button is pressed
                String contentstring = this.GetContentString(context.Request);

                isStartButtonPressed = contentstring.IndexOf("btn_start") != -1;

                isStopButtonPressed = contentstring.IndexOf("btn_stop") != -1;
            }

            // TODO extract to resources
            String responseString =
                  @"<html>
                    <title>-= RoombaMyAdmin =-</title>
                    <body border-style=""ridge"">
                        <form action= """" method=""post"">
                            <hl><strong><font face=""verdana"" color=""9781aa"">roomba</font><font color=""d1a214"" face=""verdana"">MyAdmin</font></strong></hl>";

            //!+ Process here

            if (roombaWebCtrl != null)
            {
                if (isStartButtonPressed)
                {
                    roombaWebCtrl.OnCommandDispatched(CODE_START);
                }
                if (isStopButtonPressed)
                {
                    roombaWebCtrl.OnCommandDispatched(CODE_STOP);
                }
            }

            // current roomba status
            string status;
            switch (roombaWebCtrl.GetStatus())
            {
                case STATUS_DRIVING:
                    status = "driving";
                    break;
                case STATUS_STOPPED:
                    status = "stopped";
                    break;
                default:
                    status = "unknown";
                    break;
            }

            responseString += "<div>Status: " + status + "</div>";

            responseString += @"
                            <div><input type=""submit"" name=""btn_start"" value=""Start""/></div>
                            <div><input type=""submit"" name=""btn_stop"" value=""Stop""/></div>"
                        + "<div>Charge: " + (roombaWebCtrl.GetChargeLevel() == -1 ? "Unknown" : ((int)(roombaWebCtrl.GetChargeLevel() * 100) + "%")) + "</div>" +
                        @"</form>
                    </body>
                </html>";

            return Encoding.UTF8.GetBytes(responseString);
        }

        private String GetContentString(HttpListenerRequest request)
        {
            byte[] requestData = new byte[(int)request.InputStream.Length];
            request.InputStream.Read(requestData, 0, requestData.Length);
            return new String(Encoding.UTF8.GetChars(requestData));
        }

        private void SendResponse(HttpListenerContext context, byte[] response, int statusCode)
        {
            // html status code
            context.Response.StatusCode = statusCode;
            // informet klientu ka atbilde ir html kods
            context.Response.ContentType = "text/html";
            try
            {
                // ierakstit sagatavotus datus atbildes plusmaa
                context.Response.OutputStream.Write(response, 0, response.Length);
            }
            finally
            {
                // send and close
                context.Response.Close();
            }
        }

        /** Configure Wi-Fi shield and setup internet settings */
        private void EnableNetworking()
        {
            WIZnet_W5100.Enable(SPI.SPI_module.SPI1,
                (Cpu.Pin)FEZ_Pin.Digital.Di10,
                (Cpu.Pin)FEZ_Pin.Digital.Di7,
                false);

            // TODO пояснить что и зачем
            byte[] ipAddress = new byte[] { 192, 168, 17, 36 };
            byte[] netmask = new byte[] { 255, 255, 255, 0 };
            byte[] gateway = new byte[] { 192, 168, 17, 1 };
            // set our own mac address for this device
            byte[] macAddress = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x36 };

            NetworkInterface.EnableStaticIP(ipAddress, netmask, gateway, macAddress);
        }

        private void Setup()
        {
        }

        private void startPing()
        {
            pingThread = new Thread(this.Ping);
            pingThread.Start();
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

                for (int i = 0; i < 5; i++)
                {
                    // шлём роутеру (reouterEndPoint) данные через наш сокет
                    socket.SendTo(new byte[] { 1 }, routerEndPoint);
                    Thread.Sleep(100);
                }

                // закрываем соединение
                socket.Close();
                Thread.Sleep(500);
            }
        }

        public void Start()
        {
            Setup();
            EnableNetworking();
            startPing();
            StartWebServer();
        }
    }

    public interface IRoombaWebController
    {
        float GetChargeLevel();
        int GetStatus();
        void OnCommandDispatched(int code);
    }
}
