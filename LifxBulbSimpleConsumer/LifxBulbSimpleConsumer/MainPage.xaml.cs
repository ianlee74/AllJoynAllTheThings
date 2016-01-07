using System;
using System.Collections.Generic;
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
using org.allseen.LSF.LampState;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LifxBulbSimpleConsumer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly AllJoynBusAttachment _allJoynBusAttachment;
        private LampStateConsumer _consumer;

        public MainPage()
        {
            this.InitializeComponent();

            _allJoynBusAttachment = new AllJoynBusAttachment();
            StartWatcher();
        }

        private void StartWatcher()
        {
            var watcher = new LampStateWatcher(_allJoynBusAttachment);
            watcher.Added += Watcher_Added;
            watcher.Start();
        }

        private async void Watcher_Added(LampStateWatcher sender, AllJoynServiceInfo args)
        {
            var joinResult = await LampStateConsumer.JoinSessionAsync(args, sender);
            if (joinResult.Status != AllJoynStatus.Ok) return;

            _consumer = joinResult.Consumer;

            await _consumer.SetOnOffAsync(false);
        }

        private async void LampStateToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            await _consumer.SetOnOffAsync(LampStateToggleSwitch.IsOn);
        }
    }
}
