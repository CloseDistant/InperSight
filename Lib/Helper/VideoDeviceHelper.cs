using InperSight.Lib.Bean;
using InperVideo.Camera;
using InperVideo.Interfaces;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Stylet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace InperSight.Lib.Helper
{
    public class VideoDeviceHelper : PropertyChangedBase
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        private ImageSource _WBMPPreview;
        public ImageSource WBMPPreview
        {
            get => _WBMPPreview;
            set => SetAndNotify(ref _WBMPPreview, value);
        }
        private Dictionary<string, IEnumerable<ICameraCapabilyItem>> capabilyItems = new Dictionary<string, IEnumerable<ICameraCapabilyItem>>();
        public Dictionary<string, IEnumerable<ICameraCapabilyItem>> CapabilyItems
        {
            get => capabilyItems;
            set => SetAndNotify(ref capabilyItems, value);
        }
        private bool _autoRecord;
        public bool AutoRecord
        {
            get => _autoRecord;
            set => SetAndNotify(ref _autoRecord, value);
        }
        public int _CamIndex;
        public string Name { get; set; }
        public string CustomName { get; set; }
        public double ImageWidth { get; set; }
        public double ImageHeight { get; set; }
        public bool IsActive { get; set; }
        private ICameraParamSet _cameraParamSet;
        private IVideoAcquirer videoAcquirer;

        public VideoDeviceHelper(int devIndex, string name, ICameraParamSet cameraParamSet)
        {
            _cameraParamSet = cameraParamSet;
            _CamIndex = devIndex;
            Name = name;
            videoAcquirer = new VideoAcquirerFactory().CreateVideoCapturer(devIndex, cameraParamSet);
            videoAcquirer.MatRtShowCreated += VideoAcquirer_MatRtShowCreated;
        }
        private void VideoAcquirer_MatRtShowCreated(object sender, MatRtShowEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                using (MemoryStream memoryStream = e.Context.MatFrame.FrameMat.ToMemoryStream(".bmp"))
                {
                    WBMPPreview = ToImageSource(memoryStream);
                }
                e.Context.Callback();
            }));
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
        public void Reset(ICameraParamSet cameraParamSet)
        {
            videoAcquirer.StopAcq();
            videoAcquirer.RestParam(cameraParamSet);
        }
        public void StartCapture()
        {
            videoAcquirer.StartAcq();
        }
        public void StartRecording(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName, DateTime.Now.ToString("yyyyMMddHHmmss") + "_Video");
            }
            videoAcquirer.StartRecord(path);
        }
        public void StopRecording()
        {
            videoAcquirer.StopAcq();
            videoAcquirer.StartAcq();
        }
        public void StopPreview()
        {
            videoAcquirer.StopAcq();
        }
    }
}
