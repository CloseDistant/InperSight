﻿using InperPhotometry;
using InperProtocolStack.CmdPhotometry;
using InperProtocolStack.UnderUpgrade;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using InperStudioControlLib.Lib.Config;
using Stylet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
        CancellationTokenSource tokenStart = new CancellationTokenSource();
        public StartPageViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
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
            try
            {
                Version = InperConfig.Instance.Version;
                DirectoryInfo root = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "UnderBin"));
                FileInfo[] files = root.GetFiles();
                UnderUpgradeFunc under = new UnderUpgradeFunc();

                _ = View.Dispatcher.BeginInvoke(new Action(async () =>
                  {
                      #region 下位机更新
                      if (files.Length > 0)
                      {
                          under.SendFile(files, tokenStart);
                          await TaskExecute(tokenStart.Token);
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
                          await Task.Delay(2000);
                          (View as StartPageView).loading.Visibility = Visibility.Collapsed;
                          (View as StartPageView).remainder.Text = "Initialization completed";
                          InperDeviceHelper.Instance.DeviceSet(DeviceHeuristic.Instance.DeviceList.First());
                          InperDeviceHelper.Instance.DeviceInit();

                          //uint DEVICE_INFO_PUB = 0x000000E8;
                          //uint SMARTLIGHT_INFO = 0x000000E7;

                          //InperDeviceHelper.Instance.device.GetDeviceInfo(SMARTLIGHT_INFO);

                          await Task.Delay(1000);
                          windowManager.ShowWindow(new MainWindowViewModel(windowManager));
                          RequestClose();
                      }
                  }));

            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void SearchAgain()
        {
            try
            {
                _ = View.Dispatcher.BeginInvoke(new Action(async () =>
                {
                    DeviceHeuristic.Instance.UpdateDeviceList();
                    if (DeviceHeuristic.Instance.DeviceList.Count < 1)
                    {
                        MessageBox.Show("Device not found");
                    }
                    else
                    {
                        InperDeviceHelper.Instance.DeviceSet(DeviceHeuristic.Instance.DeviceList.First());
                        await Task.Delay(3000);
                        InperDeviceHelper.Instance.DeviceInit();
                        (View as StartPageView).loading.Visibility = Visibility.Collapsed;
                        (View as StartPageView).remainder.Text = "Initialization completed";

                        windowManager.ShowWindow(new MainWindowViewModel(windowManager));
                        RequestClose();
                    }
                }));
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
