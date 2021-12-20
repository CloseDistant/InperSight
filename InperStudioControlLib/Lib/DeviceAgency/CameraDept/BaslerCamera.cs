using System;
using System.Collections.Generic;
//using Basler.Pylon;
using OpenCvSharp;

namespace InperStudioControlLib.Lib.DeviceAgency.CameraDept
{
    //public class BaslerCamera
    //{
    //    private readonly List<string> _TriggerableCamModel = new List<string>
    //    {
    //        "acA720-520um"
    //    };


    //    private bool _CameraIsTriggerable = false;
    //    public bool CameraIsInTriggerMode = false;

    //    public event EventHandler<MarkedMat> DidGrabImage;

    //    public void Reset()
    //    {
    //        if (cam == null) { return; }
    //        cam.StreamGrabber.ImageGrabbed -= OnImageGrabbed;
    //        cam.Close();
    //        // Set the acquisition mode to free running continuous acquisition when the camera is opened.
    //        cam.CameraOpened += Configuration.AcquireContinuous;
    //        try
    //        {
    //            cam.Open(30000, TimeoutHandling.Return);
    //            // DeviceVendorName, DeviceModelName, and DeviceFirmwareVersion are string parameters.
    //            System.Diagnostics.Debug.WriteLine("Camera Device Information");
    //            System.Diagnostics.Debug.WriteLine("=================================");
    //            System.Diagnostics.Debug.WriteLine("Vendor           : " + cam.Parameters[PLCamera.DeviceVendorName].GetValue());

    //            string cam_model = cam.Parameters[PLCamera.DeviceModelName].GetValue();
    //            if (_TriggerableCamModel.Contains(cam_model))
    //            {
    //                _CameraIsTriggerable = true;
    //            }
    //            else
    //            {
    //                _CameraIsTriggerable = false;
    //            }
    //            System.Diagnostics.Debug.WriteLine("Model            : " + cam_model);
    //            System.Diagnostics.Debug.WriteLine("Firmware version : " + cam.Parameters[PLCamera.DeviceFirmwareVersion].GetValue());
    //            System.Diagnostics.Debug.WriteLine("=================================");
    //            System.Diagnostics.Debug.WriteLine("");

    //            System.Diagnostics.Debug.WriteLine("Camera Device Settings");
    //            System.Diagnostics.Debug.WriteLine("=================================");

    //            double max_gain = cam.Parameters[PLCamera.Gain].GetMaximum();
    //            double gain = cam.Parameters[PLCamera.Gain].GetValue();
    //            double min_gain = cam.Parameters[PLCamera.Gain].GetMinimum();

    //            System.Diagnostics.Debug.WriteLine("Gain             : {0}", gain);
    //            System.Diagnostics.Debug.WriteLine("Max Gain         : {0}", max_gain);
    //            System.Diagnostics.Debug.WriteLine("Min Gain         : {0}", min_gain);

    //            double max_exposure = cam.Parameters[PLCamera.ExposureTime].GetMaximum() / 1000;
    //            double exposure = cam.Parameters[PLCamera.ExposureTime].GetValue() / 1000;
    //            double min_exposure = cam.Parameters[PLCamera.ExposureTime].GetMinimum() / 1000;

    //            System.Diagnostics.Debug.WriteLine("Exposure         : {0}", exposure);
    //            System.Diagnostics.Debug.WriteLine("Max Exposure     : {0}", max_exposure);
    //            System.Diagnostics.Debug.WriteLine("Min Exposure     : {0}", min_exposure);


    //            cam.Parameters[PLCamera.AcquisitionFrameRateEnable].SetValue(true);
    //            if (_CameraIsTriggerable)
    //            {
    //                cam.Parameters[PLCamera.AcquisitionFrameRate].TrySetValue(500);
    //            }
    //            else
    //            {
    //                cam.Parameters[PLCamera.AcquisitionFrameRate].TrySetValue(30);
    //            }

