using Accord.Video.DirectShow;
using InperCameraSolution.AccordSolution;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Data.Model;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Lib.Helper.JsonBean;
using log4net.Repository.Hierarchy;
using OpenCvSharp;
using Stylet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using UnityEngine;

namespace InperStudio.Lib.Bean
{
    public class VideoRecordBean : PropertyChangedBase
    {
        public string MonikerString { get; set; }

        public bool IsActive { get; set; }
        public string Name { get; set; }
        public string CustomName { get; set; }
        private BitmapSource _writeableBitmap;
        public BitmapSource WriteableBitmap
        {
            get => _writeableBitmap;
            private set => SetAndNotify(ref _writeableBitmap, value);
        }
        private bool _autoRecord;
        public bool AutoRecord
        {
            get => _autoRecord;
            set => SetAndNotify(ref _autoRecord, value);
        }
        private long _time;
        public long Time
        {
            get => _time;
            set => SetAndNotify(ref _time, value);
        }
        private int? _writeFps = null;
        public int? WriteFps
        {
            get => _writeFps;
            set => SetAndNotify(ref _writeFps, value);
        }
        private List<AccordResolutionInfo> capabilyItems = new List<AccordResolutionInfo>();
        public List<AccordResolutionInfo> CapabilyItems
        {
            get => capabilyItems;
            set => SetAndNotify(ref capabilyItems, value);
        }
        public bool IsCanOpen { get; set; } = true;
        public bool IsTracking { get; set; } = false;
        #region private
        private AccordCamraSetting accordCamraSetting;
        #endregion

        #region methods
        public VideoRecordBean(FilterInfo filterInfo)
        {
            MonikerString = filterInfo.MonikerString;
            Name = filterInfo.Name;
            accordCamraSetting = new AccordCamraSetting(filterInfo);
            accordCamraSetting.OnInfoReceived += AccordCamraSetting_OnInfoReceived;
            CapabilyItems = accordCamraSetting.AccordResolutionInfos;
        }
        private void AccordCamraSetting_OnInfoReceived(AccordImageInfo info)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                //Create a Mat object with the correct size and data type
                Mat mat = new Mat(info.Height, info.Width, info.CameraChannel == 3 ? MatType.CV_8UC3 : MatType.CV_8UC1);  // Assuming a 3-channel color image (BGR)
                // Copy the data into the Mat object
                Marshal.Copy(info.Datas, 0, mat.Data, info.Datas.Length);

