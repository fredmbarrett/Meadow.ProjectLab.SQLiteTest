using Meadow.Foundation.Leds;
using Meadow.Peripherals.Displays;
using Meadow;
using SQLiteTest.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Units;
using SQLiteTest.Utils;
using Meadow.Gateways.Bluetooth;
using Meadow.Peripherals.Sensors;

namespace SQLiteTest.Controllers
{
    public class SensorsController
    {
        private static readonly Lazy<SensorsController> instance =
            new Lazy<SensorsController>(() => new SensorsController());

        public static SensorsController Instance => instance.Value;

        IProjectLabHardware projectLab;
        LedController leds;
        DisplayController display;

        private SensorsController()
        {
            display = DisplayController.Instance;
        }

        /// <summary>
        /// Determines from the passed data log schedule whether the
        /// sensor being read is a local device sensor or an attached
        /// 'remote' sensor, and performs the readings based on the
        /// data log schedule settings
        /// </summary>
        /// <param name="logAction">DataLogSchedule received from server application</param>
        /// <returns></returns>
        public SensorReadingModel TakeSensorReadings()
        {
            SensorReadingModel reading = null;

            try
            {
                leds.StartFastBlink(Color.Red);
                display.SensorStatus = "READING";
                reading = TakeProjectLabSensorReadings();
                display.SensorStatus = "IDLE";
                leds.SetColor(Color.Green);
            }
            catch(Exception ex)
            {
                Log.Error(ex, "TakeSensorReadings() error");
            }

            return reading;
        }

        /// <summary>
        /// This method determines from the Sensor configuration that
        /// was called out in the data log schedule which channels of
        /// the sensor are active, and prepares a SensorReadingModel
        /// to contain those readings as they are performed.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="sensor"></param>
        /// <returns></returns>
        SensorReadingModel TakeProjectLabSensorReadings()
        {
            // onboard sensor reading
            var reading = new SensorReadingModel();
            var mask = (uint)0x1F;
            var channel = 0;

            do
            {
                if ((mask & 0x00000001) == 1)
                {
                    Resolver.Log.Debug($"...taking sensor reading for project lab sensor channel {channel}...");

                    float value = ReadSensorChannelValue(channel);
                    reading.Readings.Add(value);
                }

                mask >>= 1;
                channel += 1;
            }
            while (mask != 0);

            return reading;
        }

        /// <summary>
        /// Reads from the sensors defined for this device
        /// </summary>
        /// <param name="busAddress"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        float ReadSensorChannelValue(int channel)
        {
            if (AtmosphericConditions is { } conditions)
            {
                return channel switch
                {
                    0 => (Single)(PlabTemperature?.Celsius ?? 0F),
                    1 => (Single)(PlabHumidity?.Percent ?? 0F),
                    2 => (Single)(PlabPressure?.Millibar ?? 0F),
                    3 => (Single)(PlabGasResistance?.Ohms ?? 0F),
                    4 => (Single)(PlabLuminance?.Lux ?? 0F),
                    _ => 0F
                };
            }
            return 0F;

        }

        /// <summary>
        /// Primarily initialize the onboard ProjectLab sensors
        /// </summary>
        /// <param name="projLab"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Initialize(IProjectLabHardware projLab)
        {
            Log.Debug("Entering SensorsController.Initialize()");

            projectLab = projLab ??
                throw new ArgumentNullException(nameof(projLab));

            leds = LedController.Instance;

            var updateSeconds = 15;

            //---- BH1750 Light Sensor
            if (projectLab.LightSensor is { } bh1750)
            {
                Resolver.Log.Info($"Light sensor created");
                bh1750.Updated += Bh1750Updated;
                bh1750.StartUpdating(TimeSpan.FromSeconds(updateSeconds));
            }

            //---- BME688 Atmospheric sensor
            if (projectLab.EnvironmentalSensor is { } bme688)
            {
                Resolver.Log.Info($"Environmental sensor created");
                bme688.Updated += Bme688Updated;

                bme688.GasConversionIsEnabled = true;
                bme688.HeaterIsEnabled = true;
                bme688.ConfigureHeatingProfile(Bme688.HeaterProfileType.Profile1, new Temperature(300), TimeSpan.FromMilliseconds(100), new Temperature(22));
                bme688.HeaterProfile = Bme688.HeaterProfileType.Profile1;

                bme688.StartUpdating(TimeSpan.FromSeconds(updateSeconds));
            }

            Log.Debug("Leaving SensorsController.Initialize()");
        }

        #region Public Properties

        /// <summary>
        /// ProjectLab onboard BME688 atmospheric sensor
        /// </summary>
        public (Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure, Resistance? GasResistance)? AtmosphericConditions
        {
            get => atmosphericConditions;
            set
            {
                atmosphericConditions = value;
            }
        }
        (Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure, Resistance? GasResistance)? atmosphericConditions;

        /// <summary>
        /// Onboard ProjectLab Temperature Sensor
        /// </summary>
        public Temperature? PlabTemperature => AtmosphericConditions?.Temperature;

        /// <summary>
        /// Onboard ProjectLab Humidity Sensor
        /// </summary>
        public RelativeHumidity? PlabHumidity => AtmosphericConditions?.Humidity;

        /// <summary>
        /// Onboard ProjectLab Pressure Sensor
        /// </summary>
        public Pressure? PlabPressure => AtmosphericConditions?.Pressure;

        /// <summary>
        /// Onboard ProjectLab Gas Resistance (VOC) Sensor
        /// </summary>
        public Resistance? PlabGasResistance => AtmosphericConditions?.GasResistance;

        /// <summary>
        /// Onboard ProjectLab Light Sensor (BH1750)
        /// </summary>
        public Illuminance? PlabLuminance
        {
            get => plabLuminance;
            set
            {
                plabLuminance = value;
            }
        }
        Illuminance? plabLuminance;

        #endregion Public Properties

        #region Event Handlers

        private void Bme688Updated(object sender, IChangeResult<(Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure, Resistance? GasResistance)> e)
        {
            AtmosphericConditions = e.New;
        }

        private void Bh1750Updated(object sender, IChangeResult<Illuminance> e)
        {
            PlabLuminance = e.New;
        }

        #endregion Event Handlers
    }
}
