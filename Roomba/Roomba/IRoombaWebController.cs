using System;
using Microsoft.SPOT;

namespace Roomba.Roomba
{
    public interface IRoombaWebController
    {
        float GetChargeLevel();
        int GetStatus();
        void OnCommandDispatched(int code);
    }
}
