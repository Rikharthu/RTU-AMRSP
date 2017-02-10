using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using System.Threading;

namespace ButtonClicker
{
    // TODO create setup() and loop() method
    // TODO implement same functionality with InterruptPort
    public class Program
    {

        private static InputPort button;
        private static OutputPort led;
        private static bool oldButton;


        public static void Setup()
        {
            // Izveido ieejas portu uz norādītās CPU kājas 
            button = new InputPort(
                (Cpu.Pin)FEZ_Pin.Digital.LDR,   // LDR - Loader
                false, // Trokšņu filtrs: false -> izslēgts, true -> ieslēgts 
                Port.ResistorMode.PullUp); // Uzstādīt kāju uz high, kad nekas nav pievienots 

            led = new OutputPort(
                (Cpu.Pin)FEZ_Pin.Digital.LED,
                false);

            oldButton = false;
        }

        public static void Loop()
        {
            // Nolasīt kājas stāvokli : -true, high, false -> low Kad LDR poga ir nospiesta (jo ir pull-up mode),
            // tā savieno LDR kāju ar zemi, iestatot to uz low. 
            bool buttonPressed = !button.Read();
            bool buttonToggled = buttonPressed && buttonPressed != oldButton;
            if (buttonToggled)
            {
                led.Write(!led.Read());
                Debug.Print("Button Toggled");
            }
            Thread.Sleep(100);
            oldButton = buttonPressed;
        }

        public static void Main()
        {
            Setup();
            while (true) Loop();
        }
    }
}
