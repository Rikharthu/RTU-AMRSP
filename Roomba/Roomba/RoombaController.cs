using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using System.Threading;

namespace Roomba
{
    class RoombaController
    {
        // from Roomba datasheet
        private const String SERIAL_PORT_NAME = "COM2";
        private const int SERIAL_PORT_BAUD_RATE = 115200;

        private SerialPortController serialPort;
        private RoombaCommandExecutor cmdExecutor;

        private OutputPort wakeupSignalPort;

        public RoombaCommandExecutor CmdExecutor
        {
            get { return this.cmdExecutor; }
        }

        public RoombaController()
        {
            this.serialPort = new SerialPortController(SERIAL_PORT_NAME, SERIAL_PORT_BAUD_RATE);
            this.cmdExecutor = new RoombaCommandExecutor(this.serialPort);
            // connected to serial port 5 DD (Device Detect), which is used to wake up Roomba
            this.wakeupSignalPort = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di47, true);
        }

        //!++ COMMENT TO COMPILE
        public void SubscribeToSensorPacket(SensorPacket sensorPacket, int sensorPacketSize, int frequency,
            SensorPacketQuerier.SensorDataReceivedDelegate dataReceivedDelegate)
        {
            SensorPacketQuerier querier = new SensorPacketQuerier(cmdExecutor, sensorPacket, sensorPacketSize, frequency);
            querier.SensorDataReceived += dataReceivedDelegate;
            querier.Start();
        }

        public void Start()
        {
            // Wakeup Roomba
            this.SendWakeupSignal();
            // Start Roomba
            this.cmdExecutor.ExecCommand(RoombaCommand.Start);
            // Allow controlling  through serial port
            this.cmdExecutor.ExecCommand(RoombaCommand.Control);

            this.cmdExecutor.ExecCommand(RoombaCommand.FullControl);
        }

        public void TurnOff()
        {
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
