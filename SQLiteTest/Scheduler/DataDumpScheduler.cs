using SQLiteTest.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SQLiteTest.Scheduler
{
    public class DataDumpScheduler : SchedulerBase
    {
        private static DataDumpScheduler _instance;
        public static DataDumpScheduler Instance => _instance ??= new DataDumpScheduler();

        private DataDumpScheduler() { }

        public void CreateDataDumpTask(int intervalMinutes, Action task)
        {
            var interval = TimeSpan.FromMinutes(intervalMinutes);
            CreateScheduleTimer(interval, 0, task);
        }

        void CreateScheduleTimer(TimeSpan interval, int delayMinutes, Action task)
        {
            var now = DateTime.Now;

            var startTime = now.RoundUp(interval);
            startTime.AddMinutes(delayMinutes);

            var timeToGo = startTime - now;

            var timer = new Timer(x =>
            {
                task.Invoke();
            }, null, timeToGo, interval);

            Timers.Add(timer);
        }
    }
}
