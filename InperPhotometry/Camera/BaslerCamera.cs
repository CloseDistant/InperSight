using Basler.Pylon;
using OpenCvSharp;
using System;

namespace InperDeviceManagement
{
    public class MarkedMat
    {
        public int Group { get; set; } = 0;
        public long Timestamp { get; set; } = 0;
        public Mat ImageMat { get; set; }
    }


    public enum CameraRunningMode
    {
        Trigger,
        FreeRun
    }


    public class BaslerCamera
    {
        public event EventHandler<MarkedMat> DidGrabImage;

        private CameraRunningMode _RunningMode = CameraRunningMode.Trigger;
        public CameraRunningMode RunningMode => _RunningMode;


        public void SwitchRunningMode(CameraRunningMode r_mode)
        {
            switch (r_mode)
            {
                case CameraRunningMode.FreeRun:
                    EnableCameraFreeRunMode();
                    break;

                case CameraRunningMode.Trigger:
                    EnableCameraTriggerMode();
                    break;

                default:
                    break;
            }
            return;
        }


        public string ID { get; private set; } = "";


        private void EnableCameraTriggerMode()
        {
            _Camera.StreamGrabber.Stop();

            //// Select and enable the Frame Start trigger
            _Camera.Parameters[PLCamera.TriggerSelector].SetValue(PLCamera.TriggerSelector.FrameStart);
            _Camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.On);
            _Camera.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerSource.Line1);
            _Camera.Parameters[PLCamera.TriggerActivation].SetValue(PLCamera.TriggerActivation.RisingEdge);

            _Camera.Parameters[PLCamera.LineSelector].SetValue(PLCamera.LineSelector.Line2);
            _Camera.Parameters[PLCamera.LineMode].SetValue(PLCamera.LineMode.Output);
            _Camera.Parameters[PLCamera.LineSource].SetValue(PLCamera.LineSource.FrameTriggerWait);

            _Camera.Parameters[PLCamera.LineSelector].SetValue(PLCamera.LineSelector.Line3);
            _Camera.Parameters[PLCamera.LineMode].SetValue(PLCamera.LineMode.Input);
            _Camera.Parameters[PLCamera.LineInverter].SetValue(false);

            _Camera.Parameters[PLCamera.LineSelector].SetValue(PLCamera.LineSelector.Line4);
            _Camera.Parameters[PLCamera.LineMode].SetValue(PLCamera.LineMode.Input);
            _Camera.Parameters[PLCamera.LineInverter].SetValue(false);

            _Camera.Parameters[PLCamera.ChunkModeActive].SetValue(true);
            _Camera.Parameters[PLCamera.ChunkSelector].SetValue(PLCamera.ChunkSelector.LineStatusAll);
            _Camera.Parameters[PLCamera.ChunkEnable].SetValue(true);

            _Camera.Parameters[PLCamera.ChunkSelector].SetValue(PLCamera.ChunkSelector.Timestamp);
            _Camera.Parameters[PLCamera.ChunkEnable].SetValue(true);

            _RunningMode = CameraRunningMode.Trigger;

