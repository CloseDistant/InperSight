﻿using HandyControl.Tools.Extension;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Data.Model;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Lib.Helper.JsonBean;
using InperStudio.Views;
using InperStudio.Views.Control;
using InperStudioControlLib.Lib.Config;
using InperStudioControlLib.Lib.Helper;
using Stylet;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace InperStudio.ViewModels
{
    public class MainWindowViewModel : Conductor<IScreen>
    {
        #region properties
        public IWindowManager windowManager;
        private MainWindowView windowView;
        private LeftToolsControlViewModel leftToolsControlViewModel;
        private ManulControlViewModel manulControlViewModel;
        public LeftToolsControlViewModel LeftToolsControlViewModel { get => leftToolsControlViewModel; set => SetAndNotify(ref leftToolsControlViewModel, value); }
        public ManulControlViewModel ManulControlViewModel { get => manulControlViewModel; set => SetAndNotify(ref manulControlViewModel, value); }
        public static BindableCollection<ListChartValues<TimeSpan, double>> ChartDatas { get; set; } = new BindableCollection<ListChartValues<TimeSpan, double>>();
        public DataShowControlViewModel DataShowControlViewModel { get; set; }
        #endregion

        #region 构造和重载
        public MainWindowViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
        }
        public MainWindowViewModel()
        {
            //InperDeviceHelper.Instance.DeviceInit();
            _ = Task.Factory.StartNew(() => { InperDeviceHelper.Instance.DisplayProc(); });
        }
        protected override void OnViewLoaded()
        {
            try
            {
                base.OnViewLoaded();
                windowView = View as MainWindowView;
                // 手动强制刷新界面
                LeftToolsControlViewModel = new LeftToolsControlViewModel(windowManager);
                ManulControlViewModel = new ManulControlViewModel(windowManager);
                DataShowControlViewModel = new DataShowControlViewModel(windowManager);
                ActiveItem = DataShowControlViewModel;

                windowView.NonClientAreaContent = new MainTitleContentArea();
                InperDeviceHelper.Instance.device.SetSweepState(0);

                this.View.Hide();
                this.View.Show();
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        protected override void OnClose()
        {
            SaveCameraConfig();
            RequestClose();
            string exeName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string[] exeArray = exeName.Split('\\');
            KillProcess(exeArray.Last().Split('.').First());
            System.Environment.Exit(0);
        }
        public void SaveCameraConfig()
        {
            try
            {
                InperGlobalClass.CameraSignalSettings.RecordMode.IsContinuous = leftToolsControlViewModel.IsContinuous;
                InperGlobalClass.CameraSignalSettings.RecordMode.IsInterval = leftToolsControlViewModel.IsInterval;

                InperGlobalClass.CameraSignalSettings.CameraChannels.ForEach(x =>
                {
                    if (x.Name.EndsWith("-"))
                    {
                        x.Name = x.Name.Substring(0, x.Name.Length - 1);
                    }
                });

                InperDeviceHelper.Instance.CameraChannels.ToList().ForEach(x =>
                {
                    if (x.Name.EndsWith("-"))
                    {
                        x.Name = x.Name.Substring(0, x.Name.Length - 1);
                    }
                });

                InperJsonHelper.SetCameraSignalSettings(InperGlobalClass.CameraSignalSettings);
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        private void KillProcess(string processName)
        {
            Process[] myproc = Process.GetProcesses();
            foreach (Process item in myproc)
            {
                if (item.ProcessName == processName)
                {
                    item.Kill();
                }
            }

        }
        public void MainWindow_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            InperGlobalClass.EventSettings.Channels.ForEach(x =>
            {

                if (x.IsActive && x.Type == ChannelTypeEnum.Manual.ToString())
                {
                    string[] hotkeys = x.Hotkeys.Split('+');
                    if (hotkeys.Contains(e.Key.ToString()))
                    {
                        AddMarkers(hotkeys, x);
                        e.Handled = true;
                    }
                }
                else if (x.IsActive && x.Type == ChannelTypeEnum.Output.ToString() && x.Condition != null)
                {
                    if (x.Condition.Type == ChannelTypeEnum.Manual.ToString())
                    {
                        string[] hotkeys = x.Condition.Hotkeys.Split('+');
                        if (hotkeys.Contains(e.Key.ToString()))
                        {
                            AddMarkers(hotkeys, x, 1);
                            e.Handled = true;
                        }
                    }
                }
            });
            if (e.Key == Key.Space)
            {
                e.Handled = true;
                if (!windowView._focus1.IsFocused)
                {
                    windowView._focus1.Focus();
                }
                else
                {
                    windowView._focus2.Focus();
                }
            }

            //e.Handled = true;
        }
        private void AddMarkers(string[] hotkeys, EventChannelJson x, int type = 0)
        {
            int count = InperDeviceHelper.Instance.CameraChannels[0].RenderableSeries.First().DataSeries.XValues.Count;
            if (count > 0)
            {
                TimeSpan time = (TimeSpan)InperDeviceHelper.Instance.CameraChannels[0].RenderableSeries.First().DataSeries.XValues[count - 1];
                if (hotkeys.Count() == 1)
                {
                    if (Keyboard.IsKeyUp((Key)Enum.Parse(typeof(Key), hotkeys[0])))
                    {
                        InperDeviceHelper.Instance.SetMarkers(new BaseMarker()
                        {
                            CameraTime = time.Ticks,
                            ChannelId = x.ChannelId,
                            Color = x.BgColor,
                            IsIgnore = type == 0 ? true : false,
                            Type = ChannelTypeEnum.Manual.ToString(),
                            CreateTime = DateTime.Now,
                            Name = x.Name,
                            ConditionId = -1
                        });
                        return;
                    }
                }
                if (hotkeys.Count() == 2)
                {
                    if (Keyboard.IsKeyUp((Key)Enum.Parse(typeof(Key), hotkeys[0])) && Keyboard.IsKeyDown((Key)Enum.Parse(typeof(Key), hotkeys[1])))
                    {
                        InperDeviceHelper.Instance.SetMarkers(new BaseMarker()
                        {
                            CameraTime = time.Ticks,
                            ChannelId = x.ChannelId,
                            Color = x.BgColor,
                            IsIgnore = type == 0 ? true : false,
                            Type = ChannelTypeEnum.Manual.ToString(),
                            CreateTime = DateTime.Now,
                            Name = x.Name,
                            ConditionId = -1
                        });
                        return;
                    }
                }
                if (hotkeys.Count() == 3)
                {
                    if (Keyboard.IsKeyUp((Key)Enum.Parse(typeof(Key), hotkeys[0])) && Keyboard.IsKeyDown((Key)Enum.Parse(typeof(Key), hotkeys[1])) && Keyboard.IsKeyDown((Key)Enum.Parse(typeof(Key), hotkeys[2])))
                    {
                        InperDeviceHelper.Instance.SetMarkers(new BaseMarker()
                        {
                            CameraTime = time.Ticks,
                            ChannelId = x.ChannelId,
                            Color = x.BgColor,
                            IsIgnore = type == 0 ? true : false,
                            Type = ChannelTypeEnum.Manual.ToString(),
                            CreateTime = DateTime.Now,
                            Name = x.Name,
                            ConditionId = -1
                        });
                        return;
                    }
                }
            }
        }
        public void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                bool isCancle = false;
                if (!InperGlobalClass.IsImportConfig)
                {
                    string text = "Unsaved configuration has been detected.Do you want to save it?";
                    if (InperConfig.Instance.Language.ToLower() != "en_us")
                    {
                        text = "检测到有配置未保存，是否要保存？";
                    }
                    InperDialogWindow inperDialogWindow = new InperDialogWindow(text);
                    inperDialogWindow.ClickEvent += (s, statu) =>
                    {
                        if (statu == 0)
                        {
                            isCancle = !MainTitleContentArea.JsonConfigSaveAs();
                        }
                        else if (statu == 2)
                        {
                            isCancle = true;
                        }
                    };
                    inperDialogWindow.ShowDialog();
                }
                if (isCancle)
                {
                    e.Cancel = true;
                    return;
                }
                InperConfig.Instance.IsSkip = false;
                SystemSleepHelper.ResotreSleep();

                InperDeviceHelper.Instance.LightWaveLength.ToList().ForEach(x =>
                {
                    InperDeviceHelper.Instance.device.SwitchLight((uint)x.GroupId, false);
                    InperDeviceHelper.Instance.device.SetLightPower((uint)x.GroupId, 0);
                    Thread.Sleep(50);
                });
                InperDeviceHelper.Instance.device.Stop();

                string[] files = Directory.GetDirectories(Path.Combine(InperGlobalClass.DataPath));
                if (files.Length > 0)
                {
                    foreach (string s in files)
                    {
                        if (Directory.GetFiles(s).Length == 0)
                        {
                            Directory.Delete(s);
                        }
                    }
                }
                if (System.Windows.Application.Current.Windows.OfType<Window>().Count() > 1)
                {
                    foreach (var x in System.Windows.Application.Current.Windows.OfType<Window>().ToList())
                    {
                        if (!x.Name.StartsWith("MainWindow"))
                        {
                            x.Close();
                            Thread.Sleep(300);
                        }
                    }
                }
                InperLogExtentHelper.DeviceStatuSet(1);
                InperLogExtentHelper.DeviceUseMonitorOpenCountSet();
                base.OnClose();
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        #endregion

        #region
        public void ButtonVisibilitySwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            this.windowView.main.ColumnDefinitions[0].Width = new GridLength(0);
        }
        public void ButtonVisibilitySwitch_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as ToggleButton).IsFocused)
            {
                this.windowView.main.ColumnDefinitions[0].Width = new GridLength(320);
            }
        }

        #endregion

        #region left
        public void AInperTextBox_InperTextChanged(object arg1, TextChangedEventArgs arg2)
        {
            leftToolsControlViewModel.AInperTextBox_InperTextChanged(arg1, arg2);
        }
        public void ChannelRoi_TextChanged(object sender, TextChangedEventArgs e)
        {
            leftToolsControlViewModel.ChannelRoi_TextChanged(sender, e);
        }
        public void LightMode_TextChanged(object sender, TextChangedEventArgs e)
        {
            leftToolsControlViewModel.LightMode_TextChanged(sender, e);
        }
        public void Gain_TextChanged(object sender, TextChangedEventArgs e)
        {
            leftToolsControlViewModel.Gain_TextChanged(sender, e);
        }
        #endregion
    }
}
