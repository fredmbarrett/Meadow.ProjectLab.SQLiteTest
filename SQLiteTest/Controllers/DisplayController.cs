using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using SQLiteTest.Utils;
using System;
using System.Timers;

namespace SQLiteTest.Controllers
{
    public class DisplayController
    {
        private static readonly Lazy<DisplayController> instance =
            new Lazy<DisplayController>(() => new DisplayController());

        public static DisplayController Instance => instance.Value;

        IProjectLabHardware projectLab;
        MicroGraphics _graphics;
        Timer _clockTimer;
        DateTime _startTime;
        bool _isUpdating = false;
        bool _needsUpdate = false;

        private DisplayController() { }

        public void Initialize(IProjectLabHardware projLab)
        {
            Log.Debug("Entering DisplayController.Initialize()");

            projectLab = projLab;

            InitializeDisplay();
            InitializeClockTimer();

            Log.Debug("Leaving DisplayController.Initialize()");
        }

        /// <summary>
        /// Initialize graphics hardware chipset
        /// </summary>
        void InitializeDisplay()
        {
            var display = projectLab.Display;

            _graphics = new MicroGraphics(display)
            {
                IgnoreOutOfBoundsPixels = true,
                Rotation = RotationType._180Degrees,
                CurrentFont = new Font12x20()
            };

            _graphics.Clear(true);

            Update();
        }

        void InitializeClockTimer()
        {
            _startTime = DateTime.Now;
            _clockTimer = new Timer(1000)
            {
                AutoReset = true,
                Enabled = true
            };
            _clockTimer.Elapsed += OnTimerTick;
            _clockTimer.Start();
        }

        public void Update()
        {
            if (_isUpdating)
            {
                _needsUpdate = true;
                return;
            }

            _isUpdating = true;

            Draw();

            _isUpdating = false;

            if (_needsUpdate)
            {
                _needsUpdate = false;
                Update();
            }
        }

        void Draw()
        {
            int lineY = 20, line = 1, offset = 20;

            _graphics.Clear();

            //DrawEnvizorLogo();

            DrawStatus("SQLite Test", "v1.0", WildernessLabsColors.AzureBlue, 5);
            DrawStatus("Time:", DeviceCurrentTime, Color.White, offset + (lineY * line++));
            DrawStatus("Uptime:", Uptime.ToString(@"d\.hh\:mm"), Color.White, offset + (lineY * line++));
            DrawStatus("S/N:", DeviceSerialNumber, Color.White, offset + (lineY * line++));
            DrawStatus("DbType:", DatabaseType, Color.White, offset + (lineY * line++));
            DrawStatus("MemUsage:", TotalMemUsage, Color.White, offset + (lineY * line++));
            DrawStatus($"SenStatus:", $"{SensorStatus}", Color.White, offset + (lineY * line++));
            DrawStatus($"AppStatus:", $"{AppStatus}", Color.White, offset + (lineY * line++));

            _graphics.Show();
        }

        void DrawStatus(string label, string value, Color color, int yPosition)
        {
            _graphics.DrawText(x: 5, y: yPosition, label, color: color);
            _graphics.DrawText(x: 240, y: yPosition, value, alignmentH: HorizontalAlignment.Right, color: color);
        }

        /// <summary>
        /// Device Serial Number (from CPU)
        /// </summary>
        public string DeviceSerialNumber => Resolver.Device.Information.ProcessorSerialNumber;

        /// <summary>
        /// Log a status message to the display
        /// </summary>
        public string AppStatus
        {
            get => appStatus;
            set
            {
                appStatus = value;
            }
        }
        string appStatus = "";

        /// <summary>
        /// Sensor polling status
        /// </summary>
        public string SensorStatus
        {
            get => sensorStatus;
            set
            {
                sensorStatus = value;
            }
        }
        string sensorStatus = "";

        /// <summary>
        /// Which data context type is in use
        /// </summary>
        public string DatabaseType
        {
            get => databaseType;
            set
            {
                databaseType = value;
            }
        }
        string databaseType = "";

        /// <summary>
        /// Display current device time
        /// </summary>
        public string DeviceCurrentTime
        {
            get => deviceTime;
            set
            {
                deviceTime = value;
            }
        }
        string deviceTime = "00:00:00 AM";

        public string TotalMemUsage
        {
            get => memUsage;
            set
            {
                memUsage = value;
            }
        }
        string memUsage;

        public TimeSpan Uptime
        {
            get
            {
                return DateTime.Now - _startTime;
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            DeviceCurrentTime = DateTime.Now.ToString("hh:mm:ss tt");
            Update();
        }

    }
}
