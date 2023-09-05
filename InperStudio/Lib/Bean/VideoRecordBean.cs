using Accord;
using Accord.Video.DirectShow;
using Google.Protobuf.Reflection;
using InperCameraSolution.AccordSolution;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Data.Model;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Lib.Helper.JsonBean;
using log4net.Repository.Hierarchy;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
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
using System.Windows.Controls;
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
        private ImageSource _writeableBitmap;
        public ImageSource WriteableBitmap
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
        public double ActWidth { get; set; }
        public double ActHeight { get; set; }
        private List<AccordResolutionInfo> capabilyItems = new List<AccordResolutionInfo>();
        public List<AccordResolutionInfo> CapabilyItems
        {
            get => capabilyItems;
            set => SetAndNotify(ref capabilyItems, value);
        }
        public bool IsCanOpen { get; set; } = true;
        private bool isTracking = false;
        public bool IsTracking
        {
            get => isTracking;
            set
            {
                SetAndNotify(ref isTracking, value);
                if (value)
                {
                    if (inperTrackingOpenvinoHelper == null)
                    {
                        inperTrackingOpenvinoHelper = new InperTrackingOpenvinoHelper();
                        inperTrackingOpenvinoHelper.LoadModel();
                    }
                }
            }
        }
        #region private
        private AccordCamraSetting accordCamraSetting;

        private InperTrackingOpenvinoHelper inperTrackingOpenvinoHelper;
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
        private ConcurrentQueue<Mat> receivedMats = new ConcurrentQueue<Mat>();
        private CancellationTokenSource trackCancellationToken = new CancellationTokenSource();
        private void AccordCamraSetting_OnInfoReceived(AccordImageInfo info)
        {
            //System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //Create a Mat object with the correct size and data type
            Mat mat = new Mat(info.Height, info.Width, info.CameraChannel == 3 ? MatType.CV_8UC3 : MatType.CV_8UC1);  // Assuming a 3-channel color image (BGR)
                                                                                                                      // Copy the data into the Mat object
            Marshal.Copy(info.Datas, 0, mat.Data, info.Datas.Length);

            receivedMats.Enqueue(mat.Clone());
            //WriteableBitmap = BitmapSource.Create(info.Width, info.Height, 96, 96, info.CameraChannel == 3 ? PixelFormats.Bgr24 : PixelFormats.Gray8, null, info.Datas, info.CameraChannel == 3 ? info.Width * 3 : info.Width);
            //}));
        }
        private int _drawLock = 0;
        private void DrawAndTrack(CancellationToken token)
        {
            if (Interlocked.Exchange(ref _drawLock, 1) == 0)
            {
                Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            Mat mat = null;
                            while (receivedMats.Count > 0)
                            {
                                receivedMats.TryDequeue(out mat);
                            }
                            //receivedMats.TryDequeue(out mat);

                            if (mat != null)
                            {
                                if (IsTracking)
                                {
                                    DrawTrackingRect(mat);
                                }
                                else
                                {
                                    _DrawTracking = null;
                                }

                                if (_DrawTracking != null)
                                {
                                    OpenCvSharp.Rect rect = new OpenCvSharp.Rect(_DrawTracking.Left, _DrawTracking.Top, _DrawTracking.Width, _DrawTracking.Height);
                                    // 绘制矩形
                                    //Cv2.Rectangle(mat, rect, Scalar.Red, 2); // 使用红色线条绘制矩形，线宽为2 // 绘制中心点 //画点和区域
                                    //      Cv2.PutText(mat, _DrawTracking.Max_score.ToString("0.00"), new OpenCvSharp.Point(_DrawTracking.Left, _DrawTracking.Top - 10),
                                    //HersheyFonts.HersheySimplex, 0.5, new Scalar(0, 255, 0), 1);
                                    Cv2.Circle(mat, _DrawTracking.Left + _DrawTracking.Width / 2, _DrawTracking.Top + _DrawTracking.Height / 2, 10, Scalar.Red, -1); // 使用蓝色填充圆形，半径为5  //存储轨迹数据
                                    ContainsMouseZone?.Invoke(null, _DrawTracking);
                                }
                                App.Current.Dispatcher.Invoke(() =>
                                {

                                    using (MemoryStream memoryStream = mat.ToMemoryStream(".bmp"))
                                    {
                                        WriteableBitmap = ToImageSource(memoryStream);
                                    }
                                });
                            }
                            await Task.Delay(1);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                    }
                    Interlocked.Exchange(ref _drawLock, 0);
                });
            }
        }
        // at class level
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        private ImageSource ToImageSource(MemoryStream stream)
        {
            try
            {
                ImageSource source;
                using (var bitmap = new Bitmap(stream))
                {
                    var hBitmap = bitmap.GetHbitmap();
                    source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    DeleteObject(hBitmap);
                }
                return source;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }
        ///轨迹追踪数据
        private ConcurrentQueue<Tracking> trackingsQueue;
        private CancellationTokenSource _cts;
        private List<EventChannelJson> _trackingEventChannelJson = new List<EventChannelJson>();
        private List<EventChannelJson> _trackingEventChannelJsonOutput = new List<EventChannelJson>();
        ///string ==zoneName,channeltypeenum==状态
        private Dictionary<int, ChannelTypeEnum> _trackingMarkerStatu = new Dictionary<int, ChannelTypeEnum>();
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
                        var tracking = inperTrackingOpenvinoHelper.Detect(mat);
                        if (tracking.Count > 0)
                        {
                            var track = tracking.OrderByDescending(x => x.Max_score).First();
                            if (InperGlobalClass.IsRecord)
                            {
                                lock (InperDeviceHelper.Instance._timeLock)
                                {
                                    track.CameraTime = InperDeviceHelper.Instance.time / 100;
                                }
                                trackingsQueue.Enqueue(track);
                            }
                            _DrawTracking = track;
                        }
                        else
                        {
                            _DrawTracking = null;
                        }
                        Interlocked.Exchange(ref _trackingLock, 0);
                    });
                }
            }
            catch (Exception e)
            {
                Interlocked.Exchange(ref _trackingLock, 0);
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
                    _trackingMarkerStatu.Add(x.ChannelId, ChannelTypeEnum.None);
                });
                _trackingEventChannelJsonOutput.ForEach(x =>
                {
                    _trackingMarkerStatu.Add(x.Condition.ChannelId, ChannelTypeEnum.None);
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
                    if (tracking != null)
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

                }
                else
                {
                    await Task.Delay(1); // 避免无限循环，减轻CPU负担
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
        public event EventHandler<Tracking> ContainsMouseZone;
        private void TrackingToMarker(Tracking tracking)
        {
            try
            {
                var point = new System.Windows.Point(tracking.CenterX * (480 / WriteableBitmap.Width), tracking.CenterY * (320 / WriteableBitmap.Height));
                //var point = new System.Windows.Point(tracking.Left + tracking.Width / 2, tracking.Top + tracking.Height / 2);
                if (_trackingEventChannelJsonOutput.Any())
                {
                    _trackingEventChannelJsonOutput.ForEach(x =>
                    {
                        if (x.Condition.VideoZone != null && x.Condition.VideoZone.Name == Name)
                        {
                            if (x.Condition.VideoZone.AllZoneConditions.First(z => z.ZoneName == x.Condition.SymbolName) is ZoneConditions zone)
                            {
                                var rect = new System.Windows.Rect(zone.ShapeLeft, zone.ShapeTop, zone.ShapeWidth, zone.ShapeHeight);
                                bool re = TrackingStateSet(rect.Contains(point), x.Condition.ChannelId, zone.Timer, _trackingMarkerStatu[x.Condition.ChannelId] == ChannelTypeEnum.Leave);

                                if (zone.Type == _trackingMarkerStatu[x.Condition.ChannelId].ToString())
                                {
                                    if (zone.IsDelay || x.Condition.Type == ChannelTypeEnum.Stay.ToString())
                                    {
                                        if (zone.IsDelay && x.Condition.Type != ChannelTypeEnum.Stay.ToString())
                                        {
                                            zone.StartTimerDelay(x, false);
                                        }
                                        else
                                        {
                                            zone.StartTimerStay(x, false);
                                        }
                                    }
                                    else
                                    {
                                        if (x.Condition.Type != ChannelTypeEnum.Leave.ToString())
                                        {
                                            zone.ImmediatelyDrawMarker(x, false);
                                        }
                                    }
                                }
                                else
                                {
                                    if ((re && x.Condition.Type == ChannelTypeEnum.Leave.ToString()) && !zone.IsDelay)// || zone.IsTimerSignal
                                    {
                                        zone.ImmediatelyDrawMarker(x, false);
                                    }
                                    if (x.Condition.Type == ChannelTypeEnum.Stay.ToString())
                                    {
                                        zone.StayStopDraw();
                                    }
                                }
                            }
                        }
                    });
                }
                if (_trackingEventChannelJson.Any())
                {
                    _trackingEventChannelJson.ForEach(x =>
                    {
                        if (x.VideoZone != null && x.VideoZone.Name == Name)
                        {
                            x.VideoZone.AllZoneConditions.ForEach(condition =>
                            {
                                if (condition.ZoneName == x.SymbolName)
                                {
                                    var rect = new System.Windows.Rect(condition.ShapeLeft, condition.ShapeTop, condition.ShapeWidth, condition.ShapeHeight);

                                    bool re = TrackingStateSet(rect.Contains(point), x.ChannelId, condition.Timer, _trackingMarkerStatu[x.ChannelId] == ChannelTypeEnum.Leave);

                                    if (x.Type == _trackingMarkerStatu[x.ChannelId].ToString())
                                    {
                                        if (condition.IsDelay || x.Type == ChannelTypeEnum.Stay.ToString())
                                        {
                                            if (condition.IsDelay && x.Type != ChannelTypeEnum.Stay.ToString())
                                            {
                                                condition.StartTimerDelay(x, true);
                                            }
                                            else
                                            {
                                                condition.StartTimerStay(x, true);
                                            }
                                        }
                                        else
                                        {
                                            if (x.Type != ChannelTypeEnum.Leave.ToString())
                                            {
                                                condition.ImmediatelyDrawMarker(x, true);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if ((re && x.Type == ChannelTypeEnum.Leave.ToString()) && !condition.IsDelay)
                                        {
                                            condition.ImmediatelyDrawMarker(x, true);
                                        }
                                        if (x.Type == ChannelTypeEnum.Stay.ToString())
                                        {
                                            condition.StayStopDraw();
                                        }
                                    }
                                }
                            });
                        }
                        //每个Video下的zone
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private bool TrackingStateSet(bool isTrue, int name, System.Timers.Timer timer, bool isLeave = false)
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
                        _trackingMarkerStatu[name] = ChannelTypeEnum.None;
                        if (isLeave)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public async void Reset(AccordResolutionInfo resolutionInfo)
        {
            try
            {
                await accordCamraSetting.Stop();
                var resol = accordCamraSetting.VideoCapabilities.FirstOrDefault(x => x.FrameSize == resolutionInfo.FrameSize);
                accordCamraSetting.SetResoulation(resol);
                StartCapture();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public async void StartCapture()
        {
            _trackingEventChannelJson = new List<EventChannelJson>();
            _trackingEventChannelJsonOutput = new List<EventChannelJson>();
            receivedMats = new ConcurrentQueue<Mat>();
            trackCancellationToken.Cancel();
            trackCancellationToken = new CancellationTokenSource();
            await Task.Run(async () =>
            {
                while (true)
                {
                    if (_drawLock == 0)
                    {
                        break;
                    }
                    await Task.Delay(100);
                }
            });
            accordCamraSetting.Start();
            DrawAndTrack(trackCancellationToken.Token);
        }
        public async void StartRecording(string path = null)
        {
            await accordCamraSetting.Stop();
            accordCamraSetting.SetWritePath(path);
            StartCapture();
            accordCamraSetting.IsRecord = true;

            StartTracking();
        }
        public async void Stop()
        {
            await accordCamraSetting.Stop();
            trackCancellationToken.Cancel();
            accordCamraSetting.IsRecord = false;
            StopTracking();
        }
        #endregion
    }
}