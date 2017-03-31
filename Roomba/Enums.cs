using System;
using Microsoft.SPOT;

namespace Roomba
{
    public enum RoombaCommand
    {
        Start = 128,
        Control = 130,
        Safe = 131,
        FullControl = 132,
        PowerOff = 133,
        /// <summary>
        ///  Roomba Drive (137) command
        ///  Has 4 parameters:
        ///  - Velocity high byte
        ///  - Velocity low byte
        ///  - Radius high byte
        ///  - Radius low byte
        ///  Velocity - [-500;+500]
        ///  Radius - [-2000;+2000] (32768 or 32767 for forward, -1 for clockwise and +1 for counter-clockwise mode)
        /// </summary>
        Drive = 137,
        DigitLedsASCII = 164,
        /// <summary>
        /// Pieprasit sensoru datus no Roomba
        /// Parameter: pieprasitas sensoru paketes numurs
        /// </summary>
        QuerySensorPacket = 142
    };

    public enum SensorPacket
    {
        BumpsWheeldrops = 7,
        BatteryCharge = 25
    }
}
