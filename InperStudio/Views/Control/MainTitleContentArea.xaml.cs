using HandyControl.Controls;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.ViewModels;
using InperStudioControlLib.Lib.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            Growl.Success("Save success", "SuccessMsg");
        }
        private void SaveConfigAs_Click(object sender, RoutedEventArgs e)
        {
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
                    if (InperClassHelper.GetWindowByNameChar("Camera Signal Settings") != null)
                    {
                        InperClassHelper.GetWindowByNameChar("Camera Signal Settings").Close();
                    }
                    //view.loadPath.Text = openFileDialog.FileName;
                    InperJsonConfig.filepath = openFileDialog.FileName;
                    InperGlobalClass.EventPanelProperties = InperJsonHelper.GetEventPanelProperties();
                    InperGlobalClass.CameraSignalSettings = InperJsonHelper.GetCameraSignalSettings();
                    InperGlobalClass.EventSettings = InperJsonHelper.GetEventSettings();

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

                    if (InperGlobalClass.EventSettings.Channels.Count > 0)
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
                    foreach (Lib.Bean.Channel.CameraChannel item in InperDeviceHelper.Instance.CameraChannels)
                    {
                        if (!cs.Contains(item.ChannelId))
                        {
                            _cs.Add(item.ChannelId);
                        }
                    }
                    _cs.ForEach(x =>
                    {
                        CameraChannel item = InperDeviceHelper.Instance.CameraChannels.FirstOrDefault(y => y.ChannelId == x);
                        _ = InperDeviceHelper.Instance.CameraChannels.Remove(item);
                        _ = InperDeviceHelper.Instance._SignalQs.TryRemove(x);
                    });

                    foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                    {
                        if (window.Name.Contains("MainWindow"))
                        {
                            (window.DataContext as MainWindowViewModel).windowManager.ShowWindow(new SignalSettingsViewModel(SignalSettingsTypeEnum.Camera));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        private void JsonConfigSaveAs()
        {
            try
            {
                System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                dlg.Filter = "Json|*.inper";
                // 设置默认的文件名。注意！文件扩展名须与Filter匹配
                dlg.FileName = "UserConfig";
                // 显示对话框
                DialogResult r = dlg.ShowDialog();

                if (r == DialogResult.Cancel)
                {
                    return;
                }
                string fname = dlg.FileName;
                File.Copy(InperJsonConfig.filepath, fname, true);
                InperJsonConfig.filepath = fname;
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
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
    }
}
