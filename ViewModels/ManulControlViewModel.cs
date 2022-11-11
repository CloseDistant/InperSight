using InperSight.Lib.Bean;
using InperSight.Lib.Config;
using InperSight.Lib.Helper;
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
                        if (InperGlobalFunc.GetWindowByNameChar("Imaging") == null)
                        {
                            windowManager.ShowWindow(new CameraSettingViewModel());
                        }
                        else
                        {
                            _ = InperGlobalFunc.GetWindowByNameChar("Imaging").Activate();
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
