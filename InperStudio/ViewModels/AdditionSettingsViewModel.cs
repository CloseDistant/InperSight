using HandyControl.Controls;
using HandyControl.Data;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Lib.Helper.JsonBean;
using InperStudio.Views;
using InperStudioControlLib.Lib.Config;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace InperStudio.ViewModels
{
    public class AdditionSettingsViewModel : Screen
    {
        #region  properties
        private readonly AdditionSettingsTypeEnum @enum;
        private AdditionSettingsView view;

        #region video
        private BehaviorRecorderKit selectCameraItem;
        private ObservableCollection<BehaviorRecorderKit> unusedKits;
        private ObservableCollection<BehaviorRecorderKit> usedKits;
        public ObservableCollection<BehaviorRecorderKit> UnusedKits { get => unusedKits; set => SetAndNotify(ref unusedKits, value); }
        public ObservableCollection<BehaviorRecorderKit> UsedKits { get => usedKits; set => SetAndNotify(ref usedKits, value); }
        #endregion

        #region trigger
        private AdditionRecordConditions additionRecordStart;
        private AdditionRecordConditions additionRecordStop;
        public AdditionRecordConditions AdditionRecordStart { get => additionRecordStart; set => SetAndNotify(ref additionRecordStart, value); }
        public AdditionRecordConditions AdditionRecordStop { get => additionRecordStop; set => SetAndNotify(ref additionRecordStop, value); }
        #endregion

        #endregion

        public AdditionSettingsViewModel(AdditionSettingsTypeEnum @enum)
        {
            this.@enum = @enum;

            switch (@enum)
            {
                case AdditionSettingsTypeEnum.Trigger:
                    AdditionRecordStart = InperJsonHelper.GetAdditionRecordJson();
                    AdditionRecordStop = InperJsonHelper.GetAdditionRecordJson("stop");
                    break;
                case AdditionSettingsTypeEnum.Video:
                    break;
                default:
                    break;
            }
        }
        protected override void OnViewLoaded()
        {
            try
            {
                this.view = this.View as AdditionSettingsView;

                if (@enum == AdditionSettingsTypeEnum.Video)
                {
                    this.view.Height = 450;
                    this.view.video.Visibility = System.Windows.Visibility.Visible;

                    UsedKits = new ObservableCollection<BehaviorRecorderKit>();
                    UnusedKits = new ObservableCollection<BehaviorRecorderKit>();

                    InperComputerInfoHelper CompInfo = InperComputerInfoHelper.Instance;
                    foreach (KeyValuePair<int, string> c in CompInfo.ListCamerasData)
                    {
                        if (!c.Value.Contains("Basler"))
                        {
                            var item = new BehaviorRecorderKit(c.Key, c.Value);
                            UnusedKits.Add(item);
                        }
                    }
                }
                else
                {
                    this.view.trigger.Visibility = System.Windows.Visibility.Visible;
                    this.view.Title = "Start/Stop Conditions";
                }

                this.view.ConfirmClickEvent += View_ConfirmClickEvent;
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }

        private void View_ConfirmClickEvent(object arg1, System.Windows.Input.ExecutedRoutedEventArgs arg2)
        {
            try
            {
                if (@enum == AdditionSettingsTypeEnum.Video)
                {
                    if (usedKits.Count != 0)
                    {
                        var main = System.Windows.Application.Current.MainWindow.DataContext as MainWindowViewModel;

                        foreach (BehaviorRecorderKit item in UsedKits)
                        {
                            main.windowManager.ShowWindow(new VideoWindowViewModel(item));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
            this.RequestClose();
        }

        protected override void OnClose()
        {
            try
            {
                if (UnusedKits?.Count > 0)
                {
                    UnusedKits.ToList().ForEach(x => x.Dispose());
                }
                if (@enum == AdditionSettingsTypeEnum.Trigger)
                {
                    InperJsonHelper.SetAdditionRecodConditions(additionRecordStart);
                    InperJsonHelper.SetAdditionRecodConditions(additionRecordStop, "stop");
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
            GC.Collect();
        }

        #region methods Video
        public void CameraCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (selectCameraItem != null)
                {
                    selectCameraItem.StopPreview();
                    //selectCameraItem.Dispose();
                }
                selectCameraItem = view.CameraCombox.SelectedItem as BehaviorRecorderKit;

                if (selectCameraItem != null)
                {
                    selectCameraItem.StartPreview();
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void CameraMove(string moveType)
        {
            try
            {
                var camera = this.view.CameraCombox.SelectedItem as BehaviorRecorderKit;
                var camera_active = this.view.cameraActiveChannel.SelectedItem as BehaviorRecorderKit;
                if (moveType == "leftMove")//右移是激活 左移是取消激活
                {
                    if (UsedKits.Count > 0 && camera_active != null)
                    {
                        _ = UsedKits.Remove(camera_active);
                        UnusedKits.Add(camera_active);
                        if (UnusedKits.Count <= 1)
                        {
                            view.CameraCombox.SelectedIndex = 0;
                        }
                    }
                }
                else
                {
                    if (UnusedKits.Count > 0 && camera != null)
                    {
                        _ = UnusedKits.Remove(camera);
                        UsedKits.Add(camera);

                        //view.PopButton.Background = MarkerChannels.First().BgColor;
                        view.CameraCombox.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void CameraName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                HandyControl.Controls.TextBox tb = sender as HandyControl.Controls.TextBox;

                if (tb.Text.Length < 8 || !tb.Text.StartsWith("Video - "))
                {
                    tb.Text = "Video - ";
                    tb.SelectionStart = tb.Text.Length;
                    Growl.Error(new GrowlInfo() { Message = "固定字符串，请勿修改", Token = "SuccessMsg", WaitTime = 1 });
                    return;
                }

            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        #endregion

        #region methods Trigger
        public void TriggerStart_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton radio = sender as RadioButton;
                switch (radio.Name)
                {
                    case "immediately":
                        InperGlobalClass.AdditionRecordConditionsStart = AdditionRecordConditionsTypeEnum.Immediately;
                        break;
                    case "delay":
                        InperGlobalClass.AdditionRecordConditionsStart = AdditionRecordConditionsTypeEnum.Delay;
                        break;
                    case "atTime":
                        InperGlobalClass.AdditionRecordConditionsStart = AdditionRecordConditionsTypeEnum.AtTime;
                        break;
                    case "triggerRad":
                        InperGlobalClass.AdditionRecordConditionsStart = AdditionRecordConditionsTypeEnum.Trigger;
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
        public void TriggerStop_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton radio = sender as RadioButton;
                switch (radio.Name)
                {
                    case "immediatelyStop":
                        InperGlobalClass.AdditionRecordConditionsStop = AdditionRecordConditionsTypeEnum.Immediately;
                        break;
                    case "delayStop":
                        InperGlobalClass.AdditionRecordConditionsStop = AdditionRecordConditionsTypeEnum.Delay;
                        break;
                    case "atTimeStop":
                        InperGlobalClass.AdditionRecordConditionsStop = AdditionRecordConditionsTypeEnum.AtTime;
                        break;
                    case "triggerRadStop":
                        InperGlobalClass.AdditionRecordConditionsStop = AdditionRecordConditionsTypeEnum.Trigger;
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
        #endregion
    }
}
