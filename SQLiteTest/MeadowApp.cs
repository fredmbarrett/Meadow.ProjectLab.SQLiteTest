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
using SQLiteTest.Data;

namespace SQLiteTest
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        IProjectLabHardware projectLab;
        IDataContext dataContext;

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
                // Select the database type to create using the enum
                CreateDataContext(DataContextType.SQLite);

                // Initialize the onboard project lab sensors
                sensors = SensorsController.Instance;
                sensors.Initialize(projectLab);

                // Create the schedule to dump data (simulates call-in to servers)
                CreateDataDumpSchedule();

                // Create the schedule to log sensor data
                CreateDataLogSchedules();

                // Display app state and run program
                display.AppStatus = "RUN";
                leds.SetColor(Color.Green);
            }
            catch (Exception ex)
            {
                display.AppStatus = "ERROR";
                Log.Error(ex, "MeadowApp.Run() error");
                throw;
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

        /// <summary>
        /// Creates a data context of DataContextType
        /// for storage and testing
        /// </summary>
        /// <param name="dataContextType">Enum selecting data context type to create</param>
        void CreateDataContext(DataContextType dataContextType)
        {
            // Allows testing SQLite or simple in-memory
            // database for troubleshooting memory/thread issues
            // associated with SQLite and those not related
            if (dataContextType.Equals(DataContextType.SQLite))
                dataContext = new SQLiteDataContext();
            else
                dataContext = new InMemoryDataContext();
        }

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
                    dataContext.InsertSensorReading(reading);
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

                var readings = dataContext.GetSensorReadings();

                PrintSensorReadings(readings);

                dataContext.DeleteSensorReadings(readings);

                Log.Debug("Leaving Data Dump Task");
            });

            Log.Debug("Leaving CreateDataDumpSchedule()");
        }

        /// <summary>
        /// Displays list of collected SensorReadings since last data dump
        /// </summary>
        /// <param name="readings">List of sensor readings to display</param>
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