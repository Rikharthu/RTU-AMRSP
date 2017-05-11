using System;
using Microsoft.SPOT;
using System.Threading;
using Roomba.Networking;
using Roomba.Roomba;
using Roomba.Tasks;

namespace Roomba
{
    public class Program : IRoombaWebController
    {
        const int MAX_BATTERY_CAPACITY_THEORETICAL = 65535;
        const int MAX_BATTERY_CAPACITY_PRACTICAL = 2696;
        private float lastKnownBatteryLevel = -1;
        private int status;

        RoombaController controller;
        private Task currentTask;

        public static void Main()
        {
            Program p = new Program();
            p.Run();
        }

        public void Run()
        {
            WebServer server = new WebServer();
            server.setOnWebInterractionListener(this);
            server.Start();
            controller = new RoombaController();

            status = WebServer.STATUS_STOPPED;

            while (true) { };

            Debug.Print("Dead");
        }

        private void StopCurrentTask()
        {
            if (this.currentTask != null)
            {
                this.currentTask.Stop();
            }
        }

        // ___IRoombaWebController___

        public float GetChargeLevel()
        {
            return lastKnownBatteryLevel;
        }

        public int GetStatus()
        {
            return status;
        }

        public void OnWebCommandDispatched(int code)
        {
            switch (code)
            {
                case WebServer.CODE_START:
                    //status = WebServer.STATUS_DRIVING;
                    StopCurrentTask();
                    currentTask = new TaskWander(controller);
                    currentTask.Start();
                    break;
                case WebServer.CODE_STOP:
                    //status = WebServer.STATUS_STOPPED;
                    StopCurrentTask();
                    currentTask = null;
                    break;
            }
        }
    }
}
