using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.AllJoyn;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using org.allseen.LSF.LampState;
using GT = GHIElectronics.UWP.GadgeteerCore;
using GTMB = GHIElectronics.UWP.Gadgeteer.Mainboards;
using GTMO = GHIElectronics.UWP.Gadgeteer.Modules;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LedStripProducer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, ILampStateService
    {
        private GTMB.FEZCream _mainboard;
        private GTMO.Extender _extender;

        private GT.SocketInterfaces.DigitalIO _colorPin0;
        private GT.SocketInterfaces.DigitalIO _colorPin1;
        private GT.SocketInterfaces.DigitalIO _colorPin2;
        private GT.SocketInterfaces.DigitalIO _colorPin3;

        private AllJoynBusAttachment _busAttachment;
        private LampStateProducer _producer;

        private bool _isOn;
        private uint _colorCode;
        private uint _hue;

        public MainPage()
        {
            this.InitializeComponent();
            Setup();
            this.Loaded += OnLoaded;

            _isOn = true;
            _colorCode = 0x1; // default to red

            // Test code.
            //var timer = new DispatcherTimer();
            //timer.Interval = TimeSpan.FromMilliseconds(3000);
            //timer.Tick += OnTick;
            //timer.Start();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _busAttachment = new AllJoynBusAttachment();
            PopulateAboutData(_busAttachment);
            _producer = new LampStateProducer(_busAttachment) { Service = this };
            _producer.Start();
        }

        private void PopulateAboutData(AllJoynBusAttachment busAttachment)
        {
            busAttachment.AboutData.DateOfManufacture = DateTime.Now;
            busAttachment.AboutData.DefaultAppName = "FEZ Cream LED Controller";
            busAttachment.AboutData.DefaultDescription = "Controls a WS2812 LED strip.";
            busAttachment.AboutData.SupportUrl = new Uri("http://ianlee.info");
        }

        private void OnTick(object sender, object e)
        {
            if (_colorCode == 11)
            {
                _colorCode = 1;
            }
            else
            {
                _colorCode++;
            }

            SetColor(_colorCode);
        }

        private void SetColor(uint colorCode)
        {
            if (_isOn)
            {
                Debug.WriteLine("Setting color to: " + colorCode);
                _colorPin0.Write((colorCode & 1) == 1);
                _colorPin1.Write((colorCode & 2) == 2);
                _colorPin2.Write((colorCode & 4) == 4);
                _colorPin3.Write((colorCode & 8) == 8);
            }
            else
            {
                Debug.WriteLine("Turning off LED strip.");
                _colorPin0.Write(false);
                _colorPin1.Write(false);
                _colorPin2.Write(false);
                _colorPin3.Write(false);
            }
            _colorCode = colorCode;
        }

        private async void Setup()
        {
            _mainboard = await GT.Module.CreateAsync<GTMB.FEZCream>();
            _extender = await GT.Module.CreateAsync<GTMO.Extender>(_mainboard.GetProvidedSocket(8));

            _colorPin0 = await _extender.CreateDigitalIOAsync(GT.SocketPinNumber.Three, false);
            _colorPin1 = await _extender.CreateDigitalIOAsync(GT.SocketPinNumber.Four, false);
            _colorPin2 = await _extender.CreateDigitalIOAsync(GT.SocketPinNumber.Five, false);
            _colorPin3 = await _extender.CreateDigitalIOAsync(GT.SocketPinNumber.Six, false);
        }

        public IAsyncOperation<LampStateTransitionLampStateResult> TransitionLampStateAsync(AllJoynMessageInfo info, ulong interfaceMemberTimestamp,
            IReadOnlyDictionary<string, object> interfaceMemberNewState, uint interfaceMemberTransitionPeriod)
        {
            return (Task.Run(() => LampStateTransitionLampStateResult.CreateSuccessResult(1)).AsAsyncOperation());
        }

        public IAsyncOperation<LampStateApplyPulseEffectResult> ApplyPulseEffectAsync(AllJoynMessageInfo info, IReadOnlyDictionary<string, object> interfaceMemberFromState,
            IReadOnlyDictionary<string, object> interfaceMemberToState, uint interfaceMemberPeriod, uint interfaceMemberDuration,
            uint interfaceMemberNumPulses, ulong interfaceMemberTimestamp)
        {
            return (Task.Run(() => LampStateApplyPulseEffectResult.CreateSuccessResult(1)).AsAsyncOperation());
        }

        public IAsyncOperation<LampStateGetVersionResult> GetVersionAsync(AllJoynMessageInfo info)
        {
            return (Task.Run(() => LampStateGetVersionResult.CreateSuccessResult(1)).AsAsyncOperation());
        }

        public IAsyncOperation<LampStateGetOnOffResult> GetOnOffAsync(AllJoynMessageInfo info)
        {
            return (Task.Run(() => LampStateGetOnOffResult.CreateSuccessResult(_isOn)).AsAsyncOperation());
        }

        public IAsyncOperation<LampStateSetOnOffResult> SetOnOffAsync(AllJoynMessageInfo info, bool value)
        {
            return (
                Task.Run(() =>
                {
                    _isOn = value;
                    SetColor(_colorCode);
                    return LampStateSetOnOffResult.CreateSuccessResult();
                }).AsAsyncOperation());
        }

        public IAsyncOperation<LampStateGetHueResult> GetHueAsync(AllJoynMessageInfo info)
        {
            return (Task.Run(() => LampStateGetHueResult.CreateSuccessResult(_hue)).AsAsyncOperation());
        }

        public IAsyncOperation<LampStateSetHueResult> SetHueAsync(AllJoynMessageInfo info, uint value)
        {
            return (
                Task.Run(() =>
                {
                    _hue = value;
                    switch (_hue)
                    {
                        case 0x00000000:    // red
                            SetColor(1);
                            break;
                        case 0x55555555:    // green
                            SetColor(2);
                            break;
                        case 0xAAAAAAAA:    // blue
                            SetColor(3);
                            break;
                        case 0x80000000:    // cyan
                            SetColor(4);
                            break;
                        case 0x78E37900:    // white
                            SetColor(5);
                            break;
                        case 0x2B602B80:    // oldlace
                            SetColor(6);
                            break;
                        case 0xBFFFC000:    // purple
                            SetColor(7);
                            break;
                        case 0xDC70DC00:    // magenta
                            SetColor(8);
                            break;
                        case 0x349F3480:    // yellow
                            SetColor(9);
                            break;
                        case 0x238E2380:    // orange
                            SetColor(10);
                            break;
                        case 0xD554D500:    // pink
                            SetColor(11);
                            break;
                        default:
                            SetColor(0);
                            break;
                    }
                    SetColor(_colorCode);
                    return LampStateSetHueResult.CreateSuccessResult();
                }).AsAsyncOperation());
        }

        public IAsyncOperation<LampStateGetSaturationResult> GetSaturationAsync(AllJoynMessageInfo info)
        {
            return (Task.Run(() => LampStateGetSaturationResult.CreateSuccessResult(0xFFFFFFFF)).AsAsyncOperation());
        }

        public IAsyncOperation<LampStateSetSaturationResult> SetSaturationAsync(AllJoynMessageInfo info, uint value)
        {
            return (Task.Run(() => LampStateSetSaturationResult.CreateSuccessResult()).AsAsyncOperation());
        }

        public IAsyncOperation<LampStateGetColorTempResult> GetColorTempAsync(AllJoynMessageInfo info)
        {
            return (Task.Run(() => LampStateGetColorTempResult.CreateSuccessResult(0xFFFFFFFF)).AsAsyncOperation());
        }

        public IAsyncOperation<LampStateSetColorTempResult> SetColorTempAsync(AllJoynMessageInfo info, uint value)
        {
            return (Task.Run(() => LampStateSetColorTempResult.CreateSuccessResult()).AsAsyncOperation());
        }

        public IAsyncOperation<LampStateGetBrightnessResult> GetBrightnessAsync(AllJoynMessageInfo info)
        {
            return (Task.Run(() => LampStateGetBrightnessResult.CreateSuccessResult(0xFFFFFFFF)).AsAsyncOperation());
        }

        public IAsyncOperation<LampStateSetBrightnessResult> SetBrightnessAsync(AllJoynMessageInfo info, uint value)
        {
            return (Task.Run(() => LampStateSetBrightnessResult.CreateSuccessResult()).AsAsyncOperation());
        }
    }
}