            _Camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);

            return;
        }


        private void EnableCameraFreeRunMode()
        {
            _Camera.StreamGrabber.Stop();

            _Camera.Parameters[PLCamera.TriggerSelector].SetValue(PLCamera.TriggerSelector.FrameStart);
            _Camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.Off);
            //_Camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.On);

            _Camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
            _Camera.Parameters[PLCamera.AcquisitionStart].Execute();

            _RunningMode = CameraRunningMode.FreeRun;

            _Camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            return;
        }


        private void InitCamera()
        {
            _Camera.StreamGrabber.ImageGrabbed -= OnImageGrabbed;
            _Camera.Close();
            // Set the acquisition mode to free running continuous acquisition when the camera is opened.
            _Camera.CameraOpened += Configuration.AcquireContinuous;
            try
            {
                _Camera.Open(30000, TimeoutHandling.Return);
                if (_Camera.IsOpen)
                {

                    // DeviceVendorName, DeviceModelName, and DeviceFirmwareVersion are string parameters.
                    System.Diagnostics.Debug.WriteLine("Camera Device Information");
                    System.Diagnostics.Debug.WriteLine("=================================");
                    System.Diagnostics.Debug.WriteLine("Vendor           : " + _Camera.Parameters[PLCamera.DeviceVendorName].GetValue());

                    string cam_model = _Camera.Parameters[PLCamera.DeviceModelName].GetValue();
                    string ID = _Camera.Parameters[PLCamera.DeviceSerialNumber].GetValue();

                    System.Diagnostics.Debug.WriteLine("Model            : " + cam_model);
                    System.Diagnostics.Debug.WriteLine("Serial number    : " + ID);
                    System.Diagnostics.Debug.WriteLine("Firmware version : " + _Camera.Parameters[PLCamera.DeviceFirmwareVersion].GetValue());
                    System.Diagnostics.Debug.WriteLine("=================================");
                    System.Diagnostics.Debug.WriteLine("");

                    System.Diagnostics.Debug.WriteLine("Camera Device Settings");
                    System.Diagnostics.Debug.WriteLine("=================================");

                    double max_gain = _Camera.Parameters[PLCamera.Gain].GetMaximum();
                    double gain = _Camera.Parameters[PLCamera.Gain].GetValue();
                    double min_gain = _Camera.Parameters[PLCamera.Gain].GetMinimum();

                    System.Diagnostics.Debug.WriteLine("Gain             : {0}", gain);
                    System.Diagnostics.Debug.WriteLine("Max Gain         : {0}", max_gain);
                    System.Diagnostics.Debug.WriteLine("Min Gain         : {0}", min_gain);

                    double max_exposure = _Camera.Parameters[PLCamera.ExposureTime].GetMaximum() / 1000;
                    double exposure = _Camera.Parameters[PLCamera.ExposureTime].GetValue() / 1000;
                    double min_exposure = _Camera.Parameters[PLCamera.ExposureTime].GetMinimum() / 1000;

                    System.Diagnostics.Debug.WriteLine("Exposure         : {0}", exposure);
                    System.Diagnostics.Debug.WriteLine("Max Exposure     : {0}", max_exposure);
                    System.Diagnostics.Debug.WriteLine("Min Exposure     : {0}", min_exposure);


                    _Camera.Parameters[PLCamera.AcquisitionFrameRateEnable].SetValue(true);
                    _Camera.Parameters[PLCamera.AcquisitionFrameRate].TrySetValue(500);

                    double max_frame_rate = _Camera.Parameters[PLCamera.AcquisitionFrameRate].GetMaximum();
                    double min_frame_rate = _Camera.Parameters[PLCamera.AcquisitionFrameRate].GetMinimum();
                    double frame_rate = _Camera.Parameters[PLCamera.AcquisitionFrameRate].GetValue();

                    System.Diagnostics.Debug.WriteLine("Frame Rate       : {0}", frame_rate);
                    System.Diagnostics.Debug.WriteLine("Max Frame Rate   : {0}", max_frame_rate);
                    System.Diagnostics.Debug.WriteLine("Min Frame Rate   : {0}", min_frame_rate);


                    long max_width = _Camera.Parameters[PLCamera.Width].GetMaximum();
                    long max_height = _Camera.Parameters[PLCamera.Height].GetMaximum();
                    max_width = max_width >= 720 ? 720 : 640;
                    max_height = max_height >= 540 ? 540 : 480;
                    _Camera.Parameters[PLCamera.Width].TrySetValue(max_width);
                    _Camera.Parameters[PLCamera.Height].TrySetValue(max_height);

                    _Camera.Parameters[PLCamera.OffsetX].TrySetValue(0);
                    _Camera.Parameters[PLCamera.OffsetY].TrySetValue(0);

                    System.Diagnostics.Debug.WriteLine("OffsetX          : {0}", _Camera.Parameters[PLCamera.OffsetX].GetValue());
                    System.Diagnostics.Debug.WriteLine("OffsetY          : {0}", _Camera.Parameters[PLCamera.OffsetY].GetValue());

                    int VisionWidth = (int)_Camera.Parameters[PLCamera.Width].GetValue();
                    int VisionHeight = (int)_Camera.Parameters[PLCamera.Height].GetValue();
                    System.Diagnostics.Debug.WriteLine("Width            : {0}", VisionWidth);
                    System.Diagnostics.Debug.WriteLine("Height           : {0}", VisionHeight);


                    // Set an enum parameter.
                    string oldPixelFormat = _Camera.Parameters[PLCamera.PixelFormat].GetValue(); // Remember the current pixel format.
                    System.Diagnostics.Debug.WriteLine("Old PixelFormat  : " + _Camera.Parameters[PLCamera.PixelFormat].GetValue());


                    // Set pixel format to Mono12 if available.
                    if (_Camera.Parameters[PLCamera.PixelFormat].TrySetValue(PLCamera.PixelFormat.Mono12))
                    {
                        System.Diagnostics.Debug.WriteLine("New PixelFormat  : " + _Camera.Parameters[PLCamera.PixelFormat].GetValue());
                    }
                    System.Diagnostics.Debug.WriteLine("=================================");
                    // Some camera models may have auto functions enabled. To set the gain value to a specific value,
                    // the Gain Auto function must be disabled first (if gain auto is available).
                    _Camera.Parameters[PLCamera.GainAuto].TrySetValue(PLCamera.GainAuto.Off); // Set GainAuto to Off if it is writable.

                    EnableCameraFreeRunMode();
                }
            }
            catch (Exception e)
            {
                _Camera.Close();
                throw e;
            }

            _Camera.StreamGrabber.ImageGrabbed += OnImageGrabbed;
            //_Camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
        }

        private Camera _Camera;

        public BaslerCamera(ICameraInfo camera_info)
        {
            _Camera = new Camera(camera_info);
            InitCamera();

            return;
        }
        private void OnImageGrabbed(Object sender, ImageGrabbedEventArgs e)
        {
            IGrabResult grabResult = e.GrabResult;

            if (grabResult.GrabSucceeded)
            {
                Mat mat = new Mat(grabResult.Height, grabResult.Width, MatType.CV_16U, grabResult.PixelData as byte[]);
                mat.ConvertTo(mat, MatType.CV_16U, 16);

                int frame_group = 0;
                long timestamp = 0;
                switch (_RunningMode)
                {
                    case CameraRunningMode.Trigger:
                        long line_status = grabResult.ChunkData[PLChunkData.ChunkLineStatusAll].GetValue();
                        frame_group = (int)((line_status >> 2) & 0x0000000000000003);
                        timestamp = grabResult.ChunkData[PLChunkData.ChunkTimestamp].GetValue();
                        break;

                    case CameraRunningMode.FreeRun:
                    default:
                        timestamp = grabResult.Timestamp;
                        frame_group = -1;
                        break;
                }
                MarkedMat mmat = new MarkedMat() { Group = frame_group, Timestamp = timestamp, ImageMat = mat };

                DidGrabImage?.Invoke(this, mmat);
            }
            else
            {
                if (grabResult.ErrorCode == 0x1f)
                {
                    return;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Error: {0} {1}", grabResult.ErrorCode, grabResult.ErrorDescription);
                    _Camera.StreamGrabber.Stop();
                    _Camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
                }
            }
        }


        public void Close()
        {
            _Camera.Close();
        }


        public double CurrentExposure
        {
            get
            {
                double exposure = _Camera.Parameters[PLCamera.ExposureTime].GetValue() / 1000;
                return (int)(exposure * 100) / 100.0;
            }
        }


        public double CurrentFrameRate
        {
            get
            {
                double frame_rate = _Camera.Parameters[PLCamera.AcquisitionFrameRate].GetValue();
                return (int)(frame_rate * 100) / 100.0;
            }
        }


        public double CurrentGain
        {
            get
            {
                double gain = _Camera.Parameters[PLCamera.Gain].GetValue();
                return (int)(gain * 100) / 100.0;
            }
        }


        public double MaximumExposure
        {
            get
            {
                double max_exposure = _Camera.Parameters[PLCamera.ExposureTime].GetMaximum();
                max_exposure = max_exposure > 100000 ? 100000 : max_exposure;
                return max_exposure / 1000;
            }
        }


        public double MaximumFrameRate
        {
            get
            {
                double max_frame_rate = _Camera.Parameters[PLCamera.AcquisitionFrameRate].GetMaximum();
                double exposure = this.CurrentExposure;
                exposure = exposure > 3 ? exposure : 3;
                double exposure_frames = 1000 / (exposure + 0.125);
                max_frame_rate = max_frame_rate > exposure_frames ? exposure_frames : max_frame_rate;
                return (int)(max_frame_rate * 100) / 100.0;
            }
        }


        public double MaximumGain
        {
            get
            {
                double max_gain = _Camera.Parameters[PLCamera.Gain].GetMaximum();
                return ((int)(max_gain * 100) / 100.0) - 0.01;
            }
        }


        public double MinimumExposure
        {
            get
            {
                double min_exposure = _Camera.Parameters[PLCamera.ExposureTime].GetMinimum();
                return min_exposure / 1000;
            }
        }


        public double MinimumFrameRate
        {
            get
            {
                double min_frame_rate = _Camera.Parameters[PLCamera.AcquisitionFrameRate].GetMinimum();
                return (int)(min_frame_rate * 100) / 100.0 + 0.1;
            }
        }


        public double MinimumGain
        {
            get
            {
                double min_gain = _Camera.Parameters[PLCamera.Gain].GetMinimum();
                return ((int)(min_gain * 100) / 100.0) + 0.01;
            }
        }


        public void Open()
        {
            _Camera.Open(30000, TimeoutHandling.Return);
        }


        public double SetExposure(double exposure)
        {
            var exValue = exposure * 1000;
            exValue = exValue < 1000 ? 1000 : exValue;
            _Camera.Parameters[PLCamera.ExposureTime].TrySetValue(exValue);

            return this.CurrentExposure;
        }


        public double SetFrameRate(double frame_rate)
        {
            _Camera.Parameters[PLCamera.AcquisitionFrameRate].SetValue(frame_rate);
            return this.CurrentFrameRate;
        }


        public double SetGain(double gain)
        {
            _Camera.Parameters[PLCamera.Gain].TrySetValue(gain);
            return (int)(_Camera.Parameters[PLCamera.Gain].GetValue() * 100) / 100.0;
        }


        public int VisionWidth
        {
            get
            {
                return (int)_Camera.Parameters[PLCamera.Width].GetValue();
            }
        }


        public int VisionHeight
        {
            get
            {
                return (int)_Camera.Parameters[PLCamera.Height].GetValue();
            }
        }
    }
}
