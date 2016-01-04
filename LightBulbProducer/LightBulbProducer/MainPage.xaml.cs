using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.AllJoyn;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using GHIElectronics.UWP.GadgeteerCore.NativeInterfaces;
using org.allseen.LSF.LampState;
using GT = GHIElectronics.UWP.GadgeteerCore;
using GTMB = GHIElectronics.UWP.Gadgeteer.Mainboards;
using GTMO = GHIElectronics.UWP.Gadgeteer.Modules;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LightBulbProducer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private GTMB.FEZCream _mainboard;
        private GTMO.Extender _extender;

        private GT.SocketInterfaces.DigitalIO _colorPin0;
        private GT.SocketInterfaces.DigitalIO _colorPin1;
        private GT.SocketInterfaces.DigitalIO _colorPin2;
        private GT.SocketInterfaces.DigitalIO _colorPin3;

        private AllJoynBusAttachment _busAttachment;
        private LampStateProducer
        public MainPage()
        {
            this.InitializeComponent();
            Setup();
            this.Loaded += OnLoaded;

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(3000);
            timer.Tick += OnTick;
            timer.Start();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
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

        private uint _colorCode = 0;

        private void SetColor(uint colorCode)
        {
            Debug.WriteLine("Setting color to: " + colorCode);
            _colorPin0.Write((colorCode & 1) == 1);
            _colorPin1.Write((colorCode & 2) == 2);
            _colorPin2.Write((colorCode & 4) == 4);
            _colorPin3.Write((colorCode & 8) == 8);
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
    }
}
