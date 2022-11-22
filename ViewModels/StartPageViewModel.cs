using InperSight.Lib.Device;
using InperSight.Lib.Helper;
using InperSight.Views;
using InperVideo.Camera;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace InperSight.ViewModels
{
    public class StartPageViewModel : Screen
    {
        public string Version { get; set; }
        IWindowManager windowManager;
        public StartPageViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
        }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            FindDevice();
        }
        public void SearchAgain()
        {
            FindDevice();
        }
        private void FindDevice()
        {
            _ = View.Dispatcher.BeginInvoke(new Action(async () =>
            {
                KeyValuePair<int, InperVideo.Interfaces.ICameraInfo>? device = DeviceHelper.GetDeviceHelper().GetMiniscopeCameraInfo();
                if (device != null)
                {
                    InperDeviceHelper.GetInstance().ImageWidth = device.Value.Value.CapabilyItems.First().Size.Width;
                    InperDeviceHelper.GetInstance().ImageHeight = device.Value.Value.CapabilyItems.First().Size.Height;
                    InperDeviceHelper.GetInstance().MiniscopeCameraInit(device.Value.Key, new CameraParamSet(device.Value.Value.CapabilyItems.First().Size, device.Value.Value.CapabilyItems.First().FrameRate, device.Value.Value.CapabilyItems.First().Format));
                    await Task.Delay(1000);
                    (View as StartPageView).loading.Visibility = Visibility.Collapsed;
                    (View as StartPageView).remainder.Text = "Initialization completed";
                    await Task.Delay(100);
                    windowManager.ShowWindow(new MainWindowViewModel(windowManager));
                    RequestClose();
                }
                else
                {
                    (View as StartPageView).retry.Visibility = Visibility.Visible;
                    (View as StartPageView).normal.Visibility = Visibility.Collapsed;
                    return;
                }
            }));
        }
        public void Close()
        {
            this.RequestClose();
            Environment.Exit(0);
        }
    }
}
