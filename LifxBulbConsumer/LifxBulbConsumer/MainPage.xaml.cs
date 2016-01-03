﻿using System;
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
        private readonly List<LampStateConsumer> _lampStateConsumers;
        private readonly AllJoynBusAttachment _lampStateBusAttachment;
        private bool _lampState;

        public MainPage()
        {
            this.InitializeComponent();

            _lampStateConsumers = new List<LampStateConsumer>();
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

            if (joinResult.Status != AllJoynStatus.Ok) return;
            
            // success
            var consumer = (LampStateConsumer) joinResult.Consumer;
            consumer.Signals.LampStateChangedReceived += Signals_LampStateChangedReceived;
            _lampStateConsumers.Add(consumer);

            await SetLampStateAsync();

            System.Diagnostics.Debug.WriteLine("LampStateConsumer successfully added.");
        }

        private async Task SetLampStateAsync()
        {
            foreach (var lampStateConsumer in _lampStateConsumers)
            {
                await lampStateConsumer.SetOnOffAsync(_lampState);
            }            
        }

        private void Signals_LampStateChangedReceived(LampStateSignals sender, LampStateLampStateChangedReceivedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("Lamp state signal received.");
        }

        private async void LampStateToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            _lampState = LampStateToggleSwitch.IsOn;
            await SetLampStateAsync();
        }

        private async void Brightness_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var value = Convert.ToUInt32(uint.MaxValue*(e.NewValue/100.0));

            foreach (var lamp in _lampStateConsumers)
            {
                await lamp.SetBrightnessAsync(value);
            }
        }

        private async void TurnRed_OnClick(object sender, RoutedEventArgs e)
        {
            await SetLampsColorAsync(0x0000000000);
        }

        private async void TurnGreen_OnClick(object sender, RoutedEventArgs e)
        {
            await SetLampsColorAsync(0x55555555);
        }

        private async void TurnBlue_OnClick(object sender, RoutedEventArgs e)
        {
           await SetLampsColorAsync(0xAAAAAAAA);
        }

        private async Task SetLampsColorAsync(uint hue)
        {
            foreach (var lamp in _lampStateConsumers)
            {
                await lamp.SetHueAsync(hue);
            }
        }
    }
}
