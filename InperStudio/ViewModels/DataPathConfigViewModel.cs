using InperStudio.Lib.Bean;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using InperStudioControlLib.Lib.Config;
using InperStudioControlLib.Lib.DeviceAgency;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Screen = Stylet.Screen;

namespace InperStudio.ViewModels
{
    public class DataPathConfigViewModel : Screen
    {
        #region properties
        private readonly DataConfigPathTypeEnum typeEnum;
        private DataPathConfigView view;
        #endregion

        #region method
        public DataPathConfigViewModel(DataConfigPathTypeEnum typeEnum)
        {
            this.typeEnum = typeEnum;
        }
        protected override void OnViewLoaded()
        {
            view = this.View as DataPathConfigView;
            switch (typeEnum)
            {
                case DataConfigPathTypeEnum.Path:
                    view.Path.Visibility = System.Windows.Visibility.Visible;
                    break;
                case DataConfigPathTypeEnum.Load:
                    view.Load.Visibility = System.Windows.Visibility.Visible;
                    break;
                case DataConfigPathTypeEnum.Save:
                    view.Save.Visibility = System.Windows.Visibility.Visible;
                    break;
            }
            if (string.IsNullOrEmpty(view.pathText.Text))
            {
                view.pathText.Text = Environment.CurrentDirectory + @"\Data\";
            }
            if (string.IsNullOrEmpty(view.fileName.Text))
            {
                view.fileName.Text = DateTime.Now.ToString("yyyyMMddHHmmss");
            }
        }

        public void ChoosePath(string type)
        {
            try
            {
                if (type == DataConfigPathTypeEnum.Path.ToString())
                {
                    FolderBrowserDialog openFileDialog = new FolderBrowserDialog();
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        view.pathText.Text = openFileDialog.SelectedPath + @"\";
                    }
                }
                if (type == DataConfigPathTypeEnum.Load.ToString())
                {
                    var window = InperClassHelper.GetWindowByNameChar("Camera Signal");
                    if (window != null)
                    {
                        window.Close();
                    }

                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "Json|*.inper";
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        view.loadPath.Text = openFileDialog.FileName;
                        InperJsonConfig.filepath = openFileDialog.FileName;
                        InperGlobalClass.EventPanelProperties = InperJsonHelper.GetEventPanelProperties();
                        InperGlobalClass.CameraSignalSettings = InperJsonHelper.GetCameraSignalSettings();
                        InperGlobalClass.EventSettings = InperJsonHelper.GetEventSettings();

                        InperGlobalClass.CameraSignalSettings.LightMode.ForEach(x =>
                        {
                            var wg = InperDeviceHelper.Instance.LightWaveLength.FirstOrDefault(y => y.GroupId == x.GroupId);
                            if (wg != null)
                            {
                                wg.IsChecked = x.IsChecked;
                                wg.LightPower = x.LightPower;
                                if (x.IsChecked)
                                {
                                    DevPhotometry.Instance.SwitchLight(wg.GroupId, true);
                                    DevPhotometry.Instance.SetLightPower(wg.GroupId, wg.LightPower);
                                }
                            }
                        });
                        List<int> cs = new List<int>();
                        List<int> _cs = new List<int>();
                        InperGlobalClass.CameraSignalSettings.CameraChannels.ForEach(x =>
                        {
                            cs.Add(x.ChannelId);
                        });
                        foreach (var item in InperDeviceHelper.Instance.CameraChannels)
                        {
                            if (!cs.Contains(item.ChannelId))
                            {
                                _cs.Add(item.ChannelId);
                            }
                        }
                        _cs.ForEach(x =>
                        {
                            var item = InperDeviceHelper.Instance.CameraChannels.FirstOrDefault(y => y.ChannelId == x);
                            InperDeviceHelper.Instance.CameraChannels.Remove(item);
                            InperDeviceHelper.Instance._SignalQs.Remove(x);
                        });
                        this.RequestClose();
                        (App.Current.MainWindow.DataContext as MainWindowViewModel).windowManager.ShowWindow(new SignalSettingsViewModel(SignalSettingsTypeEnum.Camera));
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void InperDialogWindow_ConfirmClickEvent(object sender, ExecutedRoutedEventArgs e)
        {
            this.RequestClose();
        }
        #endregion
    }
}
