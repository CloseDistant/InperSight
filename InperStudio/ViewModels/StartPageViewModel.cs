using InperPhotometry;
using InperProtocolStack.UnderUpgrade;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using InperStudioControlLib.Lib.Config;
using Stylet;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace InperStudio.ViewModels
{
    public class StartPageViewModel : Screen
    {
        public string Version { get; set; }
        IWindowManager windowManager;
        CancellationTokenSource tokenStart = new CancellationTokenSource();
        public StartPageViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
            System.Windows.Application.Current.Resources["InperTheme"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(InperConfig.Instance.ThemeColor));
        }
        async Task TaskExecute(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(100);
            }
        }
        protected override void OnViewLoaded()
        {
            SearchDevice();
        }
        public async void SearchAgain()
        {
            (View as StartPageView).loading.Visibility = Visibility.Visible;
            (View as StartPageView).remainder.Text = "The device is being initialized...";
            await Task.Delay(1000);
            SearchDevice();
        }

        private void SearchDevice()
        {
            try
            {
                Version = InperConfig.Instance.Version;
                
                DirectoryInfo root = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "UnderBinBackup"));
                FileInfo[] files = root.GetFiles();
                UnderUpgradeFunc under = new UnderUpgradeFunc();
                if (!under.DeviceIsExist)
                {
                    (View as StartPageView).retry.Visibility = Visibility.Visible;
                    (View as StartPageView).normal.Visibility = Visibility.Collapsed;
                    return;
                }
                _ = View.Dispatcher.BeginInvoke(new Action(async () =>
                {
                    #region 下位机更新
                    if (files.Length > 0)
                    {
                        under.SendFile(files, tokenStart);
                        await TaskExecute(tokenStart.Token);
                        //if (!under.DeviceIsRestart)
                        //{
                        //    MessageBox.Show("Updating successful . Changes will take effect after restarting photometry devices.");
                        //}
                    }
                    else
                    {
                        under.UnderInitJumpUpgrade(tokenStart);
                        await TaskExecute(tokenStart.Token);
                    }
                    await Task.Delay(1000);
                    DeviceHeuristic.Instance.UpdateDeviceList();
                    #endregion
                    if (DeviceHeuristic.Instance.DeviceList.Count < 1)
                    {
                        (View as StartPageView).retry.Visibility = Visibility.Visible;
                        (View as StartPageView).normal.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        await Task.Delay(1000);
                        (View as StartPageView).loading.Visibility = Visibility.Collapsed;
                        (View as StartPageView).remainder.Text = "Initialization completed";
                        InperDeviceHelper.Instance.DeviceSet(DeviceHeuristic.Instance.DeviceList.First());
                        InperDeviceHelper.Instance.DeviceInit();

                        uint SMARTLIGHT_INFO = 0x000000E7;
                        uint DEVICE_INFO_PUB = 0x000000E8;

                        InperDeviceHelper.Instance.device.GetDeviceInfo(SMARTLIGHT_INFO);
                        await Task.Delay(500);
                        InperDeviceHelper.Instance.device.GetDeviceInfo(DEVICE_INFO_PUB);
                        await Task.Delay(500);
                        InperLogExtentHelper.InitLogDatabase();

                        windowManager.ShowWindow(new MainWindowViewModel(windowManager));
                        RequestClose();
                    }
                }));

            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex,this.GetType().Name);
            }
        }
        public void Close()
        {
            this.RequestClose();
            Environment.Exit(0);
        }
    }
}
