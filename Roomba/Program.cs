using System;
using Microsoft.SPOT;
using System.Threading;
using Roomba.Networking;
using Roomba.Roomba;
using Roomba.Tasks;
using Roomba.Networking.StatusSending;
using Roomba.Networking.RemoteCommands;

namespace Roomba
{
    public class Program : IRoombaWebController
    {
        const int MAX_BATTERY_CAPACITY_THEORETICAL = 65535;
        const int MAX_BATTERY_CAPACITY_PRACTICAL = 2696;
        private float lastKnownBatteryLevel = -1;
        private int status;

        private RoombaController controller;
        private RobotStatusSender robotStatusSender;
        private NetworkManager networkManager;
        private Task currentTask;

        private RemoteCommandReceiver remoteCommandReceiver;


        public static void Main()
        {
            Program p = new Program();
            p.Execute();
            Thread.Sleep(-1);
        }

        private Program()
        {
            this.controller = new RoombaController();
            this.robotStatusSender = new RobotStatusSender(this.controller.Sensors);
            this.networkManager = new NetworkManager();

            this.remoteCommandReceiver = new RemoteCommandReceiver();
            this.remoteCommandReceiver.OnRemoteCommandReceived +=
                new RemoteCommandReceiver.RemoteCommandReceivedDelegate(remoteCommandReceiver_RemoteCommandReceived);
        }

        private void remoteCommandReceiver_RemoteCommandReceived(RemoteCommand remoteCommand)
        {
            this.StopCurrentTask();

            if (remoteCommand.CommandType == RemoteCommandType.Drive)
            {
                Debug.Print("Received \"Drive\" command");
                this.controller.CmdExecutor.DriveWheels(
                    (short)remoteCommand.FirstParam, (short)remoteCommand.SecondParam);
            }else if (remoteCommand.CommandType == RemoteCommandType.ResetLocation)
            {
                Debug.Print("Received \"Reset Location\" command");
                // to be implemented
            }
            else if(remoteCommand.CommandType == RemoteCommandType.Wander)
            {
                Debug.Print("Received \"Wander\" command");
                this.currentTask = new TaskWander(this.controller);
                this.currentTask.Start();
            }
        }

        public void Execute()
        {
            this.networkManager.Start();
            this.controller.Start();
            this.robotStatusSender.Start();
            this.remoteCommandReceiver.Start();
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
