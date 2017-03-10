using System;
using System.IO.Ports;
using Microsoft.SPOT;
using System.Threading;

namespace Roomba
{
    class SerialPortController
    {
        private SerialPort serialPort;

        private static Object readWriteLock = new object();

        public SerialPortController(String portName, int baudRate)
        {
            this.serialPort = new SerialPort(portName, baudRate);
            this.Open();
        }

        private void Open()
        {
            if (!this.serialPort.IsOpen)
            {
                this.serialPort.Open();
            }
        }

        public void Write(byte[] dataToWrite)
        {
            lock (readWriteLock)
            {
                this.Open();
                this.serialPort.Write(dataToWrite, 0, dataToWrite.Length);
                Thread.Sleep(50);
            }
        }

        public byte[] Read(int bytesToRead)
        {
            lock (readWriteLock)
            {
                this.Open();
                byte[] data = new byte[bytesToRead];
                this.serialPort.Read(data, 0, data.Length);
                return data;
            }
        }

        public void Close()
        {
            if (this.serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }
    }
}
