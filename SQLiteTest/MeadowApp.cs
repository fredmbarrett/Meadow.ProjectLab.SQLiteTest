using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Gateways.Bluetooth;
using Meadow.Logging;
using Meadow.Peripherals.Leds;
using Meadow.Peripherals.Sensors;
using SQLite;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SQLiteTest.Utils;
using SQLiteTest.Models;
using SQLiteTest.Scheduler;
using System.Collections.Generic;
using SQLiteTest.Controllers;
using System.Linq;

namespace SQLiteTest
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        SQLiteConnection Database;
        IProjectLabHardware projectLab;
        CancellationTokenSource cts = new CancellationTokenSource();

        LedController leds;
        DisplayController display;
        SensorsController sensors;
        DataLoggingScheduler datalogScheduler;
        DataDumpScheduler datadumpScheduler;

        public override Task Run()
        {
            Log.Debug("Entering MeadowApp.Run()");

            try
            {
                sensors = SensorsController.Instance;
                sensors.Initialize(projectLab);

                ConfigureDatabase();
                CreateDataDumpSchedule();
                CreateDataLogSchedules();

                display.AppStatus = "RUN";
                leds.SetColor(Color.Green);
            }
            catch (Exception ex)
            {
                display.AppStatus = "ERROR";
                Log.Error(ex, "MeadowApp.Run() error");
            }

            return base.Run();
        }

        public override Task Initialize()
        {
            projectLab = ProjectLab.Create();

            leds = LedController.Instance;
            leds.StartBlink(Color.DarkBlue);

            display = DisplayController.Instance;
            display.Initialize(projectLab);
            display.AppStatus = "INIT";

            Log.Debug("Leaving MeadowApp.Initialize()");
            return base.Initialize();
        }

        #region Create / Delete Database Methods

        void ConfigureDatabase()
        {
            Log.Debug("Entering ConfigureDatabase()");

            Database = CreateDbConnection();
            Database.CreateTable<SensorReading>(CreateFlags.None);

            Log.Debug("Leaving ConfigureDatabase()");
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

        #endregion Create / Delete Database Methods

        #region Create Data Log Schedule Methods

        /// <summary>
        /// Initialize the data log schedules 
        /// </summary>
        void CreateDataLogSchedules()
        {
            const int SAMPLE_SECONDS = 15;

            Log.Debug("Entering CreateDataLogSchedules()");

            datalogScheduler = DataLoggingScheduler.Instance;
            datalogScheduler.ClearTimers();

            datalogScheduler.CreateDataLogTask(SAMPLE_SECONDS, () =>
            {
                Log.Debug("Entering Data Log Task");
                
                var reading = sensors.TakeSensorReadings();

                if (reading != null)
                {
                    Log.Debug("...Entering Database.Insert");
                    var entity = reading.ToDataEntity();
                    Database.Insert(entity);
                    Log.Debug("...Leaving Database.Insert");
                }

                Log.Debug("Leaving Data Log Task");
            });

            Log.Debug("Leaving CreateDataLogSchedules()");
        }

        #endregion Create Data Log Schedule Methods

        #region Create Data Dump Schedule Methods

        /// <summary>
        /// Initialize the data log schedules 
        /// </summary>
        void CreateDataDumpSchedule()
        {
            const int SAMPLE_MINUTES = 3;

            Log.Debug("Entering CreateDataDumpSchedule()");

            datadumpScheduler = DataDumpScheduler.Instance;
            datadumpScheduler.ClearTimers();

            datadumpScheduler.CreateDataDumpTask(SAMPLE_MINUTES, () =>
            {
                Log.Debug("Entering Data Dump Task");

                var readings = Database.Table<SensorReading>().ToList();

                PrintSensorReadings(readings);
                DeleteSensorReadings(readings);

                Log.Debug("Leaving Data Dump Task");
            });

            Log.Debug("Leaving CreateDataDumpSchedule()");
        }

        private void DeleteSensorReadings(List<SensorReading> readings)
        {
            Log.Debug("Deleting Sensor Readings");
            var ids = string.Join(",", readings.Select(p => p.Id).ToList());
            var sql = $"DELETE FROM SensorReading WHERE Id IN ({ids});";
            int count = 0;
            lock (Database)
            {
                count = Database.Execute(sql);
            }
            Log.Debug($"Deleted {count} Sensor Readings");
        }

        void PrintSensorReadings(List<SensorReading> readings)
        {
            Console.WriteLine($"\r dumping {readings.Count} sensor readings:");

            foreach (var r in readings)
            {
                var reading = new SensorReadingModel(r);
                Console.WriteLine(reading.ToString());
            }

            Console.WriteLine("dump complete.\r");
        }

        #endregion Create Data Dump Schedule Methods


    }
}