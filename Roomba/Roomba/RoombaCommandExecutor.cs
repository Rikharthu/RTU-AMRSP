using System;
using Microsoft.SPOT;
using Roomba.Utils;

namespace Roomba.Roomba
{
    public class RoombaCommandExecutor
    {
        SerialPortController serialPortController;
        public static Object querySensorLock = new object();


        public RoombaCommandExecutor(SerialPortController serialPortController)
        {
            this.serialPortController = serialPortController;
        }

        public void ExecGeneralCommand(byte[] command)
        {
            this.serialPortController.Write(command);
        }

        public void ExecCommand(RoombaCommand command, byte[] parameters)
        {
            byte[] commandBytes = new byte[parameters.Length + 1];
            commandBytes[0] = (byte)command;
            parameters.CopyTo(commandBytes, 1); // offset 1
            logCommand(command, parameters);
            this.ExecGeneralCommand(commandBytes);
        }

        public void ExecCommand(RoombaCommand command)
        {
            byte[] commandBytes = new byte[] { (byte)command };
            logCommand(command, null);
            this.ExecGeneralCommand(commandBytes);
        }

        public byte[] querySensorPacket(SensorPacket sensorPacket, int packetSize)
        {
            byte[] parameters = new byte[]
            {
                (byte)sensorPacket
            };

            lock (querySensorLock)
            {
                this.ExecCommand(RoombaCommand.QuerySensorPacket, parameters);
                return this.serialPortController.Read(packetSize);
            }
        }

        //++ High Level commands:
        // prepares and sends Roomba Drive command
        public void Drive(short velocity, short radius)
        {
            byte[] parameters =
            {
                // See RoombaCommand.Drive for more info
                Common.GetHighByte(velocity),
                Common.GetLowByte(velocity),
                Common.GetHighByte(radius),
                Common.GetLowByte(radius)
            };
            logCommand(RoombaCommand.Drive, parameters);
            this.ExecCommand(RoombaCommand.Drive, parameters);
        }

        // Drive forward at velocity mm/s
        public void DriveStraight(short velocity)
        {
            // 32767 - roomba constant for forward radius
            this.Drive(velocity, 32767);
        }

        public void DriveWheels(short rightVelocity, short leftVelocity)
        {
            byte[] parameters =
            {
                Common.GetHighByte(rightVelocity),
                Common.GetLowByte(rightVelocity),
                Common.GetHighByte(leftVelocity),
                Common.GetLowByte(leftVelocity)
            };
            logCommand(RoombaCommand.DriveWheels, parameters);
            this.ExecCommand(RoombaCommand.DriveWheels, parameters);
        }


        public void TurnRight(short velocity)
        {
            // -1 - right radius
            this.Drive(velocity, -1);
        }

        public void TurnLeft(short velocity)
        {
            // +1 - left radius
            this.Drive(velocity, 1);
        }

        public void Stop()
        {
            this.Drive(0, 0);
        }

        public void ShowDigitsASCII(String str)
        {
            if (str == null) return;

            byte[] parameters = new byte[4];

            for (int i = 0; i < parameters.Length; i++)
            {
                if (i <= str.Length - 1)
                {
                    parameters[i] = (byte)str[i];
                }
                else
                {
                    // fill blank space if str is shortet than 4 chars
                    parameters[i] = 32;
                }
            }

            ExecCommand(RoombaCommand.DigitLedsASCII, parameters);
        }

        private void logCommand(RoombaCommand cmd, byte[] parameters)
        {
            //string output = "Sending command " + Enum.GetName(typeof(RoombaCommand), cmd) + ", " + cmd;
            string output = "Sensing command " + cmd.ToString();
            if (parameters != null)
            {
                output += ", paramters: ";
                foreach (byte b in parameters)
                {
                    output += " " + b;
                }
            }
            //Debug.Print(output);
        }
    };
}
