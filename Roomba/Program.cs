using System;
using Microsoft.SPOT;
using System.Threading;

namespace Roomba
{
    public class Program
    {
        const int MAX_BATTERY_CAPACITY_THEORETICAL = 65535;
        const int MAX_BATTERY_CAPACITY_PRACTICAL = 2696;

        RoombaController controller;

        public static void Main()
        {
            Program p = new Program();
            p.Run();
        }

        public void Run()
        {
            controller = new RoombaController();
            controller.Start();
            //controller.CmdExecutor.ExecCommand(RoombaCommand.Safe);

            TestDriving();
            //TestSensors();
            //DoUzdevums2();
            while (true)
            {
            }

            Debug.Print("Dead");
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

            controller.CmdExecutor.DriveStraight(100);
        }

        //++ TODO check if there's no obstacles left (as sensor only triggers when something happens
        //+ => if you turned around, but still something blocks you, sensors wont trigger, as they state did not change yet
        private void Uzdevums2_BumpWheeldropsDataReceived(short sensorData)
        {
            int bumpCode = DecodeBump(sensorData);

            switch (bumpCode)
            {
                case 1:
                    // bump on left => turn right
                    controller.CmdExecutor.TurnRight(300);
                    Thread.Sleep(300);
                    break;
                case 2:
                    // bump on right => turn left
                    //controller.CmdExecutor.TurnLeft(300);
                    //Thread.Sleep(300);
                    break;
                case 3:
                    // both sides => 360 deg
                    controller.CmdExecutor.TurnLeft(100);
                    Thread.Sleep(2500);
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
            int chargePercent =(int)( (float)sensorData / MAX_BATTERY_CAPACITY_PRACTICAL*100);
            controller.CmdExecutor.ShowDigitsASCII(chargePercent+"%");

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
    }
}
