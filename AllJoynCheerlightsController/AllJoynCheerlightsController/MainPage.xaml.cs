using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.AllJoyn;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using org.allseen.LSF.LampState;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AllJoynCheerlightsController
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // AllJoyn stuff
        private List<LampStateConsumer> _lampStateConsumers;
        private AllJoynBusAttachment _lampStateBusAttachment;


        // Cheerlights stuff
        private readonly Cheerlights _cheerlights;
        private string _currentColor;

        public MainPage()
        {
            this.InitializeComponent();

            _lampStateConsumers = new List<LampStateConsumer>();
            _currentColor = "blue";

            _lampStateBusAttachment = new AllJoynBusAttachment();
            StartWatchers();

            // Start polling the Cheerlights API.
            _cheerlights = new Cheerlights();
            var timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 5);
            timer.Start();
        }

        private void StartWatchers()
        {
            var lampStateWatcher = new LampStateWatcher(_lampStateBusAttachment);
            lampStateWatcher.Added += LampStateWatcher_Added;
            lampStateWatcher.Start();
        }

        private async void LampStateWatcher_Added(LampStateWatcher sender, AllJoynServiceInfo args)
        {
            var joinResult = await LampStateConsumer.JoinSessionAsync(args, sender);

            if (joinResult.Status != AllJoynStatus.Ok) return;

            // success
            _lampStateConsumers.Add(joinResult.Consumer);
            await SetLampColorAsync(joinResult.Consumer);

            System.Diagnostics.Debug.WriteLine("LampStateConsumer successfully added.");
        }

        private async Task SetLampColorAsync(LampStateConsumer consumer)
        {
            var color = _cheerlights.Colors[_currentColor];
            await consumer.SetHueAsync(color.Hue);
            await Task.Delay(200);
            await consumer.SetSaturationAsync(color.Saturation);
            await Task.Delay(200);
        }

        private async void Timer_Tick(object sender, object e)
        {
            var newColor = await _cheerlights.GetCurrentColorAsync();
            if (newColor == _currentColor) return;

            HslColor color;
            if (!_cheerlights.Colors.TryGetValue(newColor, out color))
            {
                Debug.WriteLine(string.Format("An unknown color was detected: {0}", newColor));
                return;
            }
            _currentColor = newColor;

            Debug.WriteLine(string.Format("@cheerlights color is: {0}", _currentColor));

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    cheerlightsColorText.Text = _currentColor;
                    mainGrid.Background = new SolidColorBrush(color.WindowsColor);
                });

            foreach (var lamp in _lampStateConsumers)
            {
                await SetLampColorAsync(lamp);
            }
        }
    }
}
