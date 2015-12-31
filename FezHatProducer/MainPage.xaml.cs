using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.AllJoyn;
using Windows.Foundation;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using com.ianlee.FezHat;
using GHIElectronics.UWP.Shields;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FezHatProducer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IFezHatService
    {
        private bool _redLedState = false;
        private FEZHAT _fezhat;
        private readonly bool _isRunningOnPi;
        private AllJoynBusAttachment _busAttachment;
        private com.ianlee.FezHat.FezHatProducer _producer;
        private DispatcherTimer _buttonMonitor;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;

            var ai = AnalyticsInfo.VersionInfo;
            Debug.WriteLine(string.Format("Device Family = {0}", ai.DeviceFamily));
            _isRunningOnPi = ai.DeviceFamily == "Windows.IoT";
            //_isRunningOnPi = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Devices.Gpio.GpioController");             BUG? Always returns true regardless of the device.
            if (_isRunningOnPi)
            {
                Task.Run(async () => _fezhat = await FEZHAT.CreateAsync());
                _buttonMonitor = new DispatcherTimer();
                _buttonMonitor.Interval = new TimeSpan(TimeSpan.TicksPerSecond / 4);
                _buttonMonitor.Tick += ButtonMonitorOnTick;
                _buttonMonitor.Start();
            }
        }

        private void ButtonMonitorOnTick(object sender, object o)
        {
            if (_fezhat != null && _fezhat.IsDIO18Pressed())
            {
                Debug.WriteLine("Button DIO18 was pressed!");
                _producer.Signals.ButtonDio18Pressed();
            }

        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _busAttachment = new AllJoynBusAttachment();
            PopulateAboutData(_busAttachment);
            _producer = new com.ianlee.FezHat.FezHatProducer(_busAttachment) { Service = this };
            _producer.Start();
        }

        private static void PopulateAboutData(AllJoynBusAttachment busAttachment)
        {
            busAttachment.AboutData.DateOfManufacture = DateTime.Now;
            busAttachment.AboutData.DefaultAppName = "Ian's FEZ HAT";
            busAttachment.AboutData.DefaultDescription = "Controls and monitors the devices on the FEZ HAT.";
            busAttachment.AboutData.ModelNumber = "Model #1";
            busAttachment.AboutData.SoftwareVersion = "1.0";
            busAttachment.AboutData.SupportUrl = new Uri("http://ianlee.info");
        }

        public IAsyncOperation<FezHatSetRedLedStateResult> SetRedLedStateAsync(AllJoynMessageInfo info, bool interfaceMemberOn)
        {
            return (
                Task.Run(() =>
                {
                    SetRedLedState(interfaceMemberOn);
                    return FezHatSetRedLedStateResult.CreateSuccessResult();
                }).AsAsyncOperation());
        }

        private void SetRedLedState(bool state)
        {
            _redLedState = state;
            if (_isRunningOnPi)
            {
                _fezhat.DIO24On = _redLedState;

                // Go ahead and light up D2 & D3 too to make it more visible for the crowd.
                var redColor = (byte)(_redLedState ? 255 : 0);
                SetRgbLedD2Color(redColor, 0, 0);
                SetRgbLedD3Color(redColor, 0, 0);
            }
        }

        public IAsyncOperation<FezHatGetRedLedStateResult> GetRedLedStateAsync(AllJoynMessageInfo info)
        {
            return (
                Task.Run(() =>
                {
                    return FezHatGetRedLedStateResult.CreateSuccessResult(_redLedState);
                }).AsAsyncOperation());
        }

        public IAsyncOperation<FezHatSetRgbLedD2ColorResult> SetRgbLedD2ColorAsync(AllJoynMessageInfo info, bool interfaceMemberOn, byte interfaceMemberRed, byte interfaceMemberGreen, byte interfaceMemberBlue)
        {
            return (
                Task.Run(() =>
                {
                    if (_isRunningOnPi)
                    {
                        SetRgbLedD2Color(interfaceMemberRed, interfaceMemberGreen, interfaceMemberBlue);
                    }
                    return FezHatSetRgbLedD2ColorResult.CreateSuccessResult();
                }).AsAsyncOperation());
        }

        private void SetRgbLedD2Color(byte red, byte green, byte blue)
        {
            _fezhat.D2.Color = new FEZHAT.Color(red, green, blue);
        }

        public IAsyncOperation<FezHatSetRgbLedD3ColorResult> SetRgbLedD3ColorAsync(AllJoynMessageInfo info, bool interfaceMemberOn, byte interfaceMemberRed, byte interfaceMemberGreen, byte interfaceMemberBlue)
        {
            return (
                Task.Run(() =>
                {
                    if (_isRunningOnPi)
                    {
                        SetRgbLedD3Color(interfaceMemberRed, interfaceMemberGreen, interfaceMemberBlue);
                    }
                    return FezHatSetRgbLedD3ColorResult.CreateSuccessResult();
                }).AsAsyncOperation());
        }

        private void SetRgbLedD3Color(byte red, byte green, byte blue)
        {
            _fezhat.D3.Color = new FEZHAT.Color(red, green, blue);
        }

        public IAsyncOperation<FezHatGetLightSensorValueResult> GetLightSensorValueAsync(AllJoynMessageInfo info)
        {
            return (
                Task.Run(() =>
                {
                    var lightLevel = 0.0;
                    if (_isRunningOnPi && _fezhat != null)
                    {
                        lightLevel = _fezhat.GetLightLevel();
                    }
                    return FezHatGetLightSensorValueResult.CreateSuccessResult(lightLevel);
                }).AsAsyncOperation());
        }

        public IAsyncOperation<FezHatGetTemperatureSensorValueResult> GetTemperatureSensorValueAsync(AllJoynMessageInfo info)
        {
            return (
                Task.Run(() =>
                {
                    var tempC = 0.0;
                    if (_isRunningOnPi && _fezhat != null)
                    {
                        tempC = _fezhat.GetTemperature();
                    }
                    return FezHatGetTemperatureSensorValueResult.CreateSuccessResult(tempC, tempC * 1.8 + 32);
                }).AsAsyncOperation());
        }
    }
}
