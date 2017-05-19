using System;
using Microsoft.SPOT;
using Roomba.Roomba;
using System.Threading;
using Roomba.Roomba.Sensors;

namespace Roomba.Tasks
{
    public class TaskWander : Task
    {

        public TaskWander(RoombaController roombaController) : base(roombaController)
        {
        }

        protected override void DoWork()
        {
            Random rnd = new Random();
            while (!stop)
            {
                if (this.roombaController.Sensors.IsBump)
                {
                    int randomDelay = (int)(rnd.NextDouble() * 300) + 300;

                    switch (roombaController.Sensors.BumpCode)
                    {
                        case SensorController.CODE_BUMP_LEFT:
                            // bump on left => turn right
                            roombaController.CmdExecutor.DriveStraight(-600);
                            Thread.Sleep(100);
                            roombaController.CmdExecutor.TurnRight(300);
                            Thread.Sleep(randomDelay);
                            break;
                        case SensorController.CODE_BUMP_RIGHT:
                            // bump on right => turn left
                            roombaController.CmdExecutor.DriveStraight(-600);
                            Thread.Sleep(100);
                            roombaController.CmdExecutor.TurnLeft(300);
                            Thread.Sleep(randomDelay);
                            break;
                        case SensorController.CODE_BUMP_BOTH:
                            // both sides => 360 deg
                            roombaController.CmdExecutor.DriveStraight(-600);
                            Thread.Sleep(100);
                            // TODO ADJUST TIMING
                            roombaController.CmdExecutor.TurnLeft(300);
                            Thread.Sleep(600);
                            roombaController.CmdExecutor.Stop();
                            break;
                    }
                }
                else
                {
                    roombaController.CmdExecutor.DriveStraight(100);
                    Thread.Sleep(50);
                }
            }
            roombaController.CmdExecutor.Stop();
        }    
    }
}