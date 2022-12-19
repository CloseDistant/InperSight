using InperSight.Lib.Bean;
using InperSight.Lib.Bean.Data;
using InperSight.Lib.Bean.Data.Model;
using InperSight.Lib.Chart.Channel;
using InperSight.Lib.Config;
using InperSight.ViewModels;
using InperSight.Views;
using InperVideo.Camera;
using InperVideo.Interfaces;
using OpenCvSharp;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using Stylet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace InperSight.Lib.Helper
{
    public enum DeviceParamProperties
    {
        GAIN,
        FRAMERATE,
        LED,
        EWL
    }
    public class InperDeviceHelper : PropertyChangedBase
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        private static InperDeviceHelper deviceHelper;
        public static InperDeviceHelper GetInstance()
        {
            if (deviceHelper == null)
            {
                Interlocked.CompareExchange(ref deviceHelper, new InperDeviceHelper(), null);
            }
            return deviceHelper;
        }
        private ImageSource _WBMPPreview;
        public ImageSource WBMPPreview
        {
            get => _WBMPPreview;
            set => SetAndNotify(ref _WBMPPreview, value);
        }
        public bool MiniscopeIsStartCaptrue = false;
        public int _CamIndex;
        public double ImageWidth { get; set; }
        public double ImageHeight { get; set; }
        private ICameraParamSet _cameraParamSet;
        private IVideoAcquirer videoAcquirer;
        public BindableCollection<CameraChannel> CameraChannels { get; set; } = new BindableCollection<CameraChannel>();
        public EventChannel EventChannelChart { get; set; } = new EventChannel();
        public event Action<bool> StartCollectEvent;
        #region frame 视野相关
        public bool isSwitchToFrame = false;
        public bool isDeltaF = false;
        public double frameLeft = 0d, frameTop = 0d, frameWidth = 0d, frameHeight = 0d;
        #endregion
        public void MiniscopeCameraInit(int devIndex, ICameraParamSet cameraParamSet)
        {
            _cameraParamSet = cameraParamSet;
            _CamIndex = devIndex;
            videoAcquirer = new VideoAcquirerFactory().CreateVideoCapturer(devIndex, cameraParamSet);
            videoAcquirer.MatRtShowCreated += VideoAcquirer_MatRtShowCreated;
            Task.Factory.StartNew(() => GreyValueShow());
        }
        public void MiniscopeStartCapture()
        {
            MiniscopeIsStartCaptrue = true;
            videoAcquirer.StartAcq();
            InitDevice();
        }
        public void MiniscopeStopCapture()
        {
            videoAcquirer.StopAcq();
        }
        public void MiniscopeFpsReset(int fps)
        {
            videoAcquirer.MiniscopeFpsReset(fps);
        }
        public void MiniScopeStartRecord(string path)
        {
            videoAcquirer.StartRecord(path);
        }
        /// <summary>
        /// 第一次接收时间
        /// </summary>
        DateTime? _firstTime;
        private void VideoAcquirer_MatRtShowCreated(object sender, MatRtShowEventArgs e)
        {
            if (InperGlobalClass.IsPreview || InperGlobalClass.IsRecord)
            {
                if (_firstTime == null)
                {
                    _firstTime = e.Context.MatFrame.Time;
                }
                _GreyValueDraw.Enqueue(e.Context.MatFrame.Clone());
            }
            _GreyValueShowQueue.Enqueue(e.Context.MatFrame.Clone());
            e.Context.Callback();
        }
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
        #region 渲染存储相关
        ConcurrentQueue<VideoFrame> _GreyValueShowQueue = new ConcurrentQueue<VideoFrame>();
        DateTime baseLinePreviousTimeStamp;
        int baseLineFrameBufWritePos;
        Mat baseLineFrame;
        Mat[] baseLineFrameBuffer = new Mat[128];
        public void RestDeltaF()
        {
            baseLinePreviousTimeStamp = new DateTime();
            baseLineFrameBufWritePos = 0;
            baseLineFrameBuffer = new Mat[128];
        }
        private void GreyValueShow()
        {
            while (true)
            {
                try
                {
                    if (_GreyValueShowQueue.TryDequeue(out VideoFrame _mat))
                    {
                        Mat mat = new();
                        Cv2.ConvertScaleAbs(_mat.FrameMat, mat, InperGlobalClass.CameraSettingJsonBean.UpperLevel, InperGlobalClass.CameraSettingJsonBean.LowerLevel);
                        if (isSwitchToFrame)
                        {
                            OpenCvSharp.Rect rect = new OpenCvSharp.Rect((int)frameLeft, (int)frameTop, (int)frameWidth, (int)frameHeight);
                            mat = mat[rect];
                        }
                        if (isDeltaF)
                        {
                            if (_mat.Time.Subtract(baseLinePreviousTimeStamp).TotalMilliseconds > 50)
                            {
                                var tempMat1 = mat.Clone();
                                tempMat1.ConvertTo(tempMat1, MatType.CV_32F);
                                tempMat1 = tempMat1 / 128;
                                if (baseLineFrameBufWritePos == 0)
                                {
                                    baseLineFrame = tempMat1;
                                }
                                else if (baseLineFrameBufWritePos < 128)
                                {
                                    baseLineFrame += tempMat1;
                                }
                                else
                                {
                                    baseLineFrame += tempMat1;
                                    baseLineFrame -= baseLineFrameBuffer[baseLineFrameBufWritePos % 128];
                                }
                                baseLineFrameBuffer[baseLineFrameBufWritePos % 128] = tempMat1.Clone();
                                baseLinePreviousTimeStamp = _mat.Time;
                                baseLineFrameBufWritePos++;
                            }
                            mat.ConvertTo(mat, MatType.CV_32F);
                            Cv2.Divide(mat, baseLineFrame, mat);
                            mat = ((mat - 1.0) + 0.02) * 256 * 10;
                            mat.ConvertTo(mat, MatType.CV_8U);
                        }
                        unsafe
                        {
                            byte* addr = (byte*)mat.Data;
                            int index;
                            for (int row = 0; row < mat.Rows; row++)
                            {
                                var pxvec = mat.Ptr(row);
                                for (int col = 0; col < mat.Cols; col++)
                                {
                                    index = (int)(mat.Step() * row + (col * 3));
                                    int b = addr[index];
                                    int g = addr[index + 1];
                                    int r = addr[index + 2];
                                    if ((b + g + r) / 3 > 0xFA)
                                    {
                                        addr[index] = 0x00;
                                        addr[index + 1] = 0x00;
                                        addr[index + 2] = 0xFF;
                                    }
                                }
                            }
                        }
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            using (MemoryStream memoryStream = mat.ToMemoryStream(".bmp"))
                            {
                                WBMPPreview = ToImageSource(memoryStream);
                            }
                            mat.Dispose();
                        }));
                    }
                    Thread.Sleep(2);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }
        ConcurrentQueue<VideoFrame> _GreyValueDraw = new();
        CancellationTokenSource _greyValueToCalculateCancel = new();
        private void GreyValueToCalculate()
        {
            try
            {
                while (!_greyValueToCalculateCancel.IsCancellationRequested)
                {
                    if (_GreyValueDraw.TryDequeue(out VideoFrame mat))
                    {
                        string _saveStr = string.Empty;
                        TimeSpan time = new(mat.Time.Subtract((DateTime)_firstTime).Ticks);
                        foreach (var item in CameraChannels)
                        {
                            double r = (double)mat.FrameMat.Mean(item.Mask) / 655.35;
                            _saveStr += item.ChannelId + "," + r + " ";
                            using (item.RenderableSeries.First().DataSeries.SuspendUpdates())
                            {
                                (item.RenderableSeries.First().DataSeries as XyDataSeries<TimeSpan, double>).Append(time, r);
                            }
                        }
                        if (!string.IsNullOrEmpty(_saveStr))
                        {
                            _saveStrs.Enqueue(new ChannelRecord()
                            {
                                CameraTime = time.Ticks,
                                CreateTime = DateTime.Now,
                                Value = _saveStr
                            });
                        }
                    }
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        ConcurrentQueue<ChannelRecord> _saveStrs = new();
        CancellationTokenSource _greyValueToSaveCancel = new();
        List<ChannelRecord> _recordsCache = new();
        private void GreyValueToSave()
        {
            try
            {
                while (!_greyValueToSaveCancel.IsCancellationRequested)
                {
                    if (_saveStrs.TryDequeue(out ChannelRecord str))
                    {
                        _recordsCache.Add(str);
                        if (_recordsCache.Count >= InperGlobalClass.CameraSettingJsonBean.FPS * 5)
                        {
                            App.SqlDataInit.ChannelRecordSave(_recordsCache);
                            _recordsCache.Clear();
                        }
                    }
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        #endregion

        #region start stop
        public void Start(bool isRecord)
        {
            try
            {
                if (CameraChannels.Count == 0)
                {
                    InperGlobalFunc.ShowRemainder("未添加Neuron");
                    return;
                }
                InperGlobalClass.IsPreview = !isRecord || isRecord;
                InperGlobalClass.IsRecord = isRecord;
                InperGlobalClass.IsStop = false;
                Parallel.ForEach(CameraChannels, item =>
                {
                    item.RenderableSeries.First().DataSeries.Clear();
                });
                _GreyValueDraw = new();
                _saveStrs = new();
                _recordsCache = new();
                _greyValueToCalculateCancel = new();
                _firstTime = null;
                Task.Run(() => GreyValueToCalculate(), _greyValueToCalculateCancel.Token);

                foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                {
                    if (window.Name.Contains("MainWindow"))
                    {
                        ((window.DataContext as MainWindowViewModel).ActiveItem as DataShowControlViewModel).SciScrollSet();
                    }
                }
                StartCollectEvent?.Invoke(true);
                InperGlobalClass.IsAllowDragScroll = true;

                if (InperGlobalClass.IsRecord)
                {
                    App.SqlDataInit = new();

                    _greyValueToSaveCancel = new();
                    Task.Run(() => GreyValueToSave(), _greyValueToSaveCancel.Token);
                    MiniScopeStartRecord(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName, DateTime.Now.ToString("HHmmss")));
                    foreach (var item in InperGlobalClass.ActiveVideos)
                    {
                        if (item.IsActive && item.AutoRecord)
                        {
                            item.StartRecording(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName, DateTime.Now.ToString("HHmmss") + "_" + item.CustomName));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        public void Stop()
        {
            try
            {
                if (InperGlobalClass.IsRecord)
                {
                    if (NoteSettingViewModel.NotesCache.Count > 0)
                    {
                        App.SqlDataInit.NoteSave(NoteSettingViewModel.NotesCache);
                        NoteSettingViewModel.NotesCache.Clear();
                    }
                    foreach (var item in InperGlobalClass.ActiveVideos)
                    {
                        if (item.IsActive && item.AutoRecord)
                        {
                            item.StopRecording();
                        }
                    }
                    if (_recordsCache.Count >= 0)
                    {
                        App.SqlDataInit.ChannelRecordSave(_recordsCache);
                        _recordsCache.Clear();
                    }
                }
                InperGlobalClass.IsStop = true;
                InperGlobalClass.IsPreview = false;
                InperGlobalClass.IsRecord = false;
                _greyValueToCalculateCancel.Cancel();
                _greyValueToSaveCancel.Cancel();
                _firstTime = null;

                MiniscopeStopCapture();
                MiniscopeStartCapture();

                if (Directory.GetDirectories(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName)).Length > 0 || Directory.GetFiles(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName)).Length > 0)
                {
                    InperGlobalClass.DataFolderName = DateTime.Now.ToString("yyyyMMddHHmmss");
                    if (!Directory.Exists(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName)))
                    {
                        Directory.CreateDirectory(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName));
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        #endregion

        #region 命令相关
        private void InitDevice()
        {
            //Speed up i2c bus timer to 50us max
            var cmd1 = new byte[3] { 0xC0, 0x22, 0b00000010 };
            SendCommand(cmd1);
            //Decrease BCC timeout, units in 2ms XX
            var cmd2 = new byte[3] { 0xC0, 0x20, 0b00001010 };
            SendCommand(cmd2);
            //Make sure DES has SER ADDR
            var cmd3 = new byte[3] { 0xC0, 0x07, 0xB0 };
            SendCommand(cmd3);
            //Speed up I2c bus timer to 50u Max
            var cmd4 = new byte[3] { 0xB0, 0x0F, 0b00000010 };
            SendCommand(cmd4);
            //Decrease BCC timeout, units in 2ms
            var cmd5 = new byte[3] { 0xB0, 0x1E, 0b00001010 };
            SendCommand(cmd5);
            //sets allowable i2c addresses to send through serializer
            var cmd6 = new byte[5] { 0xC0, 0x08, 0b00100000, 0b11101110, 0b10100000 };
            SendCommand(cmd6);
            //sets sudo allowable i2c addresses to send through serializer
            var cmd7 = new byte[6] { 0xC0, 0x10, 0b00100000, 0b11101110, 0b01011000, 0b01010000 };
            SendCommand(cmd7);
            //Remap BNO axes, and sign
            var cmd8 = new byte[4] { 0b01010000, 0x41, 0b00001001, 0b00000101 };
            SendCommand(cmd8);
            //Set BNO operation mode to NDOF
            var cmd9 = new byte[3] { 0b01010000, 0x3D, 0b00001100 };
            SendCommand(cmd9);
            //Enable BNO streaming in DAQ
            var cmd10 = new byte[2] { 0xFE, 0x00 };
            SendCommand(cmd10);
            //Enable EWL Driver
            var cmd11 = new byte[3] { 0b11101110, 0x03, 0x03 };
            SendCommand(cmd11);
        }
        public void DevcieParamSet(DeviceParamProperties deviceParam, double value)
        {
            switch (deviceParam)
            {
                case DeviceParamProperties.GAIN:
                    int gain = value == 1 ? 225 : value == 2 ? 228 : 36;
                    byte[] cmd = new byte[6] { 0b00100000, 0x05, 0x00, 0xCC, (byte)((gain >> 8) & 0xFF), (byte)(gain & 0xFF) };
                    SendCommand(cmd);
                    break;
                case DeviceParamProperties.FRAMERATE:
                    int framerate = value == 10 ? 10000 : value == 15 ? 6667 : value == 20 ? 5000 : value == 25 ? 4000 : 3300;
                    var cmdFramerate = new byte[6] { 0b00100000, 0x05, 0x00, 0xC9, (byte)((framerate >> 8) & 0xFF), (byte)(framerate & 0xFF) };
                    SendCommand(cmdFramerate);
                    break;
                case DeviceParamProperties.LED:
                    byte _value = (byte)((int)(Math.Round(value) * -2.55) + 255);
                    var cmd1 = new byte[3] { 0b00100000, 0x01, _value };
                    var cmd2 = new byte[4] { 0b01011000, 0x00, 114, _value };
                    SendCommand(cmd1);
                    SendCommand(cmd2);
                    break;
                case DeviceParamProperties.EWL:
                    byte ewl = (byte)(Math.Round(value) + 127);
                    var cmdEWL = new byte[4] { 0b11101110, 0x08, ewl, 0x02 };
                    SendCommand(cmdEWL);
                    break;
            }
        }
        private void SendCommand(byte[] packet)
        {
            try
            {
                ulong tempPacket;
                if (packet.Length < 6)
                {
                    tempPacket = packet[0];
                    tempPacket |= (((ulong)packet.Length) & 0xFF) << 8;
                    for (int j = 1; j < packet.Length; j++)
                    {
                        tempPacket |= ((ulong)packet[j]) << (8 * (j + 1));
                    }

                    bool success = videoAcquirer.VideoCapture.Set(VideoCaptureProperties.Contrast, tempPacket & 0x00000000FFFF);
                    success = videoAcquirer.VideoCapture.Set(VideoCaptureProperties.Gamma, (tempPacket & 0x0000FFFF0000) >> 16) && success;
                    success = videoAcquirer.VideoCapture.Set(VideoCaptureProperties.Sharpness, (tempPacket & 0xFFFF00000000) >> 32) && success;
                    if (!success)
                    {
                        Debug.WriteLine("send command failed");
                    }
                }
                else if (packet.Length == 6)
                {
                    tempPacket = (ulong)packet[0] | 0x01;
                    for (int j = 1; j < packet.Length; j++)
                    {
                        tempPacket |= ((ulong)packet[j]) << (8 * j);
                    }
                    bool success = videoAcquirer.VideoCapture.Set(VideoCaptureProperties.Contrast, tempPacket & 0x00000000FFFF);
                    success = videoAcquirer.VideoCapture.Set(VideoCaptureProperties.Gamma, (tempPacket & 0x0000FFFF0000) >> 16) && success;
                    success = videoAcquirer.VideoCapture.Set(VideoCaptureProperties.Sharpness, (tempPacket & 0xFFFF00000000) >> 32) && success;
                    if (!success)
                    {
                        Debug.WriteLine("send command failed");
                    }
                }
                else
                {
                    Debug.WriteLine("无效的字节");
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        #endregion
    }
}
