using Meadow;
using SQLite;
using SQLiteTest.Models;
using SQLiteTest.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SQLiteTest.Data
{
    /// <summary>
    /// SQLite data store to test memory leaks and
    /// threading issues related to SQLite
    /// </summary>
    public class SQLiteDataContext : IDataContext
    {
        SQLiteConnection Database;

        public SQLiteDataContext()
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
            Log.Debug("...Entering SQLiteDataContext.InsertSensorReading");

            var entity = reading.ToDataEntity();
            Database.Insert(entity);

            Log.Debug("...Leaving SQLiteDataContext.InsertSensorReading");
            return entity;
        }

        /// <summary>
        /// Gets list of all sensor readings currently
        /// stored in the database
        /// </summary>
        /// <returns></returns>
        public List<SensorReading> GetSensorReadings()
        {
            return Database.Table<SensorReading>().ToList();
        }

        /// <summary>
        /// Clears all sensor readings from the data table
        /// </summary>
        /// <param name="readings"></param>
        /// <returns>Number of items deleted</returns>
        public int DeleteSensorReadings(List<SensorReading> readings)
        {
            Log.Debug("...Entering SQLiteDataContext.DeleteSensorReadings");

            var ids = string.Join(",", readings.Select(p => p.Id).ToList());
            var sql = $"DELETE FROM SensorReading WHERE Id IN ({ids});";
            int count = 0;
            lock (Database)
            {
                count = Database.Execute(sql);
            }

            Log.Debug($"...Leaving SQLiteDataContext.DeleteSensorReadings ({count} deleted)");
            return count;
        }

        #region Create Database Methods

        /// <summary>
        /// Establishes the SQLite database and
        /// creates a table to store SensorReading objects
        /// </summary>
        void ConfigureDatabase()
        {
            Log.Debug("Entering SQLiteDataContext.ConfigureDatabase()");

            Database = CreateDbConnection();
            Database.CreateTable<SensorReading>(CreateFlags.None);

            Log.Debug("Leaving SQLiteDataContext.ConfigureDatabase()");
        }

        /// <summary>
        /// Creates an data connection to SQLite
        /// Advanced settings assist in controlling thread concurrency 
        /// (use at your own risk :-)
        /// </summary>
        SQLiteConnection CreateDbConnection()
        {
            const bool USE_ADVANCED_SETTINGS = false;

            SQLiteConnection cn;
            string dbPath = GetDatabaseFilename();

            if (USE_ADVANCED_SETTINGS)
            {
                // set for multi-threading
                SQLite3.Config(SQLite3.ConfigOption.Serialized);

                var flags = SQLiteOpenFlags.Create |
                    SQLiteOpenFlags.ReadWrite |
                    SQLiteOpenFlags.FullMutex;

                cn = new SQLiteConnection(dbPath, flags);
            }
            else
            {
                cn = new SQLiteConnection(dbPath);
            }

            return cn;
        }

        /// <summary>
        /// Gets the database filename from the app settings
        /// and creates the path information that points to 
        /// the Meadow /data folder
        /// </summary>
        /// <returns></returns>
        string GetDatabaseFilename()
        {
            var dbName = "SqliteData.db";
            var dbPath = Path.Combine(MeadowOS.FileSystem.DataDirectory, dbName);
            Resolver.Log.Debug($"...database filename is {dbPath}");
            return dbPath;
        }

        #endregion Create Database Methods

    }
}
