using System;
using Microsoft.SPOT;
using System.Threading;

namespace Roomba
{
    public class Program
    {
        public static void Main()
        {
            Program p = new Program();
            //p.TestDriving();
            p.TestSensors();
            while (true) {
            }
            Debug.Print("Dead");
        }

        private void TestDriving()
        {
            RoombaController controller = new RoombaController();
            controller.Start();

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
            RoombaController controller = new RoombaController();
            controller.Start();
            controller.CmdExecutor.ExecCommand(RoombaCommand.Safe);

            controller.SubscribeToSensorPacket(SensorPacket.BatteryCharge, 2, 100, this.BatteryChargeDataReceived);
            controller.SubscribeToSensorPacket(SensorPacket.BumpsWheeldrops, 2,100, this.BumpWheeldropsDataReceived);
        }

        private void BumpWheeldropsDataReceived(short sensorData)
        {
            Debug.Print("Battery charge: " + sensorData);
        }

        private void BatteryChargeDataReceived(short sensorData)
        {
            byte bumpRightMask = 1;
            byte bumpLeftMask = 1 << 1;

            bool isBumpLeft = (sensorData & bumpLeftMask) != 0;
            bool isBumpRight = (sensorData & bumpRightMask) != 0;

            Debug.Print("Bumpw wheeldrops -> bump left: " + isBumpLeft+", bump right: "+isBumpRight);
        }
    }
}
