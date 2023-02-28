using SQLiteTest.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace SQLiteTest.Models
{
    public class SensorReadingModel
    {
        public int Id { get; set; }
        public DateTime ReadingDate { get; set; }
        public List<float> Readings { get; set; }

        public SensorReadingModel()
        {
            ReadingDate = DateTime.Now;
            Readings = new List<float>();
        }

        public SensorReadingModel(SensorReading entity) : this()
        {
            Id = entity.Id;
            ReadingDate = entity.ReadingDate;

            var readingBytes = entity.ReadingBytes ??= new byte[0];
            var reader = new PayloadReader(readingBytes);
            var count = readingBytes.Length / sizeof(float);

            for (var i = 0; i < count; i++)
                Readings.Add(reader.ReadSingle());
        }

        /// <summary>
        /// Converts the SensorReadingModel into a SensorReading data entity
        /// </summary>
        /// <returns></returns>
        public SensorReading ToDataEntity()
        {
            var entity = new SensorReading
            {
                ReadingDate = ReadingDate,
                ReadingBytes = new byte[Readings.Count * sizeof(float)]
            };

            var writer = new PayloadWriter(entity.ReadingBytes);
            foreach (var reading in Readings)
                writer.WriteSingle(reading);

            return entity;
        }

        public override string ToString()
        {
            var sb = new StringBuilder()
                .Append($"Id: {Id}; ")
                .Append($"Date: {ReadingDate.ToString("f")}; ")
                .Append($"ChannelData: ");

            int channel = 0;
            Readings.ForEach(p => sb.Append($"{channel++}: {p}; "));
            return sb.ToString();
        }
    }
}
