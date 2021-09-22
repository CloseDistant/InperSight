//using Accord.Video.FFMPEG;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace InperStudio.Lib.Bean
{
    struct TimerCaps
    {
        public int periodMin;
        public int periodMax;
    }
    static class TSMat
    {
        private static TimeSpan _RelativeTimestamp;
        public static TimeSpan GetRelativeTimestamp(this Mat mat)
        {
            return _RelativeTimestamp;
        }
        public static void SetRelativeTimestamp(this Mat mat, TimeSpan ts)
        {
            _RelativeTimestamp = ts;
            return;
        }
    };
    //public class BehaviorRecorderKit : BindableObject
    //{
    //    private readonly object _VideoWriterLocker = new object();
    //    private List<Task> _TaskList = new List<Task>();
    //    public BehaviorRecorderKit(int index, string name)
    //    {
    //        Name = name;
    //        _CamIndex = index;
    //        Writer = new VideoFileWriter();
    //        Stopwatch = new Stopwatch();
    //        AutoRecord = true;

    //        //_TaskList.Add(Task.Run(() => { GrabProc(); }));
    //        //_TaskList.Add(Task.Run(() => { PreviewUpdateProc(); }));
    //        //_TaskList.Add(Task.Run(() => { EncodeProc(); }));
    //    }

    //    public BehaviorRecorderKit()
    //    {
    //    }

    //    public int _CamIndex;
    //    public bool IsActive { get; set; }
    //    public string Name { get; set; }
    //    public string CustomName { get; set; }

    //    public string Path { get; private set; }

    //    public bool AutoRecord
    //    {
    //        get { return GetValue(() => AutoRecord); }
    //        set { SetValue(() => AutoRecord, value); }
    //    }

    //    public bool IsRecording
    //    {
    //        get { return GetValue(() => IsRecording); }
    //        private set { SetValue(() => IsRecording, value); }
    //    }

    //    public WriteableBitmap WriteableBitmap
    //    {
    //        get { return GetValue(() => WriteableBitmap); }
    //        private set { SetValue(() => WriteableBitmap, value); }
    //    }

    //    public VideoFileWriter Writer
    //    {
    //        get { return GetValue(() => Writer); }
    //        private set { SetValue(() => Writer, value); }
    //    }

    //    public VideoCapture Device
    //    {
    //        get { return GetValue(() => Device); }
    //        private set { SetValue(() => Device, value); }
    //    }

    //    public Stopwatch Stopwatch
    //    {
    //        get { return GetValue(() => Stopwatch); }
    //        private set { SetValue(() => Stopwatch, value); }
    //    }

    //    public string Time
    //    {
    //        get { return GetValue(() => Time); }
    //        set { SetValue(() => Time, value); }
    //    }

    //    public uint RecordedFrameCount
    //    {
    //        get { return GetValue(() => RecordedFrameCount); }
    //        set { SetValue(() => RecordedFrameCount, value); }
    //    }


    //    /////////////////// Threading Solution ///////////////////////

    //    private int _VideoFrameWidth = 640;
    //    private int _VideoFrameHeight = 480;
    //    private double _FPS = 30;

    //    private bool _BRKIsActive = true;

    //    #region Grab Process
    //    private HPTimer _GrabMetronome = new HPTimer();
    //    private void SetupGrabMetronome(double fps)
    //    {
    //        _GrabMetronome.Interval = (int)((1000 / fps) - 1);
    //        _GrabMetronome.Tick += OnGrabTime;
    //        _GrabMetronome.Start();
    //    }

    //    private void OnGrabTime()
    //    {
    //        _AREGrab.Set();
    //        return;
    //    }

    //    private AutoResetEvent _AREGrab = new AutoResetEvent(false);
    //    private void GrabProc()
    //    {
    //        var frame = new Mat(new OpenCvSharp.Size(_VideoFrameWidth, _VideoFrameHeight), MatType.CV_8UC3);
    //        while (_BRKIsActive)
    //        {
    //            _AREGrab.WaitOne();

    //            if (Device != null && Device.Read(frame))
    //            {
    //                frame.SetRelativeTimestamp(Stopwatch.Elapsed);

    //                if (Monitor.TryEnter(_PFLock))
    //                {
    //                    _PreviewMat = frame;
    //                    Monitor.Exit(_PFLock);
    //                }

    //                if (IsRecording)
    //                {
    //                    lock (_FBQLock)
    //                    {
    //                        _VideoFrameBufferQ.Enqueue(frame);
    //                    }
    //                    _AREEncode.Set();
    //                }

    //            }
    //        }
    //    }
    //    #endregion

    //    #region Preview Process
    //    private object _PFLock = new object();
    //    public Mat _PreviewMat = new Mat(new OpenCvSharp.Size(640, 480), MatType.CV_8UC3);
    //    private HPTimer _PreviewUpdateMetronome = new HPTimer();
    //    private void SetupPreviewUpdateMetronome()
    //    {
    //        _PreviewUpdateMetronome.Interval = 30;
    //        _PreviewUpdateMetronome.Tick += OnPreviewUpdateTime;
    //        _PreviewUpdateMetronome.Start();
    //    }
    //    private void OnPreviewUpdateTime()
    //    {
    //        _AREPreviewUpdate.Set();
    //        return;
    //    }

    //    private AutoResetEvent _AREPreviewUpdate = new AutoResetEvent(false);
    //    private void PreviewUpdateProc()
    //    {
    //        Bitmap bitmap;
    //        TimeSpan ts;
    //        while (true)
    //        {
    //            _ = _AREPreviewUpdate.WaitOne();
    //            if (!_BRKIsActive)
    //            {
    //                break;
    //            }

    //            lock (_PFLock)
    //            {
    //                if (_PreviewMat.Data.ToInt64() != 0)
    //                {
    //                    bitmap = _PreviewMat.Clone().ToBitmap();
    //                    ts = _PreviewMat.GetRelativeTimestamp();
    //                }
    //                else { break; }
    //            }

    //            var count = 3 * bitmap.Width * bitmap.Height;
    //            byte[] des = new byte[count];
    //            var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
    //                System.Drawing.Imaging.ImageLockMode.ReadOnly,
    //                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
    //            IntPtr ptr = bitmapData.Scan0;
    //            Marshal.Copy(ptr, des, 0, count);
    //            unsafe
    //            {
    //                Application.Current.Dispatcher.Invoke(new Action(() =>
    //                {
    //                    WriteableBitmap.Lock();
    //                    Marshal.Copy(des, 0, WriteableBitmap.BackBuffer, count);
    //                    WriteableBitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.Width, bitmap.Height));
    //                    WriteableBitmap.Unlock();
    //                }));
    //            }

    //            bitmap.UnlockBits(bitmapData);

    //            //Time = Stopwatch.Elapsed.Hours.ToString("0#") + ":" + Stopwatch.Elapsed.Minutes.ToString("0#") + ":" + Stopwatch.Elapsed.Seconds.ToString("0#");
    //            Time = ts.Hours.ToString("0#") + ":" + ts.Minutes.ToString("0#") + ":" + ts.Seconds.ToString("0#");
    //        }
    //    }
    //    #endregion


    //    #region Encode Process
    //    private object _FBQLock = new object();
    //    private Queue<Mat> _VideoFrameBufferQ = new Queue<Mat>();
    //    private AutoResetEvent _AREEncode = new AutoResetEvent(false);
    //    private void EncodeProc()
    //    {
    //        while (_BRKIsActive)
    //        {
    //            _AREEncode.WaitOne();

    //            if (Monitor.TryEnter(_FBQLock))
    //            {
    //                Mat[] mats = _VideoFrameBufferQ.ToArray();
    //                _VideoFrameBufferQ.Clear();
    //                Monitor.Exit(_FBQLock);

    //                if (mats.Count() == 0)
    //                {
    //                    continue;
    //                }

    //                if (Monitor.TryEnter(_VideoWriterLocker))
    //                {
    //                    if (AutoRecord && Writer.IsOpen)
    //                    {
    //                        foreach (var mat in mats)
    //                        {
    //                            Writer.WriteVideoFrame(mat.ToBitmap(), RecordedFrameCount++);
    //                            Writer.Flush();
    //                        }
    //                    }
    //                    Monitor.Exit(_VideoWriterLocker);
    //                }
    //            }
    //        }
    //    }
    //    #endregion


    //    #region Control
    //    public void StartPreview()
    //    {
    //        _BRKIsActive = true;
    //        Device = new VideoCapture(_CamIndex, VideoCaptureAPIs.DSHOW);
    //        // camera params setting operation is too slow, set them only when it's necessary!
    //        if (Device.FrameWidth != _VideoFrameWidth)
    //        {
    //            Device.Set(VideoCaptureProperties.FrameWidth, _VideoFrameWidth);
    //            _VideoFrameWidth = Device.FrameWidth;
    //        }
    //        if (Device.FrameHeight != _VideoFrameHeight)
    //        {
    //            Device.Set(VideoCaptureProperties.FrameHeight, _VideoFrameHeight);
    //            _VideoFrameHeight = Device.FrameHeight;
    //        }
    //        if (Device.Fps != _FPS)
    //        {
    //            Device.Set(VideoCaptureProperties.Fps, _FPS);
    //            _FPS = Device.Fps;
    //        }
    //        _PreviewMat = new Mat(new OpenCvSharp.Size(_VideoFrameWidth, _VideoFrameHeight), MatType.CV_8UC3);

    //        WriteableBitmap = new WriteableBitmap(_VideoFrameWidth, _VideoFrameHeight, 96, 96, PixelFormats.Bgr24, null);

    //        if (_TaskList.Count != 0)
    //        {
    //            _TaskList.Clear();
    //        }
    //        _TaskList.Add(Task.Run(() => { GrabProc(); }));
    //        _TaskList.Add(Task.Run(() => { PreviewUpdateProc(); }));
    //        _TaskList.Add(Task.Run(() => { EncodeProc(); }));

    //        SetupGrabMetronome(_FPS);
    //        SetupPreviewUpdateMetronome();

    //        return;
    //    }


    //    public void StopPreview()
    //    {
    //        if (Device != null && Device.IsOpened())
    //        {
    //            Device.Release();
    //        }
    //    }


    //    public void StartRecord(string filename)
    //    {
    //        if (!AutoRecord)
    //        {
    //            return;
    //        }

    //        if (!Writer.IsOpen)
    //        {
    //            if (!filename.EndsWith(".mp4"))
    //            {
    //                filename += ".mp4";
    //            }

    //            // Video file resolution must be a multiple of two
    //            var resolution = _VideoFrameWidth * _VideoFrameHeight * 3 / 4;
    //            if (resolution % 2 != 0)
    //            {
    //                resolution *= 2;
    //            }

    //            // Accord need a global Lock on open and close video file.
    //            lock (_VideoWriterLocker)
    //            {
    //                Writer.Open(
    //                    filename,
    //                    _VideoFrameWidth,
    //                    _VideoFrameHeight,
    //                    new Accord.Math.Rational(Math.Round(_FPS, 2)),
    //                    VideoCodec.MPEG4);
    //            }

    //            Stopwatch.Reset();
    //            RecordedFrameCount = 0;
    //            Stopwatch.Start();
    //            IsRecording = true;
    //        }
    //    }


    //    public void StopRecord()
    //    {
    //        if (Writer.IsOpen)
    //        {
    //            Debug.WriteLine(Stopwatch.Elapsed);
    //            Debug.WriteLine(RecordedFrameCount);
    //            Stopwatch.Stop();
    //            // Accord need a global Lock on open and close video file.
    //            // https://blog.csdn.net/liang12360640/article/details/46044763
    //            lock (_VideoWriterLocker)
    //            {
    //                Writer.Close();
    //            }
    //            IsRecording = false;
    //        }
    //    }
    //    #endregion


    //    private void CleanUpThreads()
    //    {
    //        _BRKIsActive = false;

    //        _AREGrab.Set();
    //        _AREPreviewUpdate.Set();
    //        _AREEncode.Set();

    //        Task.WaitAll(_TaskList.ToArray(), 500);
    //    }


    //    public void Dispose()
    //    {
    //        CleanUpThreads();
    //        StopRecord();
    //        StopPreview();
    //        Writer.Dispose();
    //    }
    //    ////////////////////////////////////////////////////////////////////
    //}
}
