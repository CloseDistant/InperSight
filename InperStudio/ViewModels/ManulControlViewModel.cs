using HandyControl.Controls;
using HandyControl.Data;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Data.Model;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using InperStudio.Views.Control;
using InperStudioControlLib.Lib.Config;
using InperStudioControlLib.Lib.DeviceAgency;
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
using System.Windows.Forms;
using System.Windows.Media;
using Tag = InperStudio.Lib.Data.Model.Tag;

namespace InperStudio.ViewModels
{
    public class ManulControlViewModel : Stylet.Screen
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
        private void PreviewRecord()
        {
            InperDeviceHelper.Instance.EventChannelChart.RenderableSeries.Clear();
            InperDeviceHelper.Instance.EventChannelChart.EventQs.Clear();
            InperDeviceHelper.Instance.EventChannelChart.Annotations.Clear();
            //InperDeviceHelper.Instance.device.Start();

            if (InperDeviceHelper.Instance.CameraChannels.Count <= 0)
            {
                InperGlobalClass.ShowReminderInfo("未配置数据通道");
                return;
            }
            if (!InperDeviceHelper.Instance.InitDataStruct())
            {
                InperGlobalClass.ShowReminderInfo("数据初始化失败");
                return;
            }
            if (!InperDeviceHelper.Instance.AllLightOpen())
            {
                InperGlobalClass.ShowReminderInfo("未设置激发光");
                return;
            }
            InperDeviceHelper.Instance.StartCollect();

            StartAndStopShowMarker(ChannelTypeEnum.Start, 0);

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
                InperDeviceHelper.Instance.EventChannelChart.EventQs.Clear();
                InperDeviceHelper.Instance.EventChannelChart.Annotations.Clear();
                InperDeviceHelper.Instance.device.Start();

                if (InperDeviceHelper.Instance.CameraChannels.Count <= 0)
                {
                    InperGlobalClass.ShowReminderInfo("未配置数据通道");
                    return;
                }
                if (!InperDeviceHelper.Instance.InitDataStruct())
                {
                    InperGlobalClass.ShowReminderInfo("数据初始化失败");
                    return;
                }
                if (!InperDeviceHelper.Instance.AllLightOpen())
                {
                    InperGlobalClass.ShowReminderInfo("未设置激发光");
                    return;
                }
                if (!Directory.Exists(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName)))
                {
                    _ = Directory.CreateDirectory(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName));
                }

                Task task = StartTriggerStrategy();
                if (task != null)
                {
                    await task;
                }

                //数据库优先初始化
                App.SqlDataInit = new Lib.Data.SqlDataInit();

                if (NoteSettingViewModel.NotesCache.Count > 0)
                {
                    App.SqlDataInit.sqlSugar.Insertable<List<Note>>(NoteSettingViewModel.NotesCache).ExecuteCommand();
                }
                if (DataPathConfigViewModel.DataList.Count > 0)
                {
                    DataPathConfigViewModel.DataList.ToList().ForEach(x =>
                    {
                        App.SqlDataInit.sqlSugar.Insertable(x).ExecuteCommand();
                    });
                }

                InperDeviceHelper.Instance.StartCollect();

                _ = Parallel.ForEach(InperGlobalClass.ActiveVideos, item =>
                  {
                      if (item.IsActive && item.AutoRecord)
                      {
                          item.StartRecording(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName, DateTime.Now.ToString("HHmmss") + "_" + item._CamIndex));
                      }
                  });

                InperGlobalClass.IsRecord = true;
                InperGlobalClass.IsPreview = true;
                InperGlobalClass.IsStop = false;
                (this.View as ManulControlView).Root_Gird.IsEnabled = true;


                InperDeviceHelper.Instance.saveDataTask = Task.Factory.StartNew(() => { InperDeviceHelper.Instance.SaveDateProc(); });

                StartAndStopShowMarker(ChannelTypeEnum.Start, 0);
                ((((View as ManulControlView).Parent as ContentControl).DataContext as MainWindowViewModel).ActiveItem as DataShowControlViewModel).SciScrollSet();
                InperGlobalClass.IsAllowDragScroll = true;
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        private async void StopRecord(int type = 0)
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
            }
            InperGlobalClass.IsRecord = false;
            InperGlobalClass.IsPreview = false;
            InperGlobalClass.IsStop = true;
            (View as ManulControlView).Root_Gird.IsEnabled = true;
            if (isrecord)
            {
                NoteSettingViewModel.NotesCache.Clear();
                DataPathConfigViewModel.DataList.Clear();

                _ = Parallel.ForEach(InperGlobalClass.ActiveVideos, item =>
                {
                    if (item.IsActive && item.AutoRecord)
                    {
                        item.StopRecording();
                    }
                });
                Dialog d = Dialog.Show<ProgressDialog>();
                await Task.Delay(3000);
                d.Close();
                if (d.IsClosed == false)
                {
                    d.Close();
                }

                isrecord = false;
            }
            InperDeviceHelper.Instance.StopCollect();
            if (type == 0)
            {
                StartAndStopShowMarker(ChannelTypeEnum.Stop);
            }

            InperGlobalClass.DataFolderName = DateTime.Now.ToString("yyyyMMddHHmmss");

            InperDeviceHelper.Instance.device.Stop();
            //InperDeviceHelper.Instance.AllLightClose();

        }
        private async void StartAndStopShowMarker(ChannelTypeEnum typeEnum, int type = 1)
        {
            await Task.Factory.StartNew(() =>
             {
                 if (InperDeviceHelper.Instance.CameraChannels[0].RenderableSeries.Count > 0)
                 {
                     while (InperDeviceHelper.Instance.CameraChannels[0].RenderableSeries.FirstOrDefault().DataSeries.XValues.Count <= 0)
                     {
                         Task.Delay(100);
                     }
                 }
                 _ = Parallel.ForEach(InperGlobalClass.EventSettings.Channels, channel =>
                   {
                       if (channel.Type == typeEnum.ToString())
                       {
                           InperDeviceHelper.Instance.AddMarkerByHotkeys(channel.ChannelId, channel.Name, (Color)ColorConverter.ConvertFromString(channel.BgColor), type);
                           if (InperGlobalClass.IsRecord)
                           {
                               Manual manual = new Manual()
                               {
                                   ChannelId = channel.ChannelId,
                                   CameraTime = 0,
                                   Color = channel.BgColor,
                                   Name = channel.Name,
                                   Type = channel.Type,
                                   CreateTime = DateTime.Parse(DateTime.Now.ToString("G"))
                               };
                               _ = App.SqlDataInit.sqlSugar.Insertable(manual).ExecuteCommand();
                           }
                       }
                       if (channel.Type == ChannelTypeEnum.Output.ToString() && channel.Condition != null)
                       {
                           if (channel.Condition.Type == typeEnum.ToString())
                           {

                               int count = InperDeviceHelper.Instance.CameraChannels[0].RenderableSeries.First().DataSeries.XValues.Count;
                               TimeSpan time = new TimeSpan();
                               time = typeEnum == ChannelTypeEnum.Start
                               ? (TimeSpan)InperDeviceHelper.Instance.CameraChannels[0].RenderableSeries.First().DataSeries.XValues[0]
                               : (TimeSpan)InperDeviceHelper.Instance.CameraChannels[0].RenderableSeries.First().DataSeries.XValues[count - 1];
                               InperDeviceHelper.Instance.SendCommand(channel);
                               if (InperGlobalClass.IsRecord)
                               {
                                   Output output = new Output()
                                   {
                                       ChannelId = channel.ChannelId,
                                       CameraTime = time.Ticks,
                                       CreateTime = DateTime.Parse(DateTime.Now.ToString("G"))
                                   };
                                   _ = (App.SqlDataInit?.sqlSugar.Insertable(output).ExecuteCommand());
                               }

                           }
                       }
                   });
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
            if (InperGlobalClass.AdditionRecordConditionsStop == AdditionRecordConditionsTypeEnum.AtTime)
            {

                return Task.Factory.StartNew(() =>
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
                });
            }
            return null;
        }
        #endregion
    }
}
