using Meadow;
using SQLiteTest.Models;
using SQLiteTest.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLiteTest.Data
{
    /// <summary>
    /// In-Memory implementation of a data store to test
    /// memory leakage without SQLite influence
    /// </summary>
    public class InMemoryDataContext : IDataContext
    {
        List<SensorReading> SensorReadings;

        public InMemoryDataContext()
        {
            ConfigureDatabase();
        }

        /// <summary>
        /// Writes a SensorReading to the database table
        /// </summary>
        /// <param name="reading">SensorReadingModel</param>
        /// <returns>SensorReading entity that was stored</returns>
        public SensorReading InsertSensorReading(SensorReadingModel reading)
        {
            Log.Debug("...Entering InMemoryDataContext.InsertSensorReading");

            var entity = reading.ToDataEntity();

            // bump the row id
            entity.Id = SensorReadings.Count == 0 ? 
                1 :
                SensorReadings.Select(p => p.Id).Max() + 1;

            SensorReadings.Add(entity);

            Log.Debug("...Leaving InMemoryDataContext.InsertSensorReading");
            return entity;
        }

        /// <summary>
        /// Gets list of all sensor readings currently
        /// stored in the database
        /// </summary>
        /// <returns></returns>
        public List<SensorReading> GetSensorReadings()
        {
            return SensorReadings.ToList();
        }

        /// <summary>
        /// Clears all sensor readings from the data table
        /// </summary>
        /// <param name="readings"></param>
        /// <returns>Number of items deleted</returns>
        public int DeleteSensorReadings(List<SensorReading> readings)
        {
            Log.Debug("...Entering InMemoryDataContext.DeleteSensorReadings");

            var ids = readings.Select(p => p.Id).ToList();
            int count = 0;
            lock (SensorReadings)
            {
                count = SensorReadings.RemoveAll(p => ids.Contains(p.Id));
            }

            Log.Debug($"...Leaving InMemoryDataContext.DeleteSensorReadings ({count} deleted)");
            return count;
        }

        void ConfigureDatabase()
        {
            Log.Debug("Entering InMemoryDataContext.ConfigureDatabase()");

            SensorReadings = new List<SensorReading>();

            Log.Debug("Leaving InMemoryDataContext.ConfigureDatabase()");
        }
    }
}