                if (IsTracking)
                {
                    DrawTrackingRect(mat);
                }
                if (_DrawTracking != null)
                {
                    OpenCvSharp.Rect rect = new OpenCvSharp.Rect(_DrawTracking.Left, _DrawTracking.Top, _DrawTracking.Width, _DrawTracking.Height);
                    // 绘制矩形
                    Cv2.Rectangle(mat, rect, Scalar.Blue, 2); // 使用红色线条绘制矩形，线宽为2 // 绘制中心点 //画点和区域

                    Cv2.Circle(mat, _DrawTracking.Left + _DrawTracking.Width / 2, _DrawTracking.Top + _DrawTracking.Height / 2, 2, Scalar.Blue, -1); // 使用蓝色填充圆形，半径为5  //存储轨迹数据
                }
                WriteableBitmap = BitmapSource.Create(info.Width, info.Height, 96, 96, info.CameraChannel == 3 ? PixelFormats.Bgr24 : PixelFormats.Gray8, null, info.Datas, info.CameraChannel == 3 ? info.Width * 3 : info.Width);

            }));
        }

        ///轨迹追踪数据
        private ConcurrentQueue<Tracking> trackingsQueue;
        private CancellationTokenSource _cts;
        private List<EventChannelJson> _trackingEventChannelJson = new List<EventChannelJson>();
        private List<EventChannelJson> _trackingEventChannelJsonOutput = new List<EventChannelJson>();
        ///string ==zoneName,channeltypeenum==状态
        private Dictionary<string, ChannelTypeEnum> _trackingMarkerStatu = new Dictionary<string, ChannelTypeEnum>();
        private int _trackingLock = 0;
        private Tracking _DrawTracking;
        private async void DrawTrackingRect(Mat mat)
        {
            try
            {
                if (Interlocked.Exchange(ref _trackingLock, 1) == 0)
                {
                    await Task.Run(() =>
                    {
                        var tracking = InperTrackingDnnHelper.Detect(mat);

                        if (tracking.Count > 0)
                        {
                            for (var i = 0; i < tracking.Count; i++)
                            {
                                if (InperGlobalClass.IsRecord)
                                {
                                    trackingsQueue.Enqueue(tracking[i]);
                                }
                                _DrawTracking = tracking[i];
                            }
                        }
                        Interlocked.Exchange(ref _trackingLock, 0);
                    });
                }
            }
            catch (Exception e)
            {
                InperLogExtentHelper.LogExtent(e, "VideoRecordBean");
            }
        }
        private void StartTracking()
        {
            if (IsTracking)
            {
                _cts = new CancellationTokenSource();
                _trackingEventChannelJson = InperGlobalClass.EventSettings.Channels.Where(x => x.Type == ChannelTypeEnum.Enter.ToString() || x.Type == ChannelTypeEnum.Leave.ToString() || x.Type == ChannelTypeEnum.EnterOrLeave.ToString() || x.Type == ChannelTypeEnum.Stay.ToString()).ToList();
                _trackingEventChannelJsonOutput = InperGlobalClass.EventSettings.Channels.Where(x => x.Type == ChannelTypeEnum.Output.ToString() && (x.Condition.Type == ChannelTypeEnum.Enter.ToString() || x.Condition.Type == ChannelTypeEnum.Leave.ToString() || x.Condition.Type == ChannelTypeEnum.EnterOrLeave.ToString() || x.Condition.Type == ChannelTypeEnum.Stay.ToString())).ToList();
                _trackingMarkerStatu.Clear();
                _trackingEventChannelJson.ForEach(x =>
                {
                    _trackingMarkerStatu.Add(x.Name, ChannelTypeEnum.None);
                });
                _trackingEventChannelJsonOutput.ForEach(x =>
                {
                    _trackingMarkerStatu.Add(x.Condition.Name, ChannelTypeEnum.None);
                });
                trackingsQueue = new ConcurrentQueue<Tracking>();
                _ = StoreTrackingDataAsync(_cts.Token);
            }
        }
        private void StopTracking()
        {
            if (IsTracking)
            {
                _cts?.Cancel();
            }
        }
        private async Task StoreTrackingDataAsync(CancellationToken cancellationToken)
        {
            List<Tracking> trackingsToInsert = new List<Tracking>();
            int batchSize = 100; // 根据实际需求调整批量大小

            while (!cancellationToken.IsCancellationRequested)
            {
                if (trackingsQueue.TryDequeue(out Tracking tracking))
                {

                    TrackingToMarker(tracking);

                    trackingsToInsert.Add(tracking);

                    if (trackingsToInsert.Count >= batchSize)
                    {
                        await Task.Run(() =>
                        {
                            // 执行批量插入操作
                            App.SqlDataInit.sqlSugar.Insertable(trackingsToInsert).ExecuteCommand();
                        });

                        trackingsToInsert.Clear();
                    }
                }
                else
                {
                    await Task.Delay(10); // 避免无限循环，减轻CPU负担
                }
            }

            // 处理剩余的轨迹数据
            if (trackingsToInsert.Count > 0)
            {
                await Task.Run(() =>
                {
                    //InsertTrackingsToDatabase(trackingsToInsert);
                    App.SqlDataInit.sqlSugar.Insertable(trackingsToInsert).ExecuteCommand();
                });
            }
        }
        private void TrackingToMarker(Tracking tracking)
        {
            try
            {
                var point = new System.Windows.Point(tracking.CenterX * (480 / WriteableBitmap.Width), tracking.CenterY * (320 / WriteableBitmap.Height));
                if (_trackingEventChannelJsonOutput.Any())
                {
                    _trackingEventChannelJsonOutput.ForEach(x =>
                    {
                        bool isDrawMarker = false;
                        if (x.Condition.VideoZone.AllZoneConditions.First(z => z.ZoneName == x.Condition.SymbolName) is ZoneConditions zone)
                        {
                            var rect = new System.Windows.Rect(zone.ShapeLeft, zone.ShapeTop, zone.ShapeWidth, zone.ShapeHeight);
                            bool re = TrackingStateSet(rect.Contains(point), x.Condition.Name, zone.Timer, _trackingMarkerStatu[x.Name] == ChannelTypeEnum.Leave);

                            if (zone.Type == _trackingMarkerStatu[x.Name].ToString())
                            {
                                if (!zone.Timer.Enabled)
                                {
                                    if (zone.IsDelay || x.Type == ChannelTypeEnum.Stay.ToString())
                                    {
                                        zone.Timer.Start();
                                        zone.Timer.Interval = zone.IsDelay ? zone.DelaySeconds * 1000 : zone.Duration * 1000;
                                    }
                                    else
                                    {
                                        zone.IsTimerSignal = true;
                                    }
                                }
                                if (zone.IsTimerSignal)
                                {
                                    zone.IsTimerSignal = false;
                                    isDrawMarker = true;
                                }

                            }
                            else
                            {
                                if ((re && x.Type == ChannelTypeEnum.Leave.ToString()) || zone.IsTimerSignal) { isDrawMarker = true; zone.IsTimerSignal = false; }
                            }
                        }
                        if (isDrawMarker)
                        {
                            InperDeviceHelper.Instance.SetMarkers(new BaseMarker()
                            {
                                Color = x.BgColor,
                                IsIgnore = false,
                                CameraTime = InperDeviceHelper.Instance.time / 100,
                                ChannelId = x.ChannelId,
                                CreateTime = DateTime.Now,
                                Name = x.Name,
                                Type = x.Type
                            });
                        }
                    });
                }
                if (_trackingEventChannelJson.Any())
                {
                    _trackingEventChannelJson.ForEach(x =>
                    {
                        bool isDrawMarker = false;
                        //每个Video下的zone
                        x.VideoZone.AllZoneConditions.ForEach(condition =>
                        {
                            if (condition.ZoneName == x.SymbolName)
                            {
                                var rect = new System.Windows.Rect(condition.ShapeLeft, condition.ShapeTop, condition.ShapeWidth, condition.ShapeHeight);
                                bool re = TrackingStateSet(rect.Contains(point), x.Name, condition.Timer, _trackingMarkerStatu[x.Name] == ChannelTypeEnum.Leave);

                                if (x.Type == _trackingMarkerStatu[x.Name].ToString())
                                {
                                    if (!condition.Timer.Enabled)
                                    {
                                        if (condition.IsDelay || x.Type == ChannelTypeEnum.Stay.ToString())
                                        {
                                            condition.Timer.Start();
                                            condition.Timer.Interval = condition.IsDelay ? condition.DelaySeconds * 1000 : condition.Duration * 1000;
                                        }
                                        else
                                        {
                                            condition.IsTimerSignal = true;
                                        }
                                    }
                                    if (condition.IsTimerSignal)
                                    {
                                        condition.IsTimerSignal = false;
                                        isDrawMarker = true;
                                    }
                                }
                                else
                                {
                                    if ((re && x.Type == ChannelTypeEnum.Leave.ToString()) || condition.IsTimerSignal) { isDrawMarker = true; condition.IsTimerSignal = false; }
                                }
                            }
                        });

                        if (isDrawMarker)
                        {
                            InperDeviceHelper.Instance.SetMarkers(new BaseMarker()
                            {
                                Color = x.BgColor,
                                IsIgnore = true,
                                CameraTime = InperDeviceHelper.Instance.time / 100,
                                ChannelId = x.ChannelId,
                                CreateTime = DateTime.Now,
                                Name = x.Name,
                                Type = x.Type
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private bool TrackingStateSet(bool isTrue, string name, System.Timers.Timer timer, bool isLeave = false)
        {
            if (isTrue)
            {
                if (_trackingMarkerStatu[name] != ChannelTypeEnum.Enter && _trackingMarkerStatu[name] != ChannelTypeEnum.Stay)
                {
                    _trackingMarkerStatu[name] = ChannelTypeEnum.Enter;
                }
                else
                {
                    _trackingMarkerStatu[name] = ChannelTypeEnum.Stay;
                }
            }
            else
            {
                if (_trackingMarkerStatu[name] == ChannelTypeEnum.Enter || _trackingMarkerStatu[name] == ChannelTypeEnum.Stay)
                {
                    _trackingMarkerStatu[name] = ChannelTypeEnum.Leave;
                }
                else
                {
                    //如果定时器没有在执行，就设置状态为none
                    if (!timer.Enabled)
                    {
                        if (isLeave)
                        {
                            _trackingMarkerStatu[name] = ChannelTypeEnum.None;
                            return true;
                        }
                        _trackingMarkerStatu[name] = ChannelTypeEnum.None;
                    }
                }
            }
            return false;
        }
        public void Reset(AccordResolutionInfo resolutionInfo)
        {
            try
            {
                accordCamraSetting.Stop();
                var resol = accordCamraSetting.VideoCapabilities.FirstOrDefault(x => x.FrameSize == resolutionInfo.FrameSize);
                accordCamraSetting.SetResoulation(resol);
                accordCamraSetting.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public void StartCapture()
        {
            accordCamraSetting.Start();
        }
        public void StartRecording(string path = null)
        {
            accordCamraSetting.Stop();
            accordCamraSetting.SetWritePath(path);
            accordCamraSetting.Start();
            accordCamraSetting.IsRecord = true;

            StartTracking();
        }
        public void Stop()
        {
            accordCamraSetting.Stop();
            accordCamraSetting.IsRecord = false;
            StopTracking();
        }
        #endregion
    }
}