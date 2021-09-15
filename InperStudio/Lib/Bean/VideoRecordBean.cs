using HandyControl.Controls;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Diagnostics;
using InperStudio.Lib.Helper;

namespace InperStudio.Lib.Bean
{
    public class VideoRecordBean : BindableObject
    {
        public int _CamIndex;
        public bool IsActive { get; set; }
        public string Name { get; set; }
        public string CustomName { get; set; }
        public WriteableBitmap WriteableBitmap
        {
            get { return GetValue(() => WriteableBitmap); }
            private set { SetValue(() => WriteableBitmap, value); }
        }
        public bool AutoRecord
        {
            get { return GetValue(() => AutoRecord); }
            set { SetValue(() => AutoRecord, value); }
        }
        public long Time
        {
            get { return GetValue(() => Time); }
            set { SetValue(() => Time, value); }
        }
        public Mat _capturedFrame = new Mat();
        public Mat _writeFrame = new Mat();
        #region private
        //定义Timer类
        private VideoCaptureAPIs _videoCaptureApi = VideoCaptureAPIs.DSHOW;
        private readonly ManualResetEventSlim _writerReset = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _readEvent = new ManualResetEventSlim(false);
        private VideoCapture _videoCapture;
        private VideoWriter _videoWriter;
        private Task _readThread;
        private Task _writerThread;
        private CancellationTokenSource readTokenSource;
        private CancellationTokenSource writeTokenSource;
        private CancellationToken readToken;
        private CancellationToken writeToken;
        private double FPS = 30;
        private int _VideoFrameWidth = 640;
        private int _VideoFrameHeight = 480;
        private PerformanceTimer timer = new PerformanceTimer();
        private HighAccurateTimer accurateTimer = new HighAccurateTimer();
        private long AllWriteCount = 0;
        private double timeChange = 0;
        private readonly object countCheckObject = new object();
        private bool IsVideoCaptureValid => _videoCapture != null && _videoCapture.IsOpened();
        #endregion

        #region methods
        public VideoRecordBean(int devIndex, string name)
        {
            _CamIndex = devIndex;
            Name = name;
        }
        public void StartCapture()
        {
            Recorder();
            readTokenSource = new CancellationTokenSource();
            readToken = readTokenSource.Token;

            _readEvent.Reset();
            _readThread = new Task(CaptureFrameLoop, readToken);
            _readThread.Start();
        }
        private void Recorder()
        {
            _videoCapture = VideoCapture.FromCamera(_CamIndex, _videoCaptureApi);
            _ = _videoCapture.Open(_CamIndex, _videoCaptureApi);

            _ = _videoCapture.Set(VideoCaptureProperties.Fps, FPS);
            _ = _videoCapture.Set(VideoCaptureProperties.FrameWidth, _VideoFrameWidth);//设定摄像头图片的大小
            _ = _videoCapture.Set(VideoCaptureProperties.FrameHeight, _VideoFrameHeight);
        }
        public void StartRecording(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName, DateTime.Now.ToString("yyyyMMddHHmmss"));
            }
            path += ".avi";
            if (_writerThread != null)
            {
                return;
            }

            if (!IsVideoCaptureValid)
            {
                Growl.Warning("Player device not found", "SuccessMsg");
                return;
            }
            writeTokenSource = new CancellationTokenSource();
            writeToken = writeTokenSource.Token;

            _videoWriter = new VideoWriter(path, FourCC.DIVX, FPS - 5, new OpenCvSharp.Size(_videoCapture.FrameWidth, _videoCapture.FrameHeight));


            _writerReset.Reset();
            _writerThread = new Task(AddCameraFrameToRecordingThread, writeToken);
            _writerThread.Start();
            //isStartWrite = true;

