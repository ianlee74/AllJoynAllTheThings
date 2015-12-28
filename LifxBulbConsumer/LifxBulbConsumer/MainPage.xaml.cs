using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.AllJoyn;
using Windows.UI.Notifications;
using org.allseen.LSF.LampState;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LifxBulbConsumer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private LampStateConsumer _lampStateConsumer;
        private AllJoynBusAttachment _lampStateBusAttachment;
        private bool _lampState;

        public MainPage()
        {
            this.InitializeComponent();

            _lampStateBusAttachment = new AllJoynBusAttachment();
            _lampState = false;
            StartWatchers();
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

            if (joinResult.Status == AllJoynStatus.Ok)
            {
                // success
                _lampStateConsumer = joinResult.Consumer;
                _lampStateConsumer.Signals.LampStateChangedReceived += Signals_LampStateChangedReceived;

                await SetLampStateAsync();

                System.Diagnostics.Debug.WriteLine("LampStateConsumer successfully added.");
            }
            else
            {
                // do nothing
            }
        }

        private async Task SetLampStateAsync()
        {
            await _lampStateConsumer.SetOnOffAsync(_lampState);
        }

        private void Signals_LampStateChangedReceived(LampStateSignals sender, LampStateLampStateChangedReceivedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("Lamp state signal received.");

            // Show UI Toast
            var toastTemplate = ToastTemplateType.ToastText02;
            var toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

            // Populate UI Toast
            var toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode("Lamp State Changed"));

            // Create and Send UI Toast
            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private async void LampStateToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            _lampState = LampStateToggleSwitch.IsOn;
            await SetLampStateAsync();
        }
    }
}
