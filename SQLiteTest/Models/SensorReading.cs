using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace SQLiteTest.Models
{
    public class SensorReading
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DateTime ReadingDate { get; set; }
        public byte[] ReadingBytes { get; set; }

        public SensorReading()
        {
            ReadingDate = DateTime.Now;
        }

    }
}
