using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Stylet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace InperStudio.Lib.Bean
{
    public class VideoRecordBean : PropertyChangedBase
    {
        public int _CamIndex;
        public bool IsActive { get; set; }
        public string Name { get; set; }
        public string CustomName { get; set; }
        private WriteableBitmap _writeableBitmap;
        public WriteableBitmap WriteableBitmap
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
        public bool IsCanOpen = true;
        #region private
        private VideoCapture _videoCapture;
        private VideoWriter _videoWriter;
        private CancellationTokenSource readTokenSource;
        private bool _isRecord = false;
        private double _actuallFrameCount = 0;
        private int _writeFrameCount = 0;
        private ConcurrentQueue<VideoFrame> _WriteMats = new ConcurrentQueue<VideoFrame>();
        private DateTime dt_start;
        private List<VideoFrame> _calFrames = new List<VideoFrame>();
        #endregion

        #region methods
        public VideoRecordBean(int devIndex, string name)
        {
            _CamIndex = devIndex;
            Name = name;
            Init();
        }
        private void Init()
        {
            _videoCapture = new VideoCapture();
            if (!_videoCapture.Open(_CamIndex, VideoCaptureAPIs.DSHOW))
            {
                IsCanOpen = false;
                return;
            }
            _videoCapture.Set(VideoCaptureProperties.FrameWidth, 640d);
            _videoCapture.Set(VideoCaptureProperties.FrameHeight, 480d);
            _videoCapture.Set(VideoCaptureProperties.FourCC, FourCC.MJPG);
        }
        private int _redLock = 0;
        public void StartCapture()
        {
            readTokenSource = new CancellationTokenSource();
            Interlocked.Exchange(ref _redLock, 0);
            if (_videoCapture.IsDisposed)
            {
                Init();
            }
            Task.Run(() =>
            {
                if (!_videoCapture.IsOpened())
                {
                    Console.WriteLine("摄像头打开失败");
                    return;
                }
                Mat Camera = new Mat();
                while (!readTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (Interlocked.Exchange(ref _redLock, 1) == 0)
                        {
                            _ = _videoCapture.Read(Camera);
                            if (Camera.Empty())//读取视频文件时,判定帧是否为空,如果帧为空,则下方的图片处理会报异常
                            {
                                Interlocked.Exchange(ref _redLock, 0);
                                continue;
                            }
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                WriteableBitmap = Camera.ToWriteableBitmap();
                            }));
                            if (_isRecord)
                            {
                                if (_writeFps == null)
                                {
                                    VideoFrame videoFrame = new VideoFrame(DateTime.Now, Camera);
                                    _calFrames.Add(videoFrame);
                                    _WriteMats.Enqueue(videoFrame);
                                    TimeSpan acqTime = videoFrame.Time - _calFrames.First().Time;
                                    if (acqTime >= TimeSpan.FromSeconds(1) && _calFrames.Count > 30)
                                    {
                                        double calFps = (_calFrames.Count - 4) / (videoFrame.Time - _calFrames[3].Time).TotalSeconds;
                                        WriteFps = (int)Math.Ceiling(calFps * 1.01d);
                                        _videoWriter = new VideoWriter(_path, FourCC.MJPG, _writeFps.Value, new OpenCvSharp.Size(_videoCapture.FrameWidth, _videoCapture.FrameHeight));
                                        Interlocked.Exchange(ref _lock, 0);
                                    }
                                }
                                else
                                {
                                    _WriteMats.Enqueue(new VideoFrame(DateTime.Now, Camera));
                                    _ = Task.Run(() => WriteFrame());
                                }
                            }
                            Interlocked.Exchange(ref _redLock, 0);
                        }
                    }
                    catch (Exception ex)
                    {
                        App.Log.Error(ex.ToString());
                        Interlocked.Exchange(ref _redLock, 0);
                    }
                    Thread.Sleep(1);
                }
            });
        }

        private int _lock = 1;
        private string _path;

        private void WriteFrame()
        {
            if (Interlocked.Exchange(ref _lock, 1) == 0)
            {
                while (_WriteMats.TryDequeue(out VideoFrame mat))
                {
                    if (_writeFrameCount == 0)
                    {
                        dt_start = mat.Time;
                        _videoWriter.Write(mat.FrameMat);
                        _writeFrameCount++;
                    }
                    else
                    {
                        _actuallFrameCount = Math.Round((mat.Time - dt_start).TotalSeconds * _writeFps.Value) + 1;
                        while (_writeFrameCount < _actuallFrameCount)
                        {
                            if (!_videoWriter.IsDisposed)
                            {
                                _videoWriter.Write(mat.FrameMat);
                                _writeFrameCount++;
                            }
                        }
                    }
                }
                Interlocked.Exchange(ref _lock, 0);
                if (!_WriteMats.IsEmpty)
                {
                    _ = Task.Run(() => WriteFrame());
                }
            }
        }
        public void StartRecording(string path = null)
        {
            readTokenSource.Cancel();
            _ = readTokenSource.Token.Register(() =>
              {
                  if (string.IsNullOrEmpty(path))
                  {
                      path = Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName, DateTime.Now.ToString("yyyyMMddHHmmss"));
                  }
                  _path = path += ".avi";
                  _writeFps = null;
                  _calFrames.Clear();
                  _WriteMats = new ConcurrentQueue<VideoFrame>();
                  _isRecord = true;
                  _ = Interlocked.Exchange(ref _lock, 1);
                  StartCapture();
              });
        }
        public void StopRecording(int type = 0)
        {
            _isRecord = false;
            if (readTokenSource != null)
            {
                readTokenSource.Cancel();
                readTokenSource.Token.Register(async () =>
                {
                    bool isEnd = false;
                    while (!isEnd)
                    {
                        if (_WriteMats.IsEmpty)
                        {
                            isEnd = true;
                        }
                        await Task.Delay(10);
                    }
                    _writeFrameCount = 0;
                    _actuallFrameCount = 0;

                    if (_videoWriter != null && !_videoWriter.IsDisposed)
                    {
                        _videoWriter.Release();
                        _videoWriter.Dispose();
                    }
                    if (type == 0)
                    {
                        StartCapture();
                    }
                });
            }
        }
        public void StopPreview()
        {
            if (readTokenSource != null)
            {
                readTokenSource.Cancel();
                _ = readTokenSource.Token.Register(() =>
                  {
                      Interlocked.Exchange(ref _redLock, 1);
                      _videoCapture.Dispose();
                      if (_videoWriter != null && !_videoWriter.IsDisposed)
                      {
                          _videoWriter.Release();
                          _videoWriter.Dispose();
                      }
                  });
            }
        }
        #endregion
    }
}