using HandyControl.Controls;
using HandyControl.Data;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Bean.Stimulus;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.ViewModels;
using InperStudioControlLib.Lib.Config;
using SciChart.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace InperStudio.Views.Control
{
    /// <summary>
    /// MainTitleContentArea.xaml 的交互逻辑
    /// </summary>
    public partial class MainTitleContentArea : Border
    {
        public InperGlobalClass InperGlobalClass { get; set; } = new InperGlobalClass();
        public List<string> SkinColorList { get; set; } = InperColorHelper.ColorPresetList;
        public MainTitleContentArea()
        {
            InitializeComponent();
            DataContext = this;
            this.move1.AddHandler(Border.MouseDownEvent, new System.Windows.Input.MouseButtonEventHandler(Border_MouseDown), true);
            this.move2.AddHandler(Border.MouseDownEvent, new System.Windows.Input.MouseButtonEventHandler(Border_MouseDown), true);
        }

        private void ImportConfig_Click(object sender, RoutedEventArgs e)
        {
            DefalutFileLoad();
        }
        private void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            Growl.Success(new GrowlInfo() { Message = "Saved successfully", Token = "SuccessMsg", WaitTime = 1 });
        }
        private void SaveConfigAs_Click(object sender, RoutedEventArgs e)
        {
            foreach (System.Windows.Window window in App.Current.Windows)
            {
                if (window.Name.Contains("MainWindow"))
                {
                    var main = window.DataContext as MainWindowViewModel;
                    main.SaveCameraConfig();
                    break;
                }
            }
            JsonConfigSaveAs();
        }
        private void DefalutFileLoad()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Json|*.inper"
                };
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //if (InperClassHelper.GetWindowByNameChar("Camera Signal") != null)
                    //{
                    //    InperClassHelper.GetWindowByNameChar("Camera Signal").Close();
                    //}
                    //view.loadPath.Text = openFileDialog.FileName;
                    InperJsonConfig.filepath = openFileDialog.FileName;
                    InperGlobalClass.EventPanelProperties = InperJsonHelper.GetEventPanelProperties();
                    InperGlobalClass.CameraSignalSettings = InperJsonHelper.GetCameraSignalSettings();
                    InperGlobalClass.EventSettings = InperJsonHelper.GetEventSettings();
                    InperGlobalClass.StimulusSettings = InperJsonHelper.GetStimulusSettings() ?? new Lib.Helper.JsonBean.StimulusSettings();
                    bool.TryParse(InperJsonHelper.GetDisplaySetting("analog"), out bool analog);
                    InperGlobalClass.IsDisplayAnalog = analog;
                    bool.TryParse(InperJsonHelper.GetDisplaySetting("trigger"), out bool trigger);
                    InperGlobalClass.IsDisplayTrigger = trigger;
                    bool.TryParse(InperJsonHelper.GetDisplaySetting("note"), out bool note);
                    InperGlobalClass.IsDisplayNote = note;
                    bool.TryParse(InperJsonHelper.GetDisplaySetting("sprit"), out bool sprit);
                    InperGlobalClass.IsDisplaySprit = sprit;
                    if (!string.IsNullOrEmpty(InperJsonHelper.GetDataPathSetting()))
                    {
                        try
                        {
                            InperGlobalClass.DataPath = InperJsonHelper.GetDataPathSetting() ?? InperGlobalClass.DataPath;
                            Directory.CreateDirectory(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName));
                        }
                        catch (Exception)
                        {

                        }
                    }

                    if (InperGlobalClass.StimulusSettings.Sweeps.Count > 0)
                    {
                        StimulusBeans.Instance.Sweeps.Clear();
                        InperGlobalClass.StimulusSettings.Sweeps.ToList().ForEach(x => StimulusBeans.Instance.Sweeps.Add(x));
                    }
                    if (InperGlobalClass.StimulusSettings.WaveForms.Count > 0)
                    {
                        StimulusBeans.Instance.WaveForms.Clear();
                        InperGlobalClass.StimulusSettings.WaveForms.ToList().ForEach(x => StimulusBeans.Instance.WaveForms.Add(x));
                    }
                    StimulusBeans.Instance.DioID = InperGlobalClass.StimulusSettings.DioID;
                    StimulusBeans.Instance.IsConfigSweep = InperGlobalClass.StimulusSettings.IsConfigSweep;
                    StimulusBeans.Instance.Hour = InperGlobalClass.StimulusSettings.Hour;
                    StimulusBeans.Instance.Minute = InperGlobalClass.StimulusSettings.Minute;
                    StimulusBeans.Instance.Seconds = InperGlobalClass.StimulusSettings.Seconds;
                    StimulusBeans.Instance.TriggerId = InperGlobalClass.StimulusSettings.TriggerId;
                    StimulusBeans.Instance.TriggerMode = InperGlobalClass.StimulusSettings.TriggerMode;
                    StimulusBeans.Instance.IsTrigger = InperGlobalClass.StimulusSettings.IsTrigger;

                    if (StimulusBeans.Instance.IsConfigSweep)
                    {
                        StimulusBeans.Instance.StimulusCommandSend();
                    }

                    InperDeviceHelper.Instance.device.SetExposure(InperGlobalClass.CameraSignalSettings.Exposure);
                    InperGlobalClass.SetSampling(InperGlobalClass.CameraSignalSettings.Sampling);

                    InperGlobalClass.ManualEvents.Clear();
                    foreach (Lib.Helper.JsonBean.EventChannelJson item in InperGlobalClass.EventSettings.Channels)
                    {
                        if (item.Type == ChannelTypeEnum.Manual.ToString())
                        {
                            InperGlobalClass.ManualEvents.Add(item);
                        }
                        if (item.Condition?.Type == ChannelTypeEnum.Manual.ToString())
                        {
                            InperGlobalClass.ManualEvents.Add(new Lib.Helper.JsonBean.EventChannelJson()
                            {
                                BgColor = item.BgColor,
                                ChannelId = item.ChannelId,
                                Hotkeys = item.Condition.Hotkeys,
                                HotkeysCount = item.Condition.HotkeysCount,
                                Name = item.Name,
                                SymbolName = item.SymbolName,
                                Type = item.Condition.Type,
                                IsActive = item.IsActive
                            });
                        }
                    }

                    if (InperGlobalClass.EventSettings.Channels.Count(x => x.Type != ChannelTypeEnum.TriggerStart.ToString() && x.Type != ChannelTypeEnum.TriggerStop.ToString()) > 0)
                    {
                        InperGlobalClass.IsExistEvent = true;
                    }

                    InperGlobalClass.CameraSignalSettings.LightMode.ForEach(x =>
                    {
                        var wg = InperDeviceHelper.Instance.LightWaveLength.FirstOrDefault(y => y.GroupId == x.GroupId);
                        if (wg != null)
                        {
                            wg.IsChecked = x.IsChecked;
                            wg.LightPower = x.LightPower;
                            if (x.IsChecked)
                            {
                                InperDeviceHelper.Instance.device.SwitchLight((uint)wg.GroupId, true);
                                InperDeviceHelper.Instance.device.SetLightPower((uint)wg.GroupId, wg.LightPower);
                            }
                        }
                    });

                    List<int> cs = new List<int>();
                    List<int> _cs = new List<int>();
                    InperGlobalClass.CameraSignalSettings.CameraChannels.ForEach(x =>
                    {
                        cs.Add(x.ChannelId);
                    });
                    foreach (CameraChannel item in InperDeviceHelper.Instance.CameraChannels)
                    {
                        item.RenderableSeries.ForEachDo(x => x.DataSeries.Clear());
                        if (!cs.Contains(item.ChannelId))
                        {
                            _cs.Add(item.ChannelId);
                        }
                    }
                    _cs.ForEach(x =>
                    {
                        CameraChannel item = InperDeviceHelper.Instance.CameraChannels.FirstOrDefault(y => y.ChannelId == x);
                        _ = InperDeviceHelper.Instance.CameraChannels.Remove(item);
                    });
                    //if (InperClassHelper.GetWindowByNameChar("inper") is System.Windows.Window window)
                    //{
                    //    (window.DataContext as MainWindowViewModel).LeftToolsControlViewModel.InitConfig(true);
                    //}
                    foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                    {
                        if (window.Name.Contains("MainWindow"))
                        {
                            (window.DataContext as MainWindowViewModel).LeftToolsControlViewModel.InitConfig(true);
                        }
                    }
                }

                InperGlobalClass.IsImportConfig = true;
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public static bool JsonConfigSaveAs()
        {
            try
            {
                System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog
                {
                    Filter = "Json|*.inper",
                    // 设置默认的文件名。注意！文件扩展名须与Filter匹配
                    FileName = "UserConfig"
                };
                // 显示对话框
                DialogResult r = dlg.ShowDialog();

                if (r == DialogResult.Cancel)
                {
                    return false;
                }
                string fname = dlg.FileName;
                if (InperJsonConfig.filepath != fname)
                {
                    File.Copy(InperJsonConfig.filepath, fname, true);
                }
                InperJsonConfig.filepath = fname;
                InperGlobalClass.IsImportConfig = true;

            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, "MainTitleContentArea");
            }
            return true;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                {
                    if (window.Name.Contains("MainWindow"))
                    {
                        window.DragMove();
                    }
                }

            }
        }
        private void DataFolderName_Click(object sender, RoutedEventArgs e)
        {
            foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
            {
                if (window.Name.Contains("MainWindow"))
                {
                    (window.DataContext as MainWindowViewModel).windowManager.ShowDialog(new DataPathConfigViewModel(DataConfigPathTypeEnum.Path));
                }
            }
        }

        private void SkinList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (sender as System.Windows.Controls.ListBox).SelectedItem;

            System.Windows.Application.Current.Resources["InperTheme"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(item.ToString()));
        }

        private void Skin_Click(object sender, RoutedEventArgs e) => PopupConfig.IsOpen = true;

        private void About_Click(object sender, RoutedEventArgs e)
        {
            foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
            {
                if (window.Name.Contains("MainWindow"))
                {
                    (window.DataContext as MainWindowViewModel).windowManager.ShowDialog(new AboutInperSignalViewModel());
                }
            }
        }

        private void Language_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                {
                    if (window.Name.Contains("MainWindow"))
                    {
                        (window.DataContext as MainWindowViewModel).windowManager.ShowDialog(new PreferenceWindowViewModel());
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
    }
}
