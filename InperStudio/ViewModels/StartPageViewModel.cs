using InperPhotometry;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using InperStudioControlLib.Lib.Config;
using Stylet;
using System;
using System.Collections.Generic;
using System.IO;
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

            #region 下位机更新
            // 查找under Bin 文件夹 是否存在文件
            string path = Environment.CurrentDirectory + "/UnderBin";
            if (Directory.Exists(path))
            {
                DirectoryInfo root = new DirectoryInfo(path);
                FileInfo[] files = root.GetFiles();

                if (files.Count() > 0)
                {
                    // 如果存在文件 按照日期进行排序 asc
                    files.OrderBy(x => x.LastWriteTime).ToList().ForEach(x =>
                    {

                    });
                }
            }

            // 和下位机进行通讯 传输Bin文件
            // 等待下位机更新通知，如果未收到通知 循环三次发送bin文件
            // 更新成功 继续执行，更新失败弹窗提示
            #endregion

            _ = View.Dispatcher.BeginInvoke(new Action(async () =>
                {
                    if (DeviceHeuristic.Instance.DeviceList.Count < 1)
                    {
                        (View as StartPageView).retry.Visibility = Visibility.Visible;
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
