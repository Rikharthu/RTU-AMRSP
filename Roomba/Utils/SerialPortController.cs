using System;
using Microsoft.SPOT;
using System.Threading;
using System.IO.Ports;

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

        public byte[] Read(int count)
        {
            lock (this)
            {
                byte[] data = new byte[count];

                int bytesRead = 0;
                int offset = 0;

                while(offset < count)
                {
                    bytesRead = this.serialPort.Read(data, offset, data.Length - offset);
                    offset += bytesRead;
                }

                if(offset != count)
                {
                    Debug.Print("Serial Read fail");
                }

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