    //            double max_frame_rate = cam.Parameters[PLCamera.AcquisitionFrameRate].GetMaximum();
    //            double min_frame_rate = cam.Parameters[PLCamera.AcquisitionFrameRate].GetMinimum();
    //            double frame_rate = cam.Parameters[PLCamera.AcquisitionFrameRate].GetValue();

    //            System.Diagnostics.Debug.WriteLine("Frame Rate       : {0}", frame_rate);
    //            System.Diagnostics.Debug.WriteLine("Max Frame Rate   : {0}", max_frame_rate);
    //            System.Diagnostics.Debug.WriteLine("Min Frame Rate   : {0}", min_frame_rate);


    //            long max_width = cam.Parameters[PLCamera.Width].GetMaximum();
    //            long max_height = cam.Parameters[PLCamera.Height].GetMaximum();
    //            max_width = max_width >= 720 ? 720 : 640;
    //            max_height = max_height >= 540 ? 540 : 480;
    //            cam.Parameters[PLCamera.Width].TrySetValue(max_width);
    //            cam.Parameters[PLCamera.Height].TrySetValue(max_height);

    //            cam.Parameters[PLCamera.OffsetX].TrySetValue(0);
    //            cam.Parameters[PLCamera.OffsetY].TrySetValue(0);

    //            System.Diagnostics.Debug.WriteLine("OffsetX          : {0}", cam.Parameters[PLCamera.OffsetX].GetValue());
    //            System.Diagnostics.Debug.WriteLine("OffsetY          : {0}", cam.Parameters[PLCamera.OffsetY].GetValue());

    //            int VisionWidth = (int)cam.Parameters[PLCamera.Width].GetValue();
    //            int VisionHeight = (int)cam.Parameters[PLCamera.Height].GetValue();
    //            System.Diagnostics.Debug.WriteLine("Width            : {0}", VisionWidth);
    //            System.Diagnostics.Debug.WriteLine("Height           : {0}", VisionHeight);


    //            // Set an enum parameter.
    //            string oldPixelFormat = cam.Parameters[PLCamera.PixelFormat].GetValue(); // Remember the current pixel format.
    //            System.Diagnostics.Debug.WriteLine("Old PixelFormat  : " + cam.Parameters[PLCamera.PixelFormat].GetValue());


    //            // Set pixel format to Mono12 if available.
    //            if (cam.Parameters[PLCamera.PixelFormat].TrySetValue(PLCamera.PixelFormat.Mono12))
    //            {
    //                System.Diagnostics.Debug.WriteLine("New PixelFormat  : " + cam.Parameters[PLCamera.PixelFormat].GetValue());
    //            }
    //            System.Diagnostics.Debug.WriteLine("=================================");
    //            // Some camera models may have auto functions enabled. To set the gain value to a specific value,
    //            // the Gain Auto function must be disabled first (if gain auto is available).
    //            cam.Parameters[PLCamera.GainAuto].TrySetValue(PLCamera.GainAuto.Off); // Set GainAuto to Off if it is writable.

    //            if (_CameraIsTriggerable)
    //            {
    //                //// Select and enable the Frame Start trigger
    //                cam.Parameters[PLCamera.TriggerSelector].SetValue(PLCamera.TriggerSelector.FrameStart);
    //                cam.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.On);
    //                cam.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerSource.Line1);
    //                cam.Parameters[PLCamera.TriggerActivation].SetValue(PLCamera.TriggerActivation.RisingEdge);

    //                cam.Parameters[PLCamera.LineSelector].SetValue(PLCamera.LineSelector.Line2);
    //                cam.Parameters[PLCamera.LineMode].SetValue(PLCamera.LineMode.Output);
    //                cam.Parameters[PLCamera.LineSource].SetValue(PLCamera.LineSource.FrameTriggerWait);

    //                cam.Parameters[PLCamera.LineSelector].SetValue(PLCamera.LineSelector.Line3);
    //                cam.Parameters[PLCamera.LineMode].SetValue(PLCamera.LineMode.Input);
    //                cam.Parameters[PLCamera.LineInverter].SetValue(false);

