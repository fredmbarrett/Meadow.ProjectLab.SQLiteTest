using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Peripherals.Leds;
using System;

namespace SQLiteTest.Controllers
{
    public class LedController
    {
        private static readonly Lazy<LedController> instance =
            new Lazy<LedController>(() => new LedController());

        public static LedController Instance => instance.Value;

        readonly RgbPwmLed _featherLed;
        readonly F7FeatherV2 _device;

        private LedController()
        {
            Resolver.Log.Debug("LedController initializing...");

            _device = MeadowApp.Device;

            _featherLed = new RgbPwmLed(device: _device,
                redPwmPin: _device.Pins.OnboardLedRed,
                greenPwmPin: _device.Pins.OnboardLedGreen,
                bluePwmPin: _device.Pins.OnboardLedBlue,
                CommonType.CommonAnode);

            Resolver.Log.Debug("LedController initialization complete.");
        }

        public void SetColor(Color color)
        {
            _featherLed.Stop();
            _featherLed.SetColor(color);
        }

        public void StartBlink(Color color)
        {
            _featherLed.StartBlink(color);
        }

        public void StartFastBlink(Color color)
        {
            TimeSpan duration = TimeSpan.FromMilliseconds(500);
            _featherLed.StartBlink(color, duration, duration);
        }

        public void StopBlink()
        {
            _featherLed.Stop();
        }
    }
}
