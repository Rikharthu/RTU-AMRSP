using System;
using Microsoft.SPOT;
using Roomba.Roomba;
using System.Threading;

namespace Roomba.Tasks
{
    public abstract class Task
    {
        protected RoombaController roombaController;
        protected bool stop;

        private Thread workingThread;

        public Task(RoombaController roombaController)
        {
            this.roombaController = roombaController;
            this.stop = true;
        }

        public void Start()
        {
            if (this.stop)
            {
                this.stop = false;
                this.workingThread = new Thread(this.DoWork);
                this.workingThread.Start();
            }
        }

        protected abstract void DoWork();
    }
}
