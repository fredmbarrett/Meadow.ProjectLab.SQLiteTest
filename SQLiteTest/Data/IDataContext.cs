using SQLiteTest.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SQLiteTest.Data
{
    public interface IDataContext
    {
        /// <summary>
        /// Accepts a SensorReadingModel and converts it to
        /// a SensorReading entity, then writes it to the database;
        /// </summary>
        /// <param name="reading">SensorReadingModel containing sensor readings</param>
        /// <returns>Entity that was saved</returns>
        SensorReading InsertSensorReading(SensorReadingModel reading);

        /// <summary>
        /// Gets all SensorReadings from the database table
        /// </summary>
        /// <returns>List of stored SensorReadings</returns>
        List<SensorReading> GetSensorReadings();

        /// <summary>
        /// Deletes all sensor readings contained in the passed list
        /// </summary>
        /// <param name="readings">List of SensorReadings to delete from the database</param>
        /// <returns>Count of rows deleted</returns>
        int DeleteSensorReadings(List<SensorReading> readings);
    }
}
