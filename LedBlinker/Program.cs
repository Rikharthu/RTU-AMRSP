using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using System.Threading;

namespace LedBlinker
{
    public class Program
    {
        public const int MAX_DELAY = 1000;
        public const int MIN_DELAY = 0;
        public const int DELTA_DELAY = 20;

        private static int delay = MAX_DELAY;


        public static void Main()
        {
            // Izveido izejas portu uz atbilstošo CPU “kāju” (pin) 
            OutputPort led = new OutputPort(
                (Cpu.Pin)FEZ_Pin.Digital.LED, // Ražotāja specificētais  CPU kājas numurs 
                false); // Sākotnejais stāvoklis: - false -> low (izslēgts) - true -> high (ieslēgts) 

            while (true)
            {
                Debug.Print("Delay: " + delay);
                // Iestatīt kāju uz high 
                led.Write(true);
                Thread.Sleep(delay);
                // Iestatīt kāju uz low 
                led.Write(false);
                Thread.Sleep(delay);
                if (delay > MIN_DELAY)
                {
                    delay -= DELTA_DELAY;
                }
                else
                {
                    Debug.Print("KA-BOOM!");
                    delay = MAX_DELAY;
                }
            }
        }
    }
}
