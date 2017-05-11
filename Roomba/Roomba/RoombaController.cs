using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using System.Threading;
using System.Collections;
using Roomba.Roomba.Sensors;

namespace Roomba.Roomba
{
    public class RoombaController
    {
        // from Roomba datasheet
        private const String SERIAL_PORT_NAME = "COM2";
        private const int SERIAL_PORT_BAUD_RATE = 115200;

        private SerialPortController serialPort;
        private RoombaCommandExecutor cmdExecutor;

        private OutputPort wakeupSignalPort;
        private Stack queriers;

        public SensorController Sensors { get; private set; }

        public RoombaCommandExecutor CmdExecutor
        {
            get { return this.cmdExecutor; }
        }

        public RoombaController()
        {
            this.queriers = new Stack();
            this.serialPort = new SerialPortController(SERIAL_PORT_NAME, SERIAL_PORT_BAUD_RATE);
            this.cmdExecutor = new RoombaCommandExecutor(this.serialPort);
            // connected to serial port 5 DD (Device Detect), which is used to wake up Roomba
            this.wakeupSignalPort = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di47, true);

            this.Sensors = new SensorController(this);
        }

        public void SubscribeToSensorPacket(SensorPacket sensorPacket, int sensorPacketSize, int frequency,
            SensorPacketQuerier.SensorDataReceivedDelegate dataReceivedDelegate)
        {
            SensorPacketQuerier querier = new SensorPacketQuerier(cmdExecutor, sensorPacket, sensorPacketSize, frequency);
            querier.SensorDataReceived += dataReceivedDelegate;
            queriers.Push(querier);
            querier.Start();
        }

        public void Start()
        {
            // Wakeup Roomba
            this.SendWakeupSignal();
            Thread.Sleep(500);
            // Start Roomba
            this.cmdExecutor.ExecCommand(RoombaCommand.Start);
            Thread.Sleep(50);
            // Allow controlling  through serial port
            this.cmdExecutor.ExecCommand(RoombaCommand.Control);
            Thread.Sleep(50);
            this.cmdExecutor.ExecCommand(RoombaCommand.FullControl);
            Thread.Sleep(50);

            this.Sensors.StartSensors();
        }

        public void TurnOff()
        {
            // remove stop the sensor queriers
            while (queriers.Count != 0)
            {
                ((SensorPacketQuerier)queriers.Pop()).Stop();
            }

            this.cmdExecutor.ExecCommand(RoombaCommand.Safe);
            this.cmdExecutor.ExecCommand(RoombaCommand.PowerOff);
            this.serialPort.Close();
        }

        /// <summary>
        /// Wakeup Roomba by setting "Device Detect" pin to "low" for 500ms
        /// </summary>
        public void SendWakeupSignal()
        {
            this.wakeupSignalPort.Write(false);
            Thread.Sleep(500);
            this.wakeupSignalPort.Write(true);
        }

    }
}
