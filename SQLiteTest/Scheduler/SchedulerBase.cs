using System.Collections.Generic;
using System.Threading;

namespace SQLiteTest.Scheduler
{
    public abstract class SchedulerBase
    {
        public List<Timer> Timers
        {
            get => timers ??= new List<Timer>();
            protected set
            {
                timers = value;
            }
        }
        private List<Timer> timers = new List<Timer>();

        /// <summary>
        /// Dispose all active timers and reset the collection
        /// </summary>
        public void ClearTimers()
        {
            foreach (var t in Timers)
            {
                t.Dispose();
            }

            timers = new List<Timer>();
        }
    }
}
