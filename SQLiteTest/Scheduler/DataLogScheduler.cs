using SQLiteTest.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SQLiteTest.Scheduler
{
    public class DataLoggingScheduler : SchedulerBase
    {
        private static DataLoggingScheduler _instance;
        public static DataLoggingScheduler Instance => _instance ??= new DataLoggingScheduler();

        public void CreateDataLogTask(int intervalMinutes, Action task)
        {
            var interval = TimeSpan.FromSeconds(intervalMinutes);
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
