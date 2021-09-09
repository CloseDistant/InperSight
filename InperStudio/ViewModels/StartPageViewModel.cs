using InperPhotometry;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using InperStudioControlLib.Lib.Config;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace InperStudio.ViewModels
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
            Version = InperConfig.Instance.Version;
            _ = View.Dispatcher.BeginInvoke(new Action(async () =>
                {
                    if (DeviceHeuristic.Instance.DeviceList.Count < 1)
                    {
                        (View as StartPageView).retry.Visibility = System.Windows.Visibility.Visible;
                        (View as StartPageView).normal.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        await Task.Delay(2000);
                        InperDeviceHelper.Instance.DeviceInit(DeviceHeuristic.Instance.DeviceList.First());

                        (View as StartPageView).loading.Visibility = Visibility.Collapsed;
                        (View as StartPageView).remainder.Text = "Initialization completed";
                        await Task.Delay(1000);
                        windowManager.ShowWindow(new MainWindowViewModel(windowManager));
                        RequestClose();
                    }
                }));
        }
        public void SearchAgain()
        {
            try
            {
                DeviceHeuristic.Instance.UpdateDeviceList();
                if (DeviceHeuristic.Instance.DeviceList.Count < 1)
                {
                    MessageBox.Show("Device not found");
                }
                else
                {
                    InperDeviceHelper.Instance.DeviceInit(DeviceHeuristic.Instance.DeviceList.First());

                    (View as StartPageView).loading.Visibility = Visibility.Collapsed;
                    (View as StartPageView).remainder.Text = "Initialization completed";

                    Thread.Sleep(3000);
                    windowManager.ShowWindow(new MainWindowViewModel(windowManager));
                    RequestClose();
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void Close()
        {
            this.RequestClose();
        }
    }
}
