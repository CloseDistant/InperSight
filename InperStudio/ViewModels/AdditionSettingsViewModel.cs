using HandyControl.Controls;
using HandyControl.Data;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Lib.Helper.JsonBean;
using InperStudio.Views;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace InperStudio.ViewModels
{
    public class AdditionSettingsViewModel : Screen
    {
        #region  properties
        private readonly AdditionSettingsTypeEnum @enum;
        private AdditionSettingsView view;

        #region video

        private VideoRecordBean selectCameraItem;
        private ObservableCollection<VideoRecordBean> unusedKits;
        //private ObservableCollection<BehaviorRecorderKit> usedKits;
        public ObservableCollection<VideoRecordBean> UnusedKits { get => unusedKits; set => SetAndNotify(ref unusedKits, value); }
        private ObservableCollection<VideoRecordBean> usedKits;
        public ObservableCollection<VideoRecordBean> UsedKits { get => usedKits; set => SetAndNotify(ref usedKits, value); }
        #endregion

        #region trigger
        private AdditionRecordConditions additionRecordStart;
        private AdditionRecordConditions additionRecordStop;
        public AdditionRecordConditions AdditionRecordStart { get => additionRecordStart; set => SetAndNotify(ref additionRecordStart, value); }
        public AdditionRecordConditions AdditionRecordStop { get => additionRecordStop; set => SetAndNotify(ref additionRecordStop, value); }
        public List<string> Source { get; set; } = new List<string>() { "Trig1", "Trig2" };
        public List<string> Mode { get; set; } = new List<string>() { "Edge", "Real time" };
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
            base.OnViewLoaded();
            try
            {
                view = View as AdditionSettingsView;

                if (@enum == AdditionSettingsTypeEnum.Video)
                {
                    UsedKits = InperGlobalClass.ActiveVideos;
                    view.video.Visibility = Visibility.Visible;
                    UnusedKits = new ObservableCollection<VideoRecordBean>();

                    view.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Image' OR PNPClass = 'Camera')");

                        //Dictionary<int, string> cameras = new Dictionary<int, string>();
                        //int count = 0;
                        //foreach (ManagementBaseObject device in searcher.Get())
                        //{
                        //    cameras.Add(count, (string)device["Caption"]);
                        //    count++;
                        //}

                        //CameraInfoesReader cameraInfoesReader = new CameraInfoesReader();
                        //var cameras = cameraInfoesReader.GetCameraInfos();

                        InperComputerInfoHelper CompInfo = InperComputerInfoHelper.Instance;


                        if (UsedKits.Count == CompInfo.ListCamerasData.Count)
                        {
                            return;
                        }
                        foreach (KeyValuePair<int, string> c in CompInfo.ListCamerasData)
                        {
                            if (!c.Value.Contains("Basler"))
                            {
                                VideoRecordBean it = InperGlobalClass.ActiveVideos.FirstOrDefault(x => x._CamIndex == c.Key || x.Name == c.Value);
                                if (it == null)
                                {
                                    var item = new VideoRecordBean(c.Key, c.Value);
                                    if (item.IsCanOpen)
                                    {
                                        view.Dispatcher.Invoke(() =>
                                        {
                                            UnusedKits.Add(item);
                                        });
                                    }
                                }
                                else
                                {
                                    it.IsActive = true;
                                }
                            }
                        }


                        if (unusedKits.Count > 0)
                        {
                            view.CameraCombox.SelectedItem = unusedKits.First(x => x.IsActive == false);
                        }
                    }));
                }
                else
                {
                    view.trigger.Visibility = Visibility.Visible;
                    view.Title = "Trigger";//"Start/Stop Conditions";
                    view.IsShowOtherButton = false;
                    view.source_comb.ItemsSource = Source;
                    view.mode_comb.ItemsSource = Mode;
                    view.stop_source_comb.ItemsSource = Source;
                    view.source_comb.SelectionChanged += Source_comb_SelectionChanged;
                }
                view.ConfirmClickEvent += View_ConfirmClickEvent;
                view.OtherClickEvent += View_OtherClickEvent;
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }

        private void Source_comb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comb = (sender as System.Windows.Controls.ComboBox).SelectedValue;
            if (comb.Equals("Trig1"))
            {
                view.stop_source_comb.SelectedValue = "Trig2";
            }
            else
            {
                view.stop_source_comb.SelectedValue = "Trig1";
            }
        }

        private void View_OtherClickEvent(object obj)
        {
            try
            {
                if (@enum == AdditionSettingsTypeEnum.Video)
                {
                    MainWindowViewModel main = null;
                    foreach (System.Windows.Window window in Application.Current.Windows)
                    {
                        if (window.Name.Contains("MainWindow"))
                        {
                            main = window.DataContext as MainWindowViewModel;
                            break;
                        }
                    }
                    if (UsedKits.Count != 0)
                    {
                        foreach (var item in UsedKits)
                        {
                            if (Application.Current.Windows.OfType<System.Windows.Window>().Count() > 1)
                            {
                                var window = Application.Current.Windows.OfType<System.Windows.Window>().FirstOrDefault(x => x.Title.Equals(item.CustomName));
                                if (window != null && window.GetType().Name != "AdditionSettingsView")
                                {
                                    continue;
                                }
                            }

                            if (item.IsActive)
                            {
                                var window = new VideoWindowViewModel(item);
                                main.windowManager.ShowWindow(window);
                                window.ActivateWith(main);
                            }
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

        private void View_ConfirmClickEvent(object arg1, System.Windows.Input.ExecutedRoutedEventArgs arg2)
        {
            if (unusedKits != null)
            {
                unusedKits.ToList().ForEach(x =>
                {
                    x.StopPreview();
                });
            }
            this.RequestClose();

        }

        protected override void OnClose()
        {
            try
            {
                if (@enum == AdditionSettingsTypeEnum.Trigger)
                {
                    InperJsonHelper.SetAdditionRecodConditions(additionRecordStart);
                    InperJsonHelper.SetAdditionRecodConditions(additionRecordStop, "stop");
                }
                else
                {
                    if (UnusedKits.Count > 0)
                    {
                        UnusedKits.ToList().ForEach(x => x.StopPreview());
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }

        #region methods Video
        public void CameraCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (selectCameraItem != null)
                {
                    selectCameraItem.StopPreview();
                }
                selectCameraItem = view.CameraCombox.SelectedItem as VideoRecordBean;

                if (selectCameraItem != null)
                {
                    selectCameraItem.StartCapture();
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void ActiveVideo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ClickCount >= 2)
                {
                    VideoRecordBean item = (sender as Grid).DataContext as VideoRecordBean;
                    if (Application.Current.Windows.OfType<System.Windows.Window>().Count() > 1)
                    {
                        var window = Application.Current.Windows.OfType<System.Windows.Window>().FirstOrDefault(x => x.Title.Equals(item.CustomName));
                        if (window != null)
                        {
                            return;
                        }
                    }
                    MainWindowViewModel main = null;
                    foreach (System.Windows.Window window in Application.Current.Windows)
                    {
                        if (window.Name.Contains("MainWindow"))
                        {
                            main = window.DataContext as MainWindowViewModel;
                        }
                    }
                    if (item.IsActive)
                    {
                        var window = new VideoWindowViewModel(item);
                        main.windowManager.ShowWindow(window);
                        window.ActivateWith(main);
                    }
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
                if (moveType == "leftMove")//右移是激活 左移是取消激活
                {
                    if (UsedKits.Count > 0 && view.cameraActiveChannel.SelectedItem is VideoRecordBean camera_active)
                    {
                        if (Application.Current.Windows.OfType<System.Windows.Window>().FirstOrDefault(x => x.Title.Equals(camera_active.CustomName)) != null)
                        {
                            InperGlobalClass.ShowReminderInfo("The camera is running!");
                            return;
                        }
                        camera_active.IsActive = false;
                        //camera_active.StartCapture();
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
                    if (UnusedKits.Count > 0 && view.CameraCombox.SelectedItem is VideoRecordBean camera)
                    {
                        camera.CustomName = view.CameraName.Text;
                        if (camera.CustomName.EndsWith("-"))
                        {
                            camera.CustomName = camera.CustomName.Substring(0, camera.CustomName.Length - 1);
                        }
                        if (UsedKits.Count(x => x.CustomName == camera.CustomName) > 0)
                        {
                            Growl.Warning(new GrowlInfo() { Message = "This name already exists!", Token = "SuccessMsg", WaitTime = 1 });
                            return;
                        }
                        camera.AutoRecord = true; camera.IsActive = true;
                        _ = UnusedKits.Remove(camera);
                        UsedKits.Add(camera);
                        camera.StopPreview();
                        //view.PopButton.Background = MarkerChannels.First().BgColor;
                        view.CameraCombox.SelectedIndex = 0;
                        view.CameraName.Text = "Video-";
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

                if (tb.Text.Length < 6 || !tb.Text.StartsWith("Video-"))
                {
                    tb.Text = "Video-";
                    tb.SelectionStart = tb.Text.Length;
                    //Growl.Warning(new GrowlInfo() { Message = "固定字符串，请勿修改", Token = "SuccessMsg", WaitTime = 1 });
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
        public void AtTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var tb = sender as HandyControl.Controls.TextBox;
                if (tb.IsFocused)
                {
                    switch (tb.Name)
                    {
                        case "startHours":
                        case "stopHours":
                            if (double.Parse(tb.Text) < DateTime.Now.Hour)
                            {
                                //Growl.Warning("Incorrect input time", "SuccessMsg");
                                //tb.Text = DateTime.Now.Hour.ToString();
                                tb.Foreground = Brushes.Red;
                            }
                            break;
                        case "startMinutes":
                            if (AdditionRecordStart.AtTime.Hours == DateTime.Now.Hour)
                            {
                                if (double.Parse(tb.Text) < DateTime.Now.Minute)
                                {
                                    //Growl.Warning("Incorrect input time", "SuccessMsg");
                                    //tb.Text = DateTime.Now.Minute.ToString();
                                    tb.Foreground = Brushes.Red;
                                }
                            }
                            break;
                        case "stopMinutes":
                            if (additionRecordStop.AtTime.Hours == DateTime.Now.Hour)
                            {
                                if (double.Parse(tb.Text) < DateTime.Now.Minute)
                                {
                                    //Growl.Warning("Incorrect input time", "SuccessMsg");
                                    tb.Foreground = Brushes.Red;
                                    //tb.Text = DateTime.Now.Minute.ToString();
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
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