            accurateTimer.Interval = 1 * 1000;
            accurateTimer.Elapsed += AccurateTimer_Elapsed;
            accurateTimer.Enabled = true;
        }

        private void AccurateTimer_Elapsed(object sender, TimerEventArgs e)
        {
            try
            {
                Monitor.Enter(countCheckObject);
                TimeSpan ts = TimeSpan.FromTicks(InperDeviceHelper.Instance.time / 100);
                long actualFps = (long)(ts.TotalMilliseconds / 40);
                if (actualFps == AllWriteCount)
                {
                    timeChange = 0;
                    return;
                }
                if (actualFps > AllWriteCount)
                {
                    timeChange = ts.TotalMilliseconds / actualFps - ts.TotalMilliseconds / AllWriteCount - 2;//

                }
                else
                {
                    timeChange = ts.TotalMilliseconds / actualFps - ts.TotalMilliseconds / AllWriteCount + 1;//
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
            finally
            {
                Monitor.Exit(countCheckObject);
            }
        }

        private void CaptureFrameLoop()
        {

            while (!_readEvent.Wait(0))
            {
                try
                {
                    if (readToken.IsCancellationRequested)
                    {
                        return;
                    }
                    if (_videoCapture.IsDisposed)
                    {
                        return;
                    }
                    _capturedFrame = _videoCapture.RetrieveMat();
                    if (_videoCapture.IsOpened())
                    {
                        //Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        //{
                        //    WriteableBitmap = _capturedFrame.Clone().ToWriteableBitmap();
                        //}));

                        Cv2.ImShow("test", _capturedFrame);
                        Cv2.WaitKey(1);
                    }
                }
                catch (Exception ex)
                {
                    App.Log.Error(ex.ToString());
                }
            }
        }
        private void AddCameraFrameToRecordingThread()
        {
            double delay = 1000 / (FPS - 5);
            double iWaitKeyTime = 0;
            double _waitS = 0;
            while (!_writerReset.Wait(0))
            {
                try
                {
                    timer.Start();
                    if (Monitor.TryEnter(countCheckObject))
                    {
                        if (writeToken.IsCancellationRequested)
                        {
                            return;
                        }
                        if (!_videoCapture.IsDisposed)
                        {
                            _videoWriter.Write(_capturedFrame);
                        }
                        AllWriteCount++;
                        timer.Stop();

                        double duration = timer.Duration;

                        timer.Start();
                        iWaitKeyTime = iWaitKeyTime < 0 ? 0 : iWaitKeyTime;
                        double waitTime = delay - duration - iWaitKeyTime + timeChange;

                        int _wait = (int)Math.Floor(waitTime);
                        _waitS += waitTime % 1;
                        if (_waitS > 1)
                        {
                            _wait += 1;
                            _waitS -= 1;
                        }
                        Cv2.WaitKey(_wait);
                        Console.WriteLine("waitTime:" + waitTime + "_wait:" + _wait + " _waitS:" + _waitS + " iWaitKeyTime:" + iWaitKeyTime + " timeChange:" + timeChange);
                        timer.Stop();
                        iWaitKeyTime = timer.Duration * 1000 - _wait + 1;
                        //
                        Monitor.Exit(countCheckObject);
                    }
                }
                catch (Exception ex)
                {
                    App.Log.Error(ex.ToString());
                }
                finally
                {
                    timer.Stop();
                }
            }
        }
        public void StopRecording()
        {
            accurateTimer.Enabled = false;
            AllWriteCount = 0;
            if (_writerThread != null)
            {
                _writerReset.Set();
                writeTokenSource.Cancel();
                _writerThread = null;
                _writerReset.Reset();
            }
            Time = 0;
            _videoWriter?.Release();
            _videoWriter?.Dispose();
            _videoWriter = null;
        }
        public void StopPreview()
        {
            if (_videoCapture != null && _videoCapture.IsOpened())
            {
                Cv2.DestroyWindow("test");
                _videoCapture.Release();
                _videoCapture.Dispose();
                if (_readThread != null)
                {
                    _readEvent.Set();
                    readTokenSource.Cancel();
                    _readThread = null;
                    _readEvent.Reset();
                }
            }
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopRecording();
                if (_videoCapture != null && !_videoCapture.IsDisposed)
                {
                    if (_videoCapture != null && _videoCapture.IsOpened())
                    {
                        _videoCapture?.Release();
                        _videoCapture?.Dispose();
                    }
                }
            }
        }
        #endregion
    }
    class PerformanceTimer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(
          out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(
          out long lpFrequency);

        private long startTime, stopTime;
        private long freq;

        public PerformanceTimer()
        {
            startTime = 0;
            stopTime = 0;

            if (QueryPerformanceFrequency(out freq) == false)
            {
                throw new Exception("Timer not supported.");
            }
        }

        public void Start()
        {
            Thread.Sleep(0);
            QueryPerformanceCounter(out startTime);
        }

        public void Stop()
        {
            QueryPerformanceCounter(out stopTime);
        }

        public double Duration
        {
            get
            {
                return (double)(stopTime - startTime) / (double)freq;
            }
        }
    }

}
