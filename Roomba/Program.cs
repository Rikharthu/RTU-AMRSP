using System;
using Microsoft.SPOT;
using System.Threading;
using Roomba.Networking;
using Roomba.Roomba;

namespace Roomba
{
    public class Program : IRoombaWebController
    {
        const int MAX_BATTERY_CAPACITY_THEORETICAL = 65535;
        const int MAX_BATTERY_CAPACITY_PRACTICAL = 2696;
        private float lastKnownBatteryLevel = -1;
        private int status;

        RoombaController controller;
        private Tasks currentTask;

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
            status = WebServer.STATUS_STOPPED;

            Random rnd = new Random();

            while (true)
            {
                if (status == WebServer.STATUS_DRIVING)
                {
                    if (this.controller.Sensors.IsBump)
                    {
                        int bumpCode = DecodeBump(controller.Sensors.BumpsWheeldropsData);
                        int randomDelay = (int)(rnd.NextDouble() * 300) + 300;

                        switch (bumpCode)
                        {
                            case 1:
                                // bump on left => turn right
                                controller.CmdExecutor.DriveStraight(-600);
                                Thread.Sleep(100);
                                controller.CmdExecutor.TurnRight(300);
                                Thread.Sleep(randomDelay);
                                break;
                            case 2:
                                // bump on right => turn left
                                controller.CmdExecutor.DriveStraight(-600);
                                Thread.Sleep(100);
                                controller.CmdExecutor.TurnLeft(300);
                                Thread.Sleep(randomDelay);
                                break;
                            case 3:
                                // both sides => 360 deg
                                controller.CmdExecutor.DriveStraight(-600);
                                Thread.Sleep(100);
                                controller.CmdExecutor.TurnLeft(100);
                                Thread.Sleep(randomDelay + 100);
                                controller.CmdExecutor.Stop();
                                break;
                        }
                    }
                    else
                    {
                        controller.CmdExecutor.DriveStraight(100);
                        Thread.Sleep(50);
                    }

                    if (lastKnownBatteryLevel != controller.Sensors.BatteryPercentage)
                    {
                        // propagate an update
                        lastKnownBatteryLevel = controller.Sensors.BatteryPercentage;
                        controller.CmdExecutor.ShowDigitsASCII(lastKnownBatteryLevel + "");
                    }
                }else
                {
                    controller.CmdExecutor.Stop();
                }
                
            }
            Debug.Print("Dead");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sensorData"></param>
        /// <returns>
        /// 0 if no bump
        /// 1 if bump only from left side
        /// 2 if bump only from right side
        /// 3 if bump on both sides
        /// </returns>
        private int DecodeBump(short sensorData)
        {
            byte bumpRightMask = 1;
            byte bumpLeftMask = 1 << 1;

            bool isBumpLeft = (sensorData & bumpLeftMask) != 0;
            bool isBumpRight = (sensorData & bumpRightMask) != 0;

            Debug.Print("Bump wheeldrops -> bump left: " + isBumpLeft + ", bump right: " + isBumpRight);

            if (isBumpLeft && isBumpRight)
            {
                return 3;
            }
            else if (isBumpLeft)
            {
                return 1;
            }
            else if (isBumpRight)
            {
                return 2;
            }
            else
            {
                return 0;
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

        public void OnCommandDispatched(int code)
        {
            switch (code)
            {
                case WebServer.CODE_START:
                    status = WebServer.STATUS_DRIVING;
                    break;
                case WebServer.CODE_STOP:
                    status = WebServer.STATUS_STOPPED;
                    break;
            }
        }
    }
}
