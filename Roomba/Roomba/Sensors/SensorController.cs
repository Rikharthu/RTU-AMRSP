using System;
using Microsoft.SPOT;

namespace Roomba.Roomba.Sensors
{
    public class SensorController
    {
        public const int CODE_BUMP_NONE = 0;
        public const int CODE_BUMP_LEFT = 1;
        public const int CODE_BUMP_RIGHT = 2;
        public const int CODE_BUMP_BOTH = 3;

        private short batteryCharge;
        private RoombaController roombaController;

        public int BumpCode
        {
            get; private set;
        }

        public int BatteryPercentage
        {
            get; private set;
        }

        public bool IsBump
        {
            get; private set;
        }

        public byte BumpsWheeldropsData { get; private set; }

        public SensorController(RoombaController roombaController)
        {
            this.roombaController = roombaController;
        }

        public void StartSensors()
        {
            this.roombaController.SubscribeToSensorPacket(SensorPacket.BumpsWheeldrops, 1, 20, this.BumpWheeldropsReceived);
            this.roombaController.SubscribeToSensorPacket(SensorPacket.BatteryCharge, 2, 100, this.BatteryChargeReceived);
            // TODO
            //this.roombaController.SubscribeToSensorPacket(SensorPacket.BatteryCapacity, 2, 1000, this.BatteryCapacityReceived);
        }

        private void BumpWheeldropsReceived(short sensorData)
        {
            byte bumpRightMask = 1;     // 0b00000001
            byte bumpLeftMask = 1 << 1; // 0b00000010

            bool isBumpLeft = (sensorData & bumpLeftMask) != 0;
            bool isBumpRight = (sensorData & bumpRightMask) != 0;

            this.IsBump = isBumpLeft | isBumpRight;

            this.BumpsWheeldropsData = (byte)sensorData;

            if (isBumpLeft && isBumpRight)
            {
                BumpCode = CODE_BUMP_BOTH;
            }
            else if (isBumpLeft)
            {
                BumpCode = CODE_BUMP_LEFT;
            }
            else if (isBumpRight)
            {
                BumpCode = CODE_BUMP_RIGHT;
            }
            else
            {
                BumpCode = CODE_BUMP_NONE;
            }
        }

        private void BatteryChargeReceived(short sensorData)
        {
            this.batteryCharge = (short)sensorData;
        }

        private void BatteryCapacityReceived(short sensorData)
        {
            // TODO temporary solution 
            sensorData = 2696;
            if (sensorData != 0)
            {
                this.BatteryPercentage = (int)System.Math.Round(this.batteryCharge / ((double)sensorData) * 100);
            }
            else
            {
                this.BatteryPercentage = 0;
            }
            Debug.Print("Battery left: " + this.BatteryPercentage);
            this.roombaController.CmdExecutor.ShowDigitsASCII(this.BatteryPercentage.ToString());
        }
    }
}
