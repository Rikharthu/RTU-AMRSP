using System;
using Microsoft.SPOT;
using System.Threading;
using Roomba.Utils;

namespace Roomba.Roomba.Sensors
{
    public class SensorPacketQuerier
    {
        //!++ COMMENT TO COMPILE
        private RoombaCommandExecutor cmdExecutor;
        private SensorPacket sensorPacket;
        private int sensorPacketSize;
        private int frequency;

        private bool stop;

        public delegate void SensorDataReceivedDelegate(short sensorData);
        public event SensorDataReceivedDelegate SensorDataReceived;

        public SensorPacketQuerier(RoombaCommandExecutor cmdExecutor, SensorPacket sensorPacket, int sensorPacketSize, int frequency)
        {
            this.cmdExecutor = cmdExecutor;
            this.sensorPacket = sensorPacket;
            this.sensorPacketSize = sensorPacketSize;
            this.frequency = frequency;
        }

        public void Start()
        {
            this.stop = false;
            new Thread(this.DoWork).Start();
        }

        public void Stop()
        {
            stop = true;
            // wait some time until it is guaranteed to be stopped
            Thread.Sleep(frequency);
        }

        private void DoWork()
        {
            while (!stop)
            {
                byte[] sensorData = cmdExecutor.querySensorPacket(sensorPacket, sensorPacketSize);
                short sensorDataShort = 0;

                if (sensorPacketSize == 1)
                {
                    sensorDataShort = sensorData[0];
                }
                else if (sensorPacketSize == 2)
                {
                    // combine two bytes into short
                    sensorDataShort = Common.AssembleByte(sensorData[0], sensorData[1]);
                }

                if (SensorDataReceived != null)
                {
                    // trigger callback
                    SensorDataReceived(sensorDataShort);
                }

                Thread.Sleep(frequency);
            }
        }
    }
}