    //                cam.Parameters[PLCamera.LineSelector].SetValue(PLCamera.LineSelector.Line4);
    //                cam.Parameters[PLCamera.LineMode].SetValue(PLCamera.LineMode.Input);
    //                cam.Parameters[PLCamera.LineInverter].SetValue(false);

    //                cam.Parameters[PLCamera.ChunkModeActive].SetValue(true);
    //                cam.Parameters[PLCamera.ChunkSelector].SetValue(PLCamera.ChunkSelector.LineStatusAll);
    //                cam.Parameters[PLCamera.ChunkEnable].SetValue(true);

    //                cam.Parameters[PLCamera.ChunkSelector].SetValue(PLCamera.ChunkSelector.Timestamp);
    //                cam.Parameters[PLCamera.ChunkEnable].SetValue(true);

    //                CameraIsInTriggerMode = true;
    //            }
    //            else
    //            {
    //                cam.Parameters[PLCamera.TriggerSelector].SetValue(PLCamera.TriggerSelector.FrameStart);
    //                cam.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.Off);

    //                cam.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
    //                cam.Parameters[PLCamera.AcquisitionStart].Execute();

    //                CameraIsInTriggerMode = false;
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            cam.Close();
    //            throw e;
    //        }

    //        cam.StreamGrabber.ImageGrabbed += OnImageGrabbed;
    //        cam.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
    //    }

    //    private Camera cam;

    //    public BaslerCamera()
    //    {
    //        List<ICameraInfo> cam_list = CameraFinder.Enumerate(DeviceType.Usb);
    //        foreach (var c in cam_list)
    //        {
    //            if (c[CameraInfoKey.ModelName] == "acA720-520um")
    //            {
    //                cam = new Camera(c);
    //                break;
    //            }
    //        }

    //        Reset();

    //        return;
    //    }

    //    private void Cam_CameraOpened(object sender, EventArgs e)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    private void OnImageGrabbed(object sender, ImageGrabbedEventArgs e)
    //    {
    //        IGrabResult grabResult = e.GrabResult;

    //        if (grabResult.GrabSucceeded)
    //        {
    //            Mat mat = new Mat(grabResult.Height, grabResult.Width, MatType.CV_16U, grabResult.PixelData as byte[]);
    //            mat.ConvertTo(mat, MatType.CV_16U, 16);

    //            int frame_group = 0;
    //            long timestamp;
    //            if (CameraIsInTriggerMode)
    //            {
    //                long line_status = grabResult.ChunkData[PLChunkData.ChunkLineStatusAll].GetValue();
    //                frame_group = (int)((line_status >> 2) & 0x0000000000000003);
    //                timestamp = grabResult.ChunkData[PLChunkData.ChunkTimestamp].GetValue();
    //            }
    //            else
    //            {
    //                timestamp = grabResult.Timestamp;
    //            }

    //            MarkedMat mmat = new MarkedMat() { Group = frame_group, Timestamp = timestamp, ImageMat = mat };

    //            DidGrabImage?.Invoke(this, mmat);
    //        }
    //        else
    //        {
    //            if (grabResult.ErrorCode == 0x1f)
    //            {
    //                return;
    //            }
    //            else
    //            {
    //                System.Diagnostics.Debug.WriteLine("Error: {0} {1}", grabResult.ErrorCode, grabResult.ErrorDescription);
    //                cam.StreamGrabber.Stop();
    //                cam.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
    //            }
    //        }
    //    }


    //    public void Close()
    //    {
    //        cam.Close();
    //    }


    //    public double GetCurrentExposure()
    //    {
    //        if (cam != null)
    //        {
    //            double exposure = cam.Parameters[PLCamera.ExposureTime].GetValue() / 1000;
    //            return (int)(exposure * 100) / 100.0;
    //        }
    //        return default;
    //    }


    //    public double GetCurrentFrameRate()
    //    {
    //        if (cam != null)
    //        {
    //            double frame_rate = cam.Parameters[PLCamera.AcquisitionFrameRate].GetValue();
    //            return (int)(frame_rate * 100) / 100.0;
    //        }
    //        return default;
    //    }


