﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Home_Automation__RPi2_WinIoT_.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Page_Devices : Page
    {
        private class DeviceInfo : Library.Core.Device
        {
            public string Tooltip { get; set; }
            public SolidColorBrush StatusColor { get; set; }
            public int SlaveAddress; // <-- need to use to avoid unknown error in-place of I2C_Slave_Address.
        }

        Library.Core.Room SelectedRoom;

        public Page_Devices()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var RawData = e.Parameter as object[];
            SelectedRoom = (Library.Core.Room)RawData[0];

            UpdateTexts();
            LoadDevices();


        }

        private void UpdateTexts()
        {
            Lbl_RoomName.Text = SelectedRoom.Name;
        }

        private void LoadDevices()
        {
            if (LV_Devices.Items != null && LV_Devices.Items.Count > 0)
            {
                LV_Devices.Items.Clear();
            }
            foreach (var Device in SelectedRoom.Devices)
            {
                DeviceInfo _DevInfo = new DeviceInfo();
                _DevInfo.Id = Device.Id;
                _DevInfo.ImagePath = Device.ImagePath;
                _DevInfo.Name = Device.Name;
                _DevInfo.Pin = Device.Pin;
                _DevInfo.SlaveAddress = Device.I2C_Slave_Address;
                //._DevInfo.IsOn = (Device.Status==Library.Core.Device.StatusEnum.On)?true:false;

                // Invert Logic due to delay in task completion (TurnOn and TurnOff functions)
                if (Device.Status == Library.Core.Device.StatusEnum.On)
                {

                    _DevInfo.StatusColor = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 50, 255, 50));
                }
                else
                {
                    _DevInfo.StatusColor = new SolidColorBrush(Windows.UI.Color.FromArgb(150, 128, 128, 128));
                }
                LV_Devices.Items.Add(_DevInfo);
            }
        }

        private void LV_Devices_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            DeviceInfo _SelDevInfo = (DeviceInfo)LV_Devices.SelectedItem;
            Library.Core.Device SelectedDevice = new Library.Core.Device();

            foreach (var Device in SelectedRoom.Devices)
            {
                if (_SelDevInfo.Id == Device.Id)
                {
                    SelectedDevice = Device;
                    break;
                }
            }


            if (SelectedDevice.Status == Library.Core.Device.StatusEnum.Off)
            {
                Task.Factory.StartNew(() =>
                {
                    SelectedDevice.TurnOn();
                }).Wait(1000);
                SelectedDevice.Status = Library.Core.Device.StatusEnum.On;
            }
            else
            {
                Task.Factory.StartNew(() =>
                {
                    SelectedDevice.TurnOff();
                }).Wait(1000);
                SelectedDevice.Status = Library.Core.Device.StatusEnum.Off;
            }

            LoadDevices();
        }
    }
}
