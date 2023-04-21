using HandyControl.Controls;
using HandyControl.Data;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Chart;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Lib.Helper.JsonBean;
using InperStudio.Views;
using InperStudio.Views.Control;
using InperStudioControlLib.Lib.Config;
using InperVideo.Camera;
using InperVideo.Interfaces;
using SciChart.Core.Extensions;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace InperStudio.ViewModels
{
    public enum ShapeDashEnum
    {
        //矩形区域
        RectangleDash,
        //圆形区域
        EllipseDash,
        //多边形区域
        PathDash,
        None
    }
    public enum ShapeEnum
    {
        //矩形区域
        Rectangle,
        //圆形区域
        Ellipse,
        //多边形区域
        Path,
        None
    }
    public class AdditionSettingsViewModel : Stylet.Screen
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
        private ObservableCollection<EventChannelJson> eventChannelsStart = new ObservableCollection<EventChannelJson>();
        public ObservableCollection<EventChannelJson> EventChannelsStart { get => eventChannelsStart; set => SetAndNotify(ref eventChannelsStart, value); }
        private ObservableCollection<EventChannelJson> eventChannelsStop = new ObservableCollection<EventChannelJson>();
        public ObservableCollection<EventChannelJson> EventChannelsStop { get => eventChannelsStop; set => SetAndNotify(ref eventChannelsStop, value); }
        private ObservableCollection<ZoneVideo> zoneVideos = new ObservableCollection<ZoneVideo>();
        public ObservableCollection<ZoneVideo> ZoneVideos { get => zoneVideos; set => SetAndNotify(ref zoneVideos, value); }
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
                    if (InperConfig.Instance.Language == "en_us")
                    {
                        view.Title = "Video Settings";//"Start/Stop Conditions";
                    }
                    else
                    {
                        view.Title = "视频设置";
                    }
                    UsedKits = InperGlobalClass.ActiveVideos;
                    view.video.Visibility = Visibility.Visible;
                    UnusedKits = new ObservableCollection<VideoRecordBean>();

                    IEnumerable<ICameraInfo> camerInfoList = new List<ICameraInfo>();

                    camerInfoList = new CameraInfoesReader().GetCameraInfos();

                    int i = 0;
                    if (camerInfoList != null)
                    {
                        foreach (var c in camerInfoList)
                        {

                            if (!c.FriendlyName.Contains("Basler"))
                            {
                                VideoRecordBean it = InperGlobalClass.ActiveVideos.FirstOrDefault(x => x._CamIndex == i || x.Name == c.FriendlyName);
                                if (it == null)
                                {
                                    var item = new VideoRecordBean(i, c.FriendlyName, new CameraParamSet(c.CapabilyItems.FirstOrDefault().Size, c.CapabilyItems.FirstOrDefault().FrameRate, c.CapabilyItems.FirstOrDefault().Format));
                                    var formats = c.CapabilyItems.OrderBy(f => f.Format).GroupBy(ca => ca.Format);
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
                            i++;
                        }
                    }

                    if (unusedKits.Count > 0)
                    {
                        this.view.Dispatcher.Invoke(() =>
                        {
                            view.CameraCombox.SelectedItem = unusedKits.First(x => x.IsActive == false);
                        });
                    }

                }
                else
                {
                    view.trigger.Visibility = Visibility.Visible;
                    if (InperConfig.Instance.Language == "en_us")
                    {
                        view.Title = "Trigger";//"Start/Stop Conditions";
                    }
                    else
                    {
                        view.Title = "触发";
                    }
                    view.IsShowOtherButton = false;
                    //view.source_comb.ItemsSource = Source;
                    //view.mode_comb.ItemsSource = Mode;
                    //view.stop_source_comb.ItemsSource = Source;
                    //view.source_comb.SelectionChanged += Source_comb_SelectionChanged;
                    foreach (KeyValuePair<string, uint> item in InperDeviceHelper.Instance.device.DeviceIOIDs)
                    {
                        EventChannelsStart.Add(new EventChannelJson()
                        {
                            ChannelId = (int)item.Value,
                            SymbolName = item.Key.ToString(),
                            Name = item.Key.ToString(),
                        });
                        EventChannelsStop.Add(new EventChannelJson()
                        {
                            ChannelId = (int)item.Value,
                            SymbolName = item.Key.ToString(),
                            Name = item.Key.ToString(),
                        });
                    }

                    foreach (EventChannelJson item in InperGlobalClass.EventSettings.Channels)
                    {
                        var chn = EventChannelsStart.FirstOrDefault(x => x.ChannelId == item.ChannelId && item.SymbolName.StartsWith("DIO") && (item.Type != ChannelTypeEnum.TriggerStart.ToString() && item.Type != ChannelTypeEnum.TriggerStop.ToString()));
                        var chn1 = EventChannelsStop.FirstOrDefault(x => x.ChannelId == item.ChannelId && item.SymbolName.StartsWith("DIO") && (item.Type != ChannelTypeEnum.TriggerStart.ToString() && item.Type != ChannelTypeEnum.TriggerStop.ToString()));
                        EventChannelsStart.Remove(chn);
                        EventChannelsStop.Remove(chn1);
                        if (item.Type == ChannelTypeEnum.TriggerStart.ToString())
                        {
                            view.start_trigger.SelectedIndex = item.ChannelId;
                            EventChannelsStart.FirstOrDefault(x => x.ChannelId == item.ChannelId && item.Type == ChannelTypeEnum.TriggerStart.ToString()).IsActive = true;
                        }
                        if (item.Type == ChannelTypeEnum.TriggerStop.ToString())
                        {
                            view.stop_trigger.SelectedIndex = item.ChannelId;
                            EventChannelsStop.FirstOrDefault(x => x.ChannelId == item.ChannelId && item.Type == ChannelTypeEnum.TriggerStop.ToString()).IsActive = true;
                        }
                    }
                    //不能合并上下两个for循环的原因
                    view.start_trigger.ItemsSource = EventChannelsStart;
                    view.stop_trigger.ItemsSource = EventChannelsStop;

                    view.start_trigger.SelectionChanged += (s, e) =>
                    {

                        if ((s as System.Windows.Controls.ComboBox).SelectedValue is EventChannelJson comb)
                        {
                            if (InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.Type == ChannelTypeEnum.TriggerStart.ToString()) is EventChannelJson eventChannelJson)
                            {
                                eventChannelJson.IsActive = false;
                                InperGlobalClass.EventSettings.Channels.Remove(eventChannelJson);
                                EventChannelsStop.Add(eventChannelJson);
                                EventChannelsStart.FirstOrDefault(x => x.ChannelId == eventChannelJson.ChannelId).IsActive = false;
                            }
                            comb.IsActive = true;
                            comb.Type = ChannelTypeEnum.TriggerStart.ToString();
                            InperGlobalClass.EventSettings.Channels.Add(comb);
                            additionRecordStart.Trigger.DioId = comb.ChannelId;
                            additionRecordStart.Trigger.IsActive = true;
                            additionRecordStart.Trigger.Name = comb.Name;
                            //消去stop中的该选项，设置为激活状态
                            if (EventChannelsStop.FirstOrDefault(x => x.ChannelId == comb.ChannelId) is EventChannelJson json)
                            {
                                EventChannelsStop.Remove(json);
                                view.stop_trigger.ItemsSource = EventChannelsStop = new ObservableCollection<EventChannelJson>(EventChannelsStop.OrderBy(x => x.ChannelId));
                            }
                        }
                    };
                    view.stop_trigger.SelectionChanged += (s, e) =>
                    {

                        if ((s as System.Windows.Controls.ComboBox).SelectedValue is EventChannelJson comb)
                        {
                            if (InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.Type == ChannelTypeEnum.TriggerStop.ToString()) is EventChannelJson eventChannelJson)
                            {
                                eventChannelJson.IsActive = false;
                                InperGlobalClass.EventSettings.Channels.Remove(eventChannelJson);
                                EventChannelsStart.Add(eventChannelJson);
                                EventChannelsStop.FirstOrDefault(x => x.ChannelId == eventChannelJson.ChannelId).IsActive = false;
                            }
                            comb.IsActive = true;
                            comb.Type = ChannelTypeEnum.TriggerStop.ToString();
                            InperGlobalClass.EventSettings.Channels.Add(comb);
                            additionRecordStop.Trigger.DioId = comb.ChannelId;
                            additionRecordStop.Trigger.IsActive = true;
                            additionRecordStop.Trigger.Name = comb.Name;
                            //消去start中的该选项，设置为激活状态
                            if (EventChannelsStart.FirstOrDefault(x => x.ChannelId == comb.ChannelId) is EventChannelJson json)
                            {
                                EventChannelsStart.Remove(json);
                                view.start_trigger.ItemsSource = EventChannelsStart = new ObservableCollection<EventChannelJson>(EventChannelsStart.OrderBy(x => x.ChannelId));
                            }
                        }
                    };
                }
                // 注册 Button 的 Click 事件
                EventManager.RegisterClassHandler(typeof(FrameworkElement), FrameworkElement.MouseDownEvent, new RoutedEventHandler(Framework_Down));
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
            finally
            {
                view.ConfirmClickEvent += View_ConfirmClickEvent;
                view.OtherClickEvent += View_OtherClickEvent;
            }
        }
        private void Framework_Down(object sender, RoutedEventArgs e)
        {
            string name = (sender as FrameworkElement).Name;
            // 判断触发事件的按钮是否是 Button2
            if (name == "")
            {
                ShapeDashColorChange(ShapeDashEnum.RectangleDash, "#929292");
                ShapeDashColorChange(ShapeDashEnum.EllipseDash, "#929292");
                ShapeDashColorChange(ShapeDashEnum.PathDash, "#929292");
                ShapeColorChange(ShapeEnum.Rectangle, "#929292");
                ShapeColorChange(ShapeEnum.Ellipse, "#929292");
                ShapeColorChange(ShapeEnum.Path, "#929292");
                _shapeDashEnum = ShapeDashEnum.None;
                _shapeEnum = ShapeEnum.None;
            }
            else
            {
                switch (name)
                {
                    case "rectangleDash":
                        ShapeDashColorChange(ShapeDashEnum.EllipseDash, "#929292");
                        ShapeDashColorChange(ShapeDashEnum.PathDash, "#929292");
                        ShapeColorRest("#929292");
                        break;
                    case "ellipseDash":
                        ShapeDashColorChange(ShapeDashEnum.RectangleDash, "#929292");
                        ShapeDashColorChange(ShapeDashEnum.PathDash, "#929292");
                        ShapeColorRest("#929292");
                        break;
                    case "pathDash":
                        ShapeDashColorChange(ShapeDashEnum.RectangleDash, "#929292");
                        ShapeDashColorChange(ShapeDashEnum.EllipseDash, "#929292");
                        ShapeColorRest("#929292");
                        break;
                    case "rect":
                        ShapeColorChange(ShapeEnum.Ellipse, "#929292");
                        ShapeColorChange(ShapeEnum.Path, "#929292");
                        ShapeDashColorRest("#929292");
                        break;
                    case "ellipse":
                        ShapeColorChange(ShapeEnum.Rectangle, "#929292");
                        ShapeColorChange(ShapeEnum.Path, "#929292");
                        ShapeDashColorRest("#929292");
                        break;
                    case "path":
                        ShapeColorChange(ShapeEnum.Rectangle, "#929292");
                        ShapeColorChange(ShapeEnum.Ellipse, "#929292");
                        ShapeDashColorRest("#929292");
                        break;
                }
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
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
            this.RequestClose();
        }

        private void View_ConfirmClickEvent(object arg1, System.Windows.Input.ExecutedRoutedEventArgs arg2)
        {
            this.RequestClose();

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
                if (UsedKits.Count != 0 && !InperGlobalClass.IsRecord)
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
        }

        protected override void OnClose()
        {
            try
            {
                if (@enum == AdditionSettingsTypeEnum.Trigger)
                {
                    InperJsonHelper.SetAdditionRecodConditions(additionRecordStart);
                    InperJsonHelper.SetAdditionRecodConditions(additionRecordStop, "stop");
                    InperJsonHelper.SetEventSettings(InperGlobalClass.EventSettings);
                }
                else
                {
                    if (UnusedKits.Count > 0)
                    {
                        UnusedKits.ToList().ForEach(x => { x.StopPreview(); });
                    }


                    //配置zone到marker和output
                    if (zoneVideos.Count > 0)
                    {
                        var items = zoneVideos.GroupBy(x => x.VideoName);
                        foreach (var item in items)
                        {
                            foreach (var item2 in item)
                            {
                                if (!item2.IsActiveVideo)
                                {
                                    break;
                                }
                                //从这里找到对应VIDEO的channel
                                if (InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.Name == item2.VideoShowName.ToString()) is EventChannelJson eventChannelJson)
                                {
                                    if (eventChannelJson.VideoZone.AllZoneConditions.Count(z => z.ZoneName == item2.Name) == 0)
                                    {
                                        eventChannelJson.VideoZone.AllZoneConditions.Add(new ZoneConditions()
                                        {
                                            ZoneName = item2.Name + "-" + item2.DisplayName,
                                            ShapeLeft = Canvas.GetLeft(item2.Shape),
                                            ShapeTop = Canvas.GetTop(item2.Shape),
                                            ShapeHeight=item2.Shape.Height,
                                            ShapeWidth=item2.Shape.Width,
                                            ShapeName=item2.Name
                                        });
                                    }
                                }
                                else
                                {
                                    Random random = new Random();
                                    int randomId = random.Next(100000, 1000000);
                                    var channel = new EventChannelJson()
                                    {
                                        ChannelId = randomId,
                                        Name = item2.VideoShowName,
                                        VideoZone = new VideoZone()
                                        {
                                            Name = item2.VideoName
                                        },
                                        Type = ChannelTypeEnum.Zone.ToString(),
                                    };
                                    channel.VideoZone.AllZoneConditions.Add(new ZoneConditions()
                                    {
                                        ZoneName = item2.Name + "-" + item2.DisplayName,
                                        ShapeLeft = Canvas.GetLeft(item2.Shape),
                                        ShapeTop = Canvas.GetTop(item2.Shape),
                                        ShapeHeight = item2.Shape.Height,
                                        ShapeWidth = item2.Shape.Width,
                                        ShapeName = item2.Name
                                    });
                                    InperGlobalClass.EventSettings.Channels.Add(channel);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }

        #region methods Video
        public void CameraCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                selectCameraItem?.StopPreview();
                selectCameraItem = view.CameraCombox.SelectedItem as VideoRecordBean;
                //Dialog d = Dialog.Show<ProgressDialog>("VideoDialog");
                //await Task.Factory.StartNew(() =>
                //{
                selectCameraItem?.StartCapture();
                //d.Close();
                view.format.SelectedIndex = 0;

                //清除并重新渲染zone
                ClearZone();

                if (zoneVideos.Count > 0)
                {
                    zoneVideos.ForEachDo(zone =>
                    {
                        if (zone.VideoName == selectCameraItem.Name)
                        {
                            zone.IsActive = true;
                            zone.Shape.Fill = Brushes.Transparent;
                            view.drawAreaCanvas.Children.Add(zone.Shape);
                            var layer = AdornerLayer.GetAdornerLayer(zone.Shape);
                            var color = Brushes.Black;
                            layer.Add(new InperAdorner(zone.Shape, zone.Shape.Name, color, 0, -15, false));
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Format_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                view.framrate.SelectedIndex = 0;
                ClearZone();
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void FramerateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (view.framrate.SelectedItem != null && selectCameraItem != null)
                {
                    selectCameraItem.Reset(new CameraParamSet((view.framrate.SelectedItem as CameraCapabilyItem).Size, (view.framrate.SelectedItem as CameraCapabilyItem).FrameRate, (view.framrate.SelectedItem as CameraCapabilyItem).Format));
                    ClearZone();
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
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
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
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
                        //对应zone取消激活
                        if (zoneVideos.Count(x => x.VideoName == camera_active.Name) > 0)
                        {
                            zoneVideos.ForEachDo(x =>
                            {
                                if (x.VideoName == camera_active.Name)
                                {
                                    x.IsActive = false;
                                    x.IsActiveVideo = false;
                                }
                            });
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

                        //对应zone激活
                        if (zoneVideos.Count(x => x.VideoName == camera.Name) > 0)
                        {
                            zoneVideos.ForEachDo(x =>
                            {
                                if (x.VideoName == camera.Name)
                                {
                                    x.IsActive = true;
                                    x.IsActiveVideo = true;
                                    x.VideoShowName = camera.CustomName;
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
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
                if (tb.Text.Length > 15)
                {
                    tb.Text = tb.Text.Substring(0, 15);
                    tb.SelectionStart = tb.Text.Length;
                    return;
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }

        #region zone
        private Shape currentDashShape = null;
        private Shape currentShape = null;
        private Shape mouseDownShape = null;
        private List<Shape> drawShapes = new List<Shape>();
        private bool isAddShape = false;
        private Point startPoint;
        private Point appauatusPoint;
        private void ClearZone()
        {
            currentDashShape = null;
            currentShape = null;
            mouseDownShape = null;
            drawShapes = new List<Shape>();
            isAddShape = false;
            startPoint = new Point();
            appauatusPoint = new Point();
            _shapeDashEnum = ShapeDashEnum.None;
            _shapeEnum = ShapeEnum.None;
            view.drawAreaCanvas.Children.Clear();
            //if (zoneVideos.Count(x => x.IsActive == false) > 0)
            //{
            //    zoneVideos.RemoveWhere(x => x.IsActive == false);
            //}
        }
        public void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                isAddShape = true;
                startPoint = e.GetPosition(sender as Canvas);
                if (_shapeDashEnum != ShapeDashEnum.None && currentDashShape == null)//如果选中了Apparatus 去画Apparatus
                {
                    appauatusPoint = startPoint;
                    if (drawShapes.Count == 0)
                    {
                        switch (_shapeDashEnum)
                        {
                            case ShapeDashEnum.RectangleDash:
                                currentShape = DrawRectShape(Brushes.Red, true);
                                break;
                            case ShapeDashEnum.EllipseDash:
                                break;
                            case ShapeDashEnum.PathDash:
                                break;
                        }
                        currentDashShape = currentShape;
                        if (!_shapeDashEnum.ToString().Equals(ShapeDashEnum.PathDash.ToString()))
                        {
                            Canvas.SetLeft(currentShape, startPoint.X);
                            Canvas.SetTop(currentShape, startPoint.Y);
                        }
                    }
                }
                else //如果没有直接画Zone
                {
                    if (_shapeEnum != ShapeEnum.None && startPoint.X > appauatusPoint.X && startPoint.Y > appauatusPoint.Y)
                    {
                        switch (_shapeEnum)
                        {
                            case ShapeEnum.Rectangle:
                                currentShape = DrawRectShape(Brushes.Black, false);
                                break;
                            case ShapeEnum.Ellipse:
                                break;
                            case ShapeEnum.Path:
                                break;
                        }
                        if (!_shapeEnum.ToString().Equals(ShapeEnum.Path.ToString()))
                        {
                            Canvas.SetLeft(currentShape, startPoint.X);
                            Canvas.SetTop(currentShape, startPoint.Y);
                        }
                        drawShapes.Add(currentShape);
                    }
                }
                if (currentShape != null)
                {
                    view.drawAreaCanvas.Children.Add(currentShape);
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (isAddShape && currentShape != null)
                {
                    var layer = AdornerLayer.GetAdornerLayer(currentShape);
                    var color = (currentDashShape != null && drawShapes.Count == 0) ? Brushes.Red : Brushes.Black;
                    layer.Add(new InperAdorner(currentShape, currentShape.Name, color, 0, -15, false));
                    currentShape = null;
                }
                isAddShape = false; startPoint = new Point(); _shapeEnum = ShapeEnum.None; _shapeDashEnum = ShapeDashEnum.None;
                ShapeColorRest("#929292");
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Image_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (isAddShape && currentShape != null)
                {
                    var movePoint = e.GetPosition(sender as Canvas);
                    if (!currentShape.Name.StartsWith("P"))
                    {
                        double currentLeft = Canvas.GetLeft(currentShape), currentTop = Canvas.GetTop(currentShape);
                        //限制区域在apparatus里面
                        if (currentDashShape != null && drawShapes.Count != 0)
                        {
                            if (movePoint.X - startPoint.X >= currentDashShape.Width - startPoint.X + appauatusPoint.X || movePoint.Y - startPoint.Y >= currentDashShape.Height - startPoint.Y + appauatusPoint.Y)
                            {
                                return;
                            }
                        }
                        if (view.drawAreaCanvas.Width - currentLeft < Math.Abs(movePoint.Y - startPoint.Y))
                        {
                            return;
                        }
                        if (Math.Abs(movePoint.Y - startPoint.Y) > 5)
                        {
                            currentShape.Width = Math.Abs(movePoint.X - startPoint.X);
                            currentShape.Height = Math.Abs(movePoint.Y - startPoint.Y);

                            if (movePoint.Y - startPoint.Y < 0)
                            {
                                Canvas.SetTop(currentShape, startPoint.Y - Math.Abs(movePoint.Y - startPoint.Y));
                            }
                            if (movePoint.X - startPoint.X < 0)
                            {
                                Canvas.SetLeft(currentShape, startPoint.X - Math.Abs(movePoint.X - startPoint.X));
                            }
                        }
                    }
                    else
                    {
                        (currentShape as Polygon).Points.Add(movePoint);
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                if (isAddShape)
                {
                    Image_MouseLeftButtonUp(null, null);
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        #region draw shape
        private Shape DrawRectShape(Brush brush, bool isDash = false)
        {
            try
            {
                var rect = new Rectangle()
                {
                    Width = 100,
                    Height = 100,
                    StrokeThickness = 1,
                    Fill = Brushes.Transparent,
                    Cursor = Cursors.Hand,
                    Stroke = brush,
                    Name = "Z" + (drawShapes.Count + 1),
                    //Name = "Z" + (zoneVideos.Count == 0 ? (zoneVideos.Count + 1) : int.Parse(drawShapes.Last().Name.Substring(1, drawShapes.Last().Name.Length - 1)) + 1),
                };
                if (isDash)
                {
                    rect.Stroke = brush;
                    rect.Name = "Apparatus";
                    rect.StrokeDashArray = new DoubleCollection(new List<double>() { 1, 1 });
                    rect.Cursor = Cursors.Arrow;
                }
                else
                {
                    rect.MouseDown += (o, e) =>
                    {
                        var _shape = o as Shape;
                        _shape.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2270D95F"));
                        mouseDownShape = _shape;

                        drawShapes.ForEach(shape =>
                        {
                            if (shape.Name != _shape.Name)
                            {
                                shape.Fill = Brushes.Transparent;
                            }
                        });
                        e.Handled = true;
                    };
                }
                AddMenuitem(rect);
                return rect;
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
            return null;
        }
        private void AddMenuitem(Shape shape)
        {
            // 创建一个 ContextMenu 对象
            ContextMenu contextMenu = new ContextMenu();

            // 创建一个 MenuItem 对象
            MenuItem menuItem = new MenuItem
            {
                Header = "Delete",
                Name = shape.Name,
                Width = 100
            };
            // 将 MenuItem 添加到 ContextMenu 中
            contextMenu.Items.Add(menuItem);

            // 将 ContextMenu 赋值给 Rectangle 的 ContextMenu 属性
            shape.ContextMenu = contextMenu;

            //添加点击事件
            menuItem.Click += (o, e) =>
            {
                var item = o as MenuItem;
                if (item.Name.StartsWith("App"))
                {
                    if (drawShapes.Count > 0)
                    {
                        return;
                    }
                    if (currentDashShape != null)
                    {
                        view.drawAreaCanvas.Children.Remove(currentDashShape);
                        currentDashShape = null;
                        appauatusPoint = new Point();
                    }
                }
                else
                {
                    var shape1 = drawShapes.First(x => x.Name == item.Name);
                    if (shape1 != null)
                    {
                        view.drawAreaCanvas.Children.Remove(shape1);
                        drawShapes.Remove(shape1);
                        //重置被选中的shape为null
                        if (drawShapes.Count == 0)
                        {
                            mouseDownShape = null;
                        }
                    }
                    //这里要清除marker配置时的zone
                    if (InperGlobalClass.EventSettings.Channels.Count(x => x.VideoZone.Name == selectCameraItem.Name) > 0)
                    {
                        if (InperGlobalClass.EventSettings.Channels.FirstOrDefault(f => f.VideoZone.Name == selectCameraItem.Name) is EventChannelJson channelJson)
                        {
                            channelJson.VideoZone.AllZoneConditions.RemoveWhere(n => n.ZoneName.StartsWith(shape1.Name));
                        }
                    }
                }
            };

        }
        #endregion

        #region zone dash  RectangleDash  EllipseDash  PathDash
        private ShapeDashEnum _shapeDashEnum = ShapeDashEnum.None;
        private ShapeEnum _shapeEnum = ShapeEnum.None;
        public void AddZone()
        {
            try
            {
                if (mouseDownShape != null)
                {
                    if (zoneVideos.Count(x => x.Name == mouseDownShape.Name) < 1)
                    {
                        zoneVideos.Add(new ZoneVideo()
                        {
                            Name = mouseDownShape.Name,
                            Shape = mouseDownShape,
                            DisplayName = "Zone",
                            VideoShowName = selectCameraItem.CustomName,
                            VideoName = selectCameraItem.Name
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void ShapeDashDraw(string type)
        {
            try
            {
                switch (type)
                {
                    case "RectangleDash":
                        _shapeDashEnum = ShapeDashEnum.RectangleDash;
                        break;
                    case "EllipseDash":
                        _shapeDashEnum = ShapeDashEnum.EllipseDash;
                        break;
                    case "PathDash":
                        _shapeDashEnum = ShapeDashEnum.PathDash;
                        break;
                }
                ShapeDashColorChange(_shapeDashEnum, "#ED8F26");
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void ShapeDraw(string type)
        {
            try
            {
                switch (type)
                {
                    case "Rectangle":
                        _shapeEnum = ShapeEnum.Rectangle;
                        break;
                    case "Ellipse":
                        _shapeEnum = ShapeEnum.Ellipse;
                        break;
                    case "Path":
                        _shapeEnum = ShapeEnum.Path;
                        break;
                }
                ShapeColorChange(_shapeEnum, "#ED8F26");
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        private void ShapeDashColorChange(ShapeDashEnum shapeDashEnum, string color)
        {
            try
            {
                switch (shapeDashEnum.ToString())
                {
                    case "RectangleDash":
                        view._rectDash.Stroke = (Brush)new BrushConverter().ConvertFrom(color);
                        break;
                    case "EllipseDash":
                        view._ellipseDash.Stroke = (Brush)new BrushConverter().ConvertFrom(color);
                        break;
                    case "PathDash":
                        view._pathDash.Stroke = (Brush)new BrushConverter().ConvertFrom(color);
                        break;
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        private void ShapeDashColorRest(string color)
        {
            view._rectDash.Stroke = (Brush)new BrushConverter().ConvertFrom(color);
            view._ellipseDash.Stroke = (Brush)new BrushConverter().ConvertFrom(color);
            view._pathDash.Stroke = (Brush)new BrushConverter().ConvertFrom(color);
            _shapeDashEnum = ShapeDashEnum.None;
        }
        private void ShapeColorChange(ShapeEnum shapeEnum, string color)
        {
            try
            {
                switch (shapeEnum.ToString())
                {
                    case "Rectangle":
                        view._rect.Stroke = (Brush)new BrushConverter().ConvertFrom(color);
                        break;
                    case "Ellipse":
                        view._ellipse.Stroke = (Brush)new BrushConverter().ConvertFrom(color);
                        break;
                    case "Path":
                        view._path.Stroke = (Brush)new BrushConverter().ConvertFrom(color);
                        break;
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        private void ShapeColorRest(string color)
        {
            view._rect.Stroke = (Brush)new BrushConverter().ConvertFrom(color);
            view._ellipse.Stroke = (Brush)new BrushConverter().ConvertFrom(color);
            view._path.Stroke = (Brush)new BrushConverter().ConvertFrom(color);
            _shapeEnum = ShapeEnum.None;
        }
        #endregion

        #endregion

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
                            if (double.Parse(tb.Text) < 0 || double.Parse(tb.Text) > 23)
                            {
                                //Growl.Warning("Incorrect input time", "SuccessMsg");
                                //tb.Text = DateTime.Now.Hour.ToString();
                                tb.Foreground = Brushes.Red;
                            }
                            break;
                        case "startMinutes":
                            if (AdditionRecordStart.AtTime.Hours == DateTime.Now.Hour)
                            {
                                if (double.Parse(tb.Text) < 0 || double.Parse(tb.Text) > 59)
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
                                if (double.Parse(tb.Text) < 0 || double.Parse(tb.Text) > 59)
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
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
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
                if (InperGlobalClass.EventSettings.Channels.Count(x => x.Type == ChannelTypeEnum.TriggerStart.ToString()) > 0 && InperGlobalClass.AdditionRecordConditionsStart != AdditionRecordConditionsTypeEnum.Trigger)
                {
                    InperGlobalClass.EventSettings.Channels.RemoveWhere(x => x.Type == ChannelTypeEnum.TriggerStart.ToString());
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
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
                if (InperGlobalClass.EventSettings.Channels.Count(x => x.Type == ChannelTypeEnum.TriggerStop.ToString()) > 0 && InperGlobalClass.AdditionRecordConditionsStop != AdditionRecordConditionsTypeEnum.Trigger)
                {
                    InperGlobalClass.EventSettings.Channels.RemoveWhere(x => x.Type == ChannelTypeEnum.TriggerStop.ToString());
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        #endregion
    }
}