    //    public double GetCurrentGain()
    //    {
    //        if (cam != null)
    //        {
    //            double gain = cam.Parameters[PLCamera.Gain].GetValue();
    //            return (int)(gain * 100) / 100.0;
    //        }
    //        return default;
    //    }


    //    public double GetMaximumExposure()
    //    {
    //        if (cam != null)
    //        {
    //            double max_exposure = cam.Parameters[PLCamera.ExposureTime].GetMaximum();
    //            max_exposure = max_exposure > 100000 ? 100000 : max_exposure;
    //            return max_exposure / 1000;
    //        }
    //        return default;
    //    }


    //    public double GetMaximumFrameRate()
    //    {
    //        if (cam != null)
    //        {
    //            double max_frame_rate = cam.Parameters[PLCamera.AcquisitionFrameRate].GetMaximum();
    //            double exposure = GetCurrentExposure();
    //            exposure = exposure > 3 ? exposure : 3;
    //            double exposure_frames = 1000 / (exposure + 0.125);
    //            max_frame_rate = max_frame_rate > exposure_frames ? exposure_frames : max_frame_rate;
    //            return (int)(max_frame_rate * 100) / 100.0;
    //        }
    //        return default;
    //    }


    //    public double GetMaximumGain()
    //    {
    //        if (cam != null)
    //        {
    //            double max_gain = cam.Parameters[PLCamera.Gain].GetMaximum();
    //            return ((int)(max_gain * 100) / 100.0) - 0.01;
    //        }
    //        return default;
    //    }


    //    public double GetMinimumExposure()
    //    {
    //        if (cam != null)
    //        {
    //            double min_exposure = cam.Parameters[PLCamera.ExposureTime].GetMinimum();
    //            return min_exposure / 1000;
    //        }
    //        return default;
    //    }


    //    public double GetMinimumFrameRate()
    //    {
    //        if (cam != null)
    //        {
    //            double min_frame_rate = cam.Parameters[PLCamera.AcquisitionFrameRate].GetMinimum();
    //            return (int)(min_frame_rate * 100) / 100.0 + 0.1;
    //        }
    //        return default;
    //    }


    //    public double GetMinimumGain()
    //    {
    //        if (cam != null)
    //        {
    //            double min_gain = cam.Parameters[PLCamera.Gain].GetMinimum();
    //            return ((int)(min_gain * 100) / 100.0) + 0.01;
    //        }
    //        return default;
    //    }


    //    public void Open()
    //    {
    //        cam?.Open(30000, TimeoutHandling.Return);
    //    }


    //    public double SetExposure(double exposure)
    //    {
    //        var exValue = exposure * 1000;
    //        exValue = exValue < 1000 ? 1000 : exValue;
    //        if (cam != null)
    //            cam.Parameters[PLCamera.ExposureTime].TrySetValue(exValue);

    //        return GetCurrentExposure();
    //    }


    //    public double SetFrameRate(double frame_rate)
    //    {
    //        if (cam != null)
    //        {
    //            cam.Parameters[PLCamera.AcquisitionFrameRate].SetValue(frame_rate);
    //            return GetCurrentFrameRate();
    //        }
    //        return default;
    //    }


    //    public double SetGain(double gain)
    //    {
    //        if (cam != null)
    //        {
    //            cam.Parameters[PLCamera.Gain].TrySetValue(gain);
    //            return (int)(cam.Parameters[PLCamera.Gain].GetValue() * 100) / 100.0;
    //        }
    //        return default;
    //    }


    //    public int GetVisionWidth()
    //    {
    //        if (cam != null)
    //            return (int)cam.Parameters[PLCamera.Width].GetValue();
    //        return default;
    //    }


    //    public int GetVisionHeight()
    //    {
    //        if (cam != null)
    //            return (int)cam?.Parameters[PLCamera.Height].GetValue();
    //        return default;
    //    }


    //    public bool IsInTriggerMode()
    //    {
    //        return CameraIsInTriggerMode;
    //    }
    //}
}
