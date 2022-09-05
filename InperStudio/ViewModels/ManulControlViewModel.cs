using HandyControl.Controls;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Data.Model;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using InperStudio.Views.Control;
using Stylet;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace InperStudio.ViewModels
{
    public class ManulControlViewModel : Stylet.Screen
    {
        #region properties
        private readonly IWindowManager windowManager;
        public MainWindowViewModel MainWindowViewModel { get; private set; }

        private Sprite sprite = null;
        #endregion
        public ManulControlViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
            MainWindowViewModel = new MainWindowViewModel();
        }
        protected override void OnViewLoaded()
        {
            //InperDeviceHelper.Instance.WaveInitEvent += (s, e) =>
            //{
            //    if (e)
            //    {
            //        _ = View.Dispatcher.BeginInvoke(new Action(() =>
            //          {
            //              if (InperClassHelper.GetWindowByNameChar("Camera Signal Settings") == null)
            //              {
            //                  windowManager.ShowWindow(new SignalSettingsViewModel(SignalSettingsTypeEnum.Camera));
            //              }
            //          }));
            //    }
            //    else
            //    {
            //        Growl.Error(new GrowlInfo() { Message = "设备初始化出现问题", Token = "SuccessMsg", WaitTime = 1 });
            //    }
            //};
        }
        #region
        public void DataPathShow(string type)
        {
            try
            {
                switch (type)
                {
                    case "Path":
                        //windowManager.ShowDialog(new DataPathConfigViewModel(DataConfigPathTypeEnum.Path));
                        break;
                    case "Load":
                        //DefalutFileLoad();
                        //windowManager.ShowDialog(new DataPathConfigViewModel(DataConfigPathTypeEnum.Load));
                        break;
                    case "Save":
                        //JsonConfigSaveAs();
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
                        else
                        {
                            InperClassHelper.GetWindowByNameChar("Camera Signal Settings").Activate();
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
                    case "Note":
                        if (InperClassHelper.GetWindowByNameChar("Note") == null)
                        {
                            windowManager.ShowWindow(new NoteSettingViewModel());
                        }
                        else
                        {
                            InperClassHelper.GetWindowByNameChar("Note").Activate();
                        }
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
        private async void PreviewRecord()
        {
            InperDeviceHelper.Instance.EventChannelChart.RenderableSeries.Clear();
            InperDeviceHelper.Instance.EventChannelChart.Annotations.Clear();
            InperDeviceHelper.Instance.device.Start();

            if (InperDeviceHelper.Instance.CameraChannels.Count <= 0)
            {
                InperGlobalClass.ShowReminderInfo("Please add at least one data channel");
                return;
            }
            if (!InperDeviceHelper.Instance.InitDataStruct())
            {
                InperGlobalClass.ShowReminderInfo("数据初始化失败");
                return;
            }
            if (!InperDeviceHelper.Instance.AllLightOpen())
            {
                InperGlobalClass.ShowReminderInfo("Please select at least one light");
                return;
            }

            InperDeviceHelper.Instance.StartCollect();
            await StartAndStopShowMarker(ChannelTypeEnum.Start);

            InperGlobalClass.IsPreview = true;
            InperGlobalClass.IsRecord = false;
            InperGlobalClass.IsStop = false;

            ((((View as ManulControlView).Parent as ContentControl).DataContext as MainWindowViewModel).ActiveItem as DataShowControlViewModel).SciScrollSet();
            InperGlobalClass.IsAllowDragScroll = true;
        }
        private async void StartRecord()
        {
            try
            {
                if (InperGlobalClass.IsPreview)
                {
                    StopRecord(1);
                }
                InperDeviceHelper.Instance.EventChannelChart.RenderableSeries.Clear();
                InperDeviceHelper.Instance.EventChannelChart.Annotations.Clear();
                if (!InperDeviceHelper.Instance.AllLightOpen())
                {
                    InperGlobalClass.ShowReminderInfo("Please select at least one light");
                    return;
                }
                InperDeviceHelper.Instance.device.Start();

                if (InperDeviceHelper.Instance.CameraChannels.Count <= 0)
                {
                    InperGlobalClass.ShowReminderInfo("Please add at least one data channel");
                    return;
                }
                if (!InperDeviceHelper.Instance.InitDataStruct())
                {
                    InperGlobalClass.ShowReminderInfo("数据初始化失败");
                    return;
                }
                if (!Directory.Exists(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName)))
                {
                    _ = Directory.CreateDirectory(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName));
                }
                File.Create(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName, "inper.ipd")).Close();

                Task task = StartTriggerStrategy();
                if (task != null)
                {
                    await task;
                }

                //数据库优先初始化
                App.SqlDataInit = new Lib.Data.SqlDataInit();

                if (DataPathConfigViewModel.DataList.Count > 0)
                {
                    DataPathConfigViewModel.DataList.ToList().ForEach(x =>
                    {
                        App.SqlDataInit.sqlSugar.Insertable(x).ExecuteCommand();
                    });
                }

                InperDeviceHelper.Instance.StartCollect();

                foreach (var item in InperGlobalClass.ActiveVideos)
                {
                    if (item.IsActive && item.AutoRecord)
                    {
                        item.StartRecording(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName, DateTime.Now.ToString("HHmmss") + "_" + item.CustomName));
                    }
                }

                InperGlobalClass.IsRecord = true;
                InperGlobalClass.IsPreview = true;
                InperGlobalClass.IsStop = false;
                (this.View as ManulControlView).Root_Gird.IsEnabled = true;

                await StartAndStopShowMarker(ChannelTypeEnum.Start);
                ((((View as ManulControlView).Parent as ContentControl).DataContext as MainWindowViewModel).ActiveItem as DataShowControlViewModel).SciScrollSet();
                InperGlobalClass.IsAllowDragScroll = true;

                if (InperGlobalClass.AdditionRecordConditionsStop == AdditionRecordConditionsTypeEnum.AtTime)
                {
                    var obj = InperJsonHelper.GetAdditionRecordJson("stop");
                    _ = Task.Factory.StartNew(() =>
                      {
                          TimeSpan time = new TimeSpan(obj.AtTime.Hours, obj.AtTime.Minutes, obj.AtTime.Seconds);
                          while (true)
                          {
                              var currentTime = TimeSpan.Parse(DateTime.Now.ToLongTimeString()).Ticks;
                              if (currentTime >= time.Ticks)
                              {
                                  break;
                              }
                              Task.Delay(10);
                          }
                          this.View.Dispatcher.Invoke(() => { StopRecord(); });
                      });
                }

                sprite = Sprite.Show(new HeartbeatContrrol());

            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        private async void StopRecord(int type = 0)
        {
            try
            {
                bool isrecord = false;
                if (InperGlobalClass.IsRecord)
                {
                    Task task = StopTriggerStrategy();
                    if (task != null)
                    {
                        await task;
                    }
                    isrecord = true;
                    if (sprite != null)
                    {
                        sprite.Close();
                        sprite = null;
                    }
                }
                if (type == 0)
                {
                    await StartAndStopShowMarker(ChannelTypeEnum.Stop);
                }
                InperDeviceHelper.Instance.device.Stop();
                InperGlobalClass.IsRecord = false;
                InperGlobalClass.IsPreview = false;
                InperGlobalClass.IsStop = true;

                (View as ManulControlView).Root_Gird.IsEnabled = true;

                InperDeviceHelper.Instance.StopPlot();

                if (isrecord)
                {
                    if (NoteSettingViewModel.NotesCache.Count > 0)
                    {
                        NoteSettingViewModel.NotesCache.ForEach(x =>
                        {
                            App.SqlDataInit.sqlSugar.Insertable(x).ExecuteCommand();
                        });
                        NoteSettingViewModel.NotesCache.Clear();
                    }
                    DataPathConfigViewModel.DataList.Clear();

                    foreach (VideoRecordBean item in InperGlobalClass.ActiveVideos)
                    {
                        if (item.IsActive && item.AutoRecord)
                        {
                            item.StopRecording();
                        }
                    }
                    Dialog d = Dialog.Show<ProgressDialog>("MainDialog");
                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                    await Task.Factory.StartNew(() => { InperDeviceHelper.Instance.StopCollect(tokenSource); }, tokenSource.Token);
                    await Task.Delay(1000);
                    d.Close();

                    isrecord = false;
                }

                if (Directory.GetDirectories(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName)).Length > 0 || Directory.GetFiles(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName)).Length > 0)
                {
                    InperGlobalClass.DataFolderName = DateTime.Now.ToString("yyyyMMddHHmmss");
                    if (!Directory.Exists(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName)))
                    {
                        _ = Directory.CreateDirectory(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName));
                    }
                }

                InperDeviceHelper.Instance.AllLightClose();
                //System.Windows.MessageBox.Show("收到点数：" + InperDeviceHelper.Instance.count + "存储收到点数：" + InperDeviceHelper.Instance.count1 + "记录时长：" + seconds);
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
            finally
            {
                InperGlobalClass.IsRecord = false;
                InperGlobalClass.IsPreview = false;
                InperGlobalClass.IsStop = true;
            }

        }
        private Task StartAndStopShowMarker(ChannelTypeEnum typeEnum)
        {

            return Task.Factory.StartNew(() =>
             {
                 TimeSpan time = new TimeSpan(0);
                 if (InperDeviceHelper.Instance.CameraChannels[0].RenderableSeries.First().DataSeries.XValues.Count > 0)
                 {
                     time = typeEnum == ChannelTypeEnum.Start
                         ? new TimeSpan(0)
                         : (TimeSpan)InperDeviceHelper.Instance.CameraChannels[0].RenderableSeries.First().DataSeries.XValues[InperDeviceHelper.Instance.CameraChannels[0].RenderableSeries.First().DataSeries.XValues.Count - 1];
                 }
                 foreach (var channel in InperGlobalClass.EventSettings.Channels)
                 {
                     if (channel.Type == typeEnum.ToString())
                     {
                         InperDeviceHelper.Instance.SetMarkers(new BaseMarker()
                         {
                             CameraTime = time.Ticks,
                             ChannelId = channel.ChannelId,
                             Color = channel.BgColor,
                             IsIgnore = true,
                             Name = channel.Name,
                             Type = channel.Type,
                             CreateTime = DateTime.Now
                         });
                     }
                     if (channel.Type == ChannelTypeEnum.Output.ToString() && channel.Condition != null)
                     {
                         if (channel.Condition.Type == typeEnum.ToString())
                         {
                             InperDeviceHelper.Instance.SetMarkers(new BaseMarker()
                             {
                                 CameraTime = time.Ticks,
                                 ChannelId = channel.ChannelId,
                                 Color = channel.BgColor,
                                 IsIgnore = false,
                                 ConditionId = channel.Condition.ChannelId,
                                 Name = channel.Name,
                                 Type = channel.Condition.Type,
                                 CreateTime = DateTime.Now
                             });
                         }
                     }
                 }
             });
        }
        private Task StartTriggerStrategy()
        {
            (this.View as ManulControlView).Root_Gird.IsEnabled = false;
            var obj = InperJsonHelper.GetAdditionRecordJson();
            if (InperGlobalClass.AdditionRecordConditionsStart == AdditionRecordConditionsTypeEnum.Delay)
            {
                return Task.Factory.StartNew(() =>
                {
                    InperDeviceHelper.Instance.AllLightClose();
                    DateTime time = DateTime.Now.AddSeconds(obj.Delay.Value);
                    while (true)
                    {
                        if (DateTime.Now.Ticks >= time.Ticks)
                        {
                            break;
                        }
                        Task.Delay(10);
                    }
                    InperDeviceHelper.Instance.AllLightOpen();
                });
            }
            if (InperGlobalClass.AdditionRecordConditionsStart == AdditionRecordConditionsTypeEnum.AtTime)
            {

                return Task.Factory.StartNew(() =>
                {
                    InperDeviceHelper.Instance.AllLightClose();
                    TimeSpan time = new TimeSpan(obj.AtTime.Hours, obj.AtTime.Minutes, obj.AtTime.Seconds);
                    while (true)
                    {
                        var currentTime = TimeSpan.Parse(DateTime.Now.ToLongTimeString()).Ticks;
                        if (currentTime >= time.Ticks)
                        {
                            break;
                        }
                        Task.Delay(10);
                    }
                    InperDeviceHelper.Instance.AllLightOpen();
                });
            }
            if (InperGlobalClass.AdditionRecordConditionsStart == AdditionRecordConditionsTypeEnum.Trigger)
            {
                return Task.Factory.StartNew(() =>
                {
                    InperDeviceHelper.Instance.AllLightClose();

                    if (obj.Trigger.Mode.Equals("Edge"))
                    {

                    }

                    InperDeviceHelper.Instance.AllLightOpen();
                });
            }
            return null;
        }
        private Task StopTriggerStrategy()
        {
            (this.View as ManulControlView).Root_Gird.IsEnabled = false;
            var obj = InperJsonHelper.GetAdditionRecordJson("stop");

            if (InperGlobalClass.AdditionRecordConditionsStop == AdditionRecordConditionsTypeEnum.Delay)
            {
                return Task.Factory.StartNew(() =>
                {
                    DateTime time = DateTime.Now.AddSeconds(obj.Delay.Value);
                    while (true)
                    {
                        if (DateTime.Now.Ticks >= time.Ticks)
                        {
                            break;
                        }
                        _ = Task.Delay(10);
                    }
                });
            }

            return null;
        }
        #endregion
    }
}
