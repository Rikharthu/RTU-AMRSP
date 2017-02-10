using System;
using Microsoft.SPOT;

namespace NetWorker
{
    //+++ NOTE
    // lorem ipsum porem
    
    public class Program
    {
        public static void Setup()
        {
            Debug.Print(Resources.GetString(Resources.StringResources.String1));
            Debug.Print(Resources.GetString(Resources.StringResources.String2));
        }

        public static void Loop()
        {

        }

        public static void Main()
        {          
            Setup();

            while (true)
            {
                Loop();
            }
        }
    }
}
