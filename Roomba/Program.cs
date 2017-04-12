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
        public static int lastKnownBatteryLevel = -1;

        RoombaController controller;

        public static void Main()
        {
            Program p = new Program();
            p.Run();
        }

        public void Run()
        {
            //controller = new RoombaController();
            //controller.Start();
            //controller.CmdExecutor.ExecCommand(RoombaCommand.Safe);

            //!++ UNCOMMENT TO STOP
            //Stop();

            //TestDriving();
            //TestSensors();

            //!+ Uzdevums 2
            //DoUzdevums2();

            WebServer server = new WebServer();
            server.setOnWebInterractionListener(this);
            server.Start();

            while (true)
            {
            }

            Debug.Print("Dead");
        }

        private void Stop()
        {
            if (controller != null)
            {
                
                controller.CmdExecutor.Stop();
                controller.TurnOff();
                controller = null;
            }
        }

        private void TestDriving()
        {
            // move forward for 5 seconds
            controller.CmdExecutor.DriveStraight(300);
            Thread.Sleep(1000);

            controller.CmdExecutor.Stop();

            // turn around for 1.3 seconds (~180 degrees)
            controller.CmdExecutor.TurnLeft(300);
            Thread.Sleep(1300);

            controller.CmdExecutor.Stop();

            // repeat
            controller.CmdExecutor.DriveStraight(100);
            Thread.Sleep(5000);

            controller.CmdExecutor.TurnLeft(300);
            Thread.Sleep(1300);

            controller.CmdExecutor.Stop();
            controller.TurnOff();
        }

        private void TestSensors()
        {
            controller.SubscribeToSensorPacket(SensorPacket.BatteryCharge, 2, 100, this.BatteryChargeDataReceived);
            controller.SubscribeToSensorPacket(SensorPacket.BumpsWheeldrops, 1, 100, this.BumpWheeldropsDataReceived);
        }

        private void DoUzdevums2()
        {
            controller.SubscribeToSensorPacket(SensorPacket.BatteryCharge, 2, 100, this.BatteryChargeDataReceived);
            controller.SubscribeToSensorPacket(SensorPacket.BumpsWheeldrops, 1, 100, this.Uzdevums2_BumpWheeldropsDataReceived);
        }

        //++ TODO check if there's no obstacles left (as sensor only triggers when something happens
        //+ => if you turned around, but still something blocks you, sensors wont trigger, as they state did not change yet
        private void Uzdevums2_BumpWheeldropsDataReceived(short sensorData)
        {
            int bumpCode = DecodeBump(sensorData);

            Random rnd = new Random();

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
                default:
                    controller.CmdExecutor.DriveStraight(100);
                    break;
            }
            controller.CmdExecutor.DriveStraight(100);
        }

        private void BatteryChargeDataReceived(short sensorData)
        {
            Debug.Print("Battery charge: " + sensorData);
            int chargePercent = (int)((float)sensorData / MAX_BATTERY_CAPACITY_PRACTICAL * 100);
            lastKnownBatteryLevel = chargePercent;
            controller.CmdExecutor.ShowDigitsASCII(chargePercent + "%");

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

        private void BumpWheeldropsDataReceived(short sensorData)
        {
            int bumpCode = DecodeBump(sensorData);
        }

        // OnWebInterractionListener
        public void OnInterraction(int code)
        {
            switch (code)
            {
                case WebServer.CODE_START:
                    Debug.Print("Received CODE_START");
                    // every time restart again
                    controller = new RoombaController();
                    controller.Start();
                    DoUzdevums2();
                    break;
                case WebServer.CODE_STOP:
                    Debug.Print("Received CODE_STOP");
                    Stop();
                    break;
            }
        }

        // ___IRoombaWebController___

        public float GetChargeLevel()
        {
            throw new NotImplementedException();
        }

        public int GetStatus()
        {
            throw new NotImplementedException();
        }

        public void OnCommandDispatched(int code)
        {
            throw new NotImplementedException();
        }
    }
}
