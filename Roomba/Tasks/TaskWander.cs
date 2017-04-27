using System;
using Microsoft.SPOT;
using Roomba.Roomba;

namespace Roomba.Tasks
{
    class TaskWander : Task
    {
        public TaskWander(RoombaController roombaController) : base(roombaController)
        {
        }

        protected override void DoWork()
        {
            throw new NotImplementedException();
        }
    }
}
