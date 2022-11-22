using InperSight.Lib.Bean;
using InperSight.Lib.Config;
using InperSight.Lib.Helper;
using InperSight.Views;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperSight.ViewModels
{
    public class ManulControlViewModel : Screen
    {
        #region properties
        private readonly IWindowManager windowManager;
        #endregion
        public ManulControlViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
        }
        public void SignalSettingsShow(string type)
        {
            try
            {
                switch (type)
                {
                    case "Camera":
                        if (InperGlobalFunc.GetWindowByNameChar("Insight") == null)
                        {
                            var camera = new CameraSettingViewModel();
                            windowManager.ShowWindow(camera);
                        }
                        else
                        {
                            _ = InperGlobalFunc.GetWindowByNameChar("Insight").Activate();
                        }
                        break;
                    case "Analog":
                        break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        public void EventSettingsShow(string type)
        {
            try
            {
                switch (type)
                {
                    case "Marker":
                        break;
                    case "Output":
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        public void AdditionSettingsShow(string type)
        {
            try
            {
                switch (type)
                {
                    case "Trigger":
                        break;
                    case "Video":
                        _ = windowManager.ShowDialog(new VideoSettingViewModel());
                        break;
                    case "Note":
                        if (InperGlobalFunc.GetWindowByNameChar("Note") == null)
                        {
                            windowManager.ShowWindow(new NoteSettingViewModel());
                        }
                        else
                        {
                            InperGlobalFunc.GetWindowByNameChar("Note").Activate();
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }

        public void RecordSettingsShow(string type)
        {
            try
            {
                switch (type)
                {
                    case "Preview":
                        InperDeviceHelper.GetInstance().Start(false);
                        break;
                    case "Start":
                        InperDeviceHelper.GetInstance().Start(true);
                        break;
                    case "Stop":
                        InperDeviceHelper.GetInstance().Stop();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
    }
}
