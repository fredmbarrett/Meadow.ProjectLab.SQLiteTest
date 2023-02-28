using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace SQLiteTest.Models
{
    public class DataLogSchedule
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ActionDays { get; set; }
        public int ActionType { get; set; }
        public int ActionHour { get; set; }
        public int ActionMinute { get; set; }
        public int ActionSecond { get; set; }
        public int ActionParam { get; set; }
    }
}
