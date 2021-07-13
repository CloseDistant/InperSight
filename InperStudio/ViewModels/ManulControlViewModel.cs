using HandyControl.Controls;
using HandyControl.Data;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using SciChart.Charting.Model.DataSeries;
using Stylet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace InperStudio.ViewModels
{
    public class ManulControlViewModel : Screen
    {
        #region properties
        private readonly IWindowManager windowManager;
        public MainWindowViewModel MainWindowViewModel { get; private set; }

        #endregion
        public ManulControlViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
            MainWindowViewModel = new MainWindowViewModel();
        }
        protected override void OnViewLoaded()
        {
            InperDeviceHelper.Instance.WaveInitEvent += (s, e) =>
            {
                if (e)
                {
                    View.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (InperClassHelper.GetWindowByNameChar("Camera Signal Settings") == null)
                        {
                            windowManager.ShowWindow(new SignalSettingsViewModel(Lib.Enum.SignalSettingsTypeEnum.Camera));
                        }
                    }));
                }
                else
                {
                    Growl.Error(new GrowlInfo() { Message = "设备初始化出现问题", Token = "SuccessMsg", WaitTime = 1 });
                }
            };
        }
        #region
        public void DataPathShow(string type)
        {
            try
            {
                switch (type)
                {
                    case "Path":
                        windowManager.ShowDialog(new DataPathConfigViewModel(DataConfigPathTypeEnum.Path));
                        break;
                    case "Load":
                        windowManager.ShowDialog(new DataPathConfigViewModel(DataConfigPathTypeEnum.Load));
                        break;
                    case "Save":
                        windowManager.ShowDialog(new DataPathConfigViewModel(DataConfigPathTypeEnum.Save));
                        break;
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void SignalSettingsShow(string type)
        {
            try
            {
                switch (type)
                {
                    case "Camera":
                        if (InperClassHelper.GetWindowByNameChar("Camera Signal Settings") == null)
                        {
                            windowManager.ShowWindow(new SignalSettingsViewModel(SignalSettingsTypeEnum.Camera));
                        }
                        break;
                    case "Analog":
                        windowManager.ShowDialog(new SignalSettingsViewModel(SignalSettingsTypeEnum.Analog));
                        break;
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void EventSettingsShow(string type)
        {
            try
            {
                switch (type)
                {
                    case "Marker":
                        _ = windowManager.ShowDialog(new EventSettingsViewModel(EventSettingsTypeEnum.Marker));
                        break;
                    case "Output":
                        _ = windowManager.ShowDialog(new EventSettingsViewModel(EventSettingsTypeEnum.Output));
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void AdditionSettingsShow(string type)
        {
            try
            {
                switch (type)
                {
                    case "Trigger":
                        _ = windowManager.ShowDialog(new AdditionSettingsViewModel(AdditionSettingsTypeEnum.Trigger));
                        break;
                    case "Video":
                        _ = windowManager.ShowDialog(new AdditionSettingsViewModel(AdditionSettingsTypeEnum.Video));
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void RecordSettingsShow(string type)
        {
            try
            {
                switch (type)
                {
                    case "Preview":
                        PreviewRecord();
                        break;
                    case "Start":
                        StartRecord();
                        break;
                    case "Stop":
                        StopRecord();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        private void PreviewRecord()
        {
            InperGlobalClass.IsPreview = true;
            InperGlobalClass.IsRecord = false;
            InperGlobalClass.IsStop = false;

            InperDeviceHelper.Instance.StartCollect();

            ((((View as ManulControlView).Parent as ContentControl).DataContext as MainWindowViewModel).ActiveItem as DataShowControlViewModel).SciScrollSet();
        }
        private void StartRecord()
        {
            if (!InperGlobalClass.IsPreview)
            {
                InperDeviceHelper.Instance.StartCollect();
            }
            InperGlobalClass.IsRecord = true;
            InperGlobalClass.IsPreview = true;
            InperGlobalClass.IsStop = false;

            if (!Directory.Exists(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName)))
            {
                _ = Directory.CreateDirectory(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName));
            }
        }
        private void StopRecord()
        {
            InperGlobalClass.IsRecord = false;
            InperGlobalClass.IsPreview = false;
            InperGlobalClass.IsStop = true;
            InperGlobalClass.DataFolderName = DateTime.Now.ToString("yyyyMMddHHmmss");

            InperDeviceHelper.Instance.StopCollect();

            while (InperClassHelper.GetWindowByNameChar("Video Window") != null)
            {
                InperClassHelper.GetWindowByNameChar("Video Window").Close();
            }
        }
        #endregion
    }
}
