using InperSight.Lib.Bean;
using InperSight.Lib.Chart.Channel;
using InperSight.Lib.Config;
using InperVideo.Camera;
using InperVideo.Interfaces;
using OpenCvSharp;
using Stylet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
        public int _CamIndex;
        public double ImageWidth { get; set; }
        public double ImageHeight { get; set; }
        private ICameraParamSet _cameraParamSet;
        private IVideoAcquirer videoAcquirer;
        public BindableCollection<CameraChannel> CameraChannels { get; set; } = new BindableCollection<CameraChannel>();
        public EventChannel EventChannelChart { get; set; } = new EventChannel();

        #region frame 视野相关
        public bool isSwitchToFrame = false;
        public double frameLeft = 0d, frameTop = 0d, frameWidth = 0d, frameHeight = 0d;
        #endregion
        public void MiniscopeCameraInit(int devIndex, ICameraParamSet cameraParamSet)
        {
            _cameraParamSet = cameraParamSet;
            _CamIndex = devIndex;
            videoAcquirer = new VideoAcquirerFactory().CreateVideoCapturer(devIndex, cameraParamSet);
            videoAcquirer.MatRtShowCreated += VideoAcquirer_MatRtShowCreated;
        }
        public void MiniscopeStartCapture()
        {
            videoAcquirer.StartAcq();
            InitDevice();
        }
        public void MiniscopeStopCapture()
        {
            videoAcquirer.StopAcq();
        }
        private void VideoAcquirer_MatRtShowCreated(object sender, MatRtShowEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {

                Mat mat = new();
                Cv2.ConvertScaleAbs(e.Context.MatFrame.FrameMat.Clone(), mat, InperGlobalClass.CameraSettingJsonBean.UpperLevel , InperGlobalClass.CameraSettingJsonBean.LowerLevel );
                if (isSwitchToFrame)
                {
                    OpenCvSharp.Rect rect = new OpenCvSharp.Rect((int)frameLeft, (int)frameTop, (int)frameWidth, (int)frameHeight);
                    mat = mat[rect];
                }
                using (MemoryStream memoryStream = mat.ToMemoryStream(".bmp"))
                { 
                    WBMPPreview = ToImageSource(memoryStream);
                }
                e.Context.Callback();
                mat.Dispose();
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
        private unsafe void SendCommand(byte[] packet)
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
