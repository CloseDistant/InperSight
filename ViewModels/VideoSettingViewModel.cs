using Google.Protobuf.WellKnownTypes;
using HandyControl.Controls;
using HandyControl.Data;
using InperSight.Lib.Bean;
using InperSight.Lib.Config;
using InperSight.Lib.Device;
using InperSight.Lib.Helper;
using InperSight.Views;
using InperVideo.Camera;
using InperVideo.Interfaces;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace InperSight.ViewModels
{
    public class VideoSettingViewModel : Screen
    {
        private VideoSettingView view;
        private VideoDeviceHelper selectCameraItem;
        private ObservableCollection<VideoDeviceHelper> unusedKits;
        public ObservableCollection<VideoDeviceHelper> UnusedKits { get => unusedKits; set => SetAndNotify(ref unusedKits, value); }
        private ObservableCollection<VideoDeviceHelper> usedKits;
        public ObservableCollection<VideoDeviceHelper> UsedKits { get => usedKits; set => SetAndNotify(ref usedKits, value); }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            view = View as VideoSettingView;

            UsedKits = InperGlobalClass.ActiveVideos;
            UnusedKits = new();
            var camerInfoList = DeviceHelper.GetDeviceHelper().GetCameraInfo();

            foreach (var cameraInfo in camerInfoList)
            {
                var it = InperGlobalClass.ActiveVideos.FirstOrDefault(x => x._CamIndex == cameraInfo.Key || x.Name == cameraInfo.Value.FriendlyName);
                if (it == null)
                {
                    var item = new VideoDeviceHelper(cameraInfo.Key, cameraInfo.Value.FriendlyName, new CameraParamSet(cameraInfo.Value.CapabilyItems.FirstOrDefault().Size, cameraInfo.Value.CapabilyItems.FirstOrDefault().FrameRate, cameraInfo.Value.CapabilyItems.FirstOrDefault().Format));
                    var formats = cameraInfo.Value.CapabilyItems.OrderBy(f => f.Format).GroupBy(ca => ca.Format);
                    foreach (var format in formats)
                    {
                        if (!format.Key.Equals("RGB24"))
                        {
                            item.CapabilyItems.Add(format.Key, format.OrderBy(x => x.FrameRate));
                        }
                    }
                    UnusedKits.Add(item);
                }
                else
                {
                    if (!InperGlobalClass.IsPreview || !InperGlobalClass.IsRecord)
                    {
                        it.IsActive = true;
                    }
                }
            }
            view.ConfirmClickEvent += View_ConfirmClickEvent;
        }
        private void View_ConfirmClickEvent(object arg1, System.Windows.Input.ExecutedRoutedEventArgs arg2)
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
            if (UsedKits.Count != 0 && !InperGlobalClass.IsRecord)
            {
                foreach (var item in UsedKits)
                {
                    if (Application.Current.Windows.OfType<System.Windows.Window>().Count() > 1)
                    {
                        var window = Application.Current.Windows.OfType<System.Windows.Window>().FirstOrDefault(x => x.Title.Equals(item.CustomName));
                        if (window != null && window.GetType().Name != "VideoSettingView")
                        {
                            continue;
                        }
                    }

                    if (item.IsActive)
                    { 
                        var window = new VideoShowViewModel(item);
                        main.windowManager.ShowWindow(window);
                        window.ActivateWith(main);
                    }
                }
            }

            if (unusedKits != null)
            {
                //unusedKits.ToList().ForEach(x =>
                //{
                //    x.StopPreview();
                //});
                foreach (var item in unusedKits)
                {
                    item.StopPreview();
                }
            }

            this.RequestClose();

        }
        public void CameraCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (selectCameraItem != null)
                {
                    selectCameraItem.StopPreview();
                }
                selectCameraItem = view.CameraCombox.SelectedItem as VideoDeviceHelper;
                if (selectCameraItem != null)
                {
                    selectCameraItem.StartCapture();
                }
                view.format.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
            }
        }
        public void Format_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                view.framrate.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
            }
        }
        public void FramerateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (view.framrate.SelectedItem != null && selectCameraItem != null)
                {
                    selectCameraItem.Reset(new CameraParamSet((view.framrate.SelectedItem as CameraCapabilyItem).Size, (view.framrate.SelectedItem as CameraCapabilyItem).FrameRate, (view.framrate.SelectedItem as CameraCapabilyItem).Format));
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
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
                LoggerHelper.Error(ex.Message);
            }
        }
        public void CameraMove(string moveType)
        {
            try
            {
                if (moveType == "leftMove")//右移是激活 左移是取消激活
                {
                    if (UsedKits.Count > 0 && view.cameraActiveChannel.SelectedItem is VideoDeviceHelper camera_active)
                    {
                        if (Application.Current.Windows.OfType<System.Windows.Window>().FirstOrDefault(x => x.Title.Equals(camera_active.CustomName)) != null)
                        {
                            InperGlobalFunc.ShowRemainder("The camera is running!");
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
                    if (UnusedKits.Count > 0 && view.CameraCombox.SelectedItem is VideoDeviceHelper camera)
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
                LoggerHelper.Error(ex.Message);
            }
        }

        protected override void OnClose()
        {
            try
            {
                if (UnusedKits.Count > 0)
                {
                    UnusedKits.ToList().ForEach(x => { x.StopPreview(); });
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
    }
}
