using System;
using Microsoft.SPOT;

namespace Roomba.Networking.RemoteCommands
{
    /// <summary>
    /// Reprsente komandas no lietotaja saskarsnes
    /// Komanda ir 9 baitu wirkne, kur baiti:
    /// 0 -> Komandas identifikators
    /// 1..4 -> (int) Komandas pirmais parametrs
    /// 5..8 -> (int) Komandas otrias parameters
    /// </summary>
    public class RemoteCommand
    {
        // 0
        public RemoteCommandType CommandType
        {
            get; set;
        }

        // 1..4
        public int FirstParam { get; set; }

        // 5..8
        public int SecondParam { get; set; }

        public RemoteCommand() { }

        public RemoteCommand(RemoteCommandType commandType, int firstParam, int secondParam)
        {
            this.CommandType = commandType;
            this.FirstParam = firstParam;
            this.SecondParam = secondParam;
        }
    }
}
