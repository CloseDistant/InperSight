using System;
using InperStudioControlLib.Lib.DeviceAgency.CameraDept;
using InperStudioControlLib.Lib.DeviceAgency.ControlDept;
using OpenCvSharp;

namespace InperStudioControlLib.Lib.DeviceAgency
{
    //public class MarkedMat
    //{
    //    public int Group { get; set; } = 0;
    //    public long Timestamp { get; set; } = 0;
    //    public Mat ImageMat { get; set; }
    //}


    //public class DevPhotometry
    //{
    //    private static readonly DevPhotometry _Instance = new DevPhotometry();
    //    public static DevPhotometry Instance => _Instance;


    //    private USARTOperator _USARTOpe = USARTOperator.Instance;
    //    private CameraAgent _CamAgent = CameraAgent.Instance;

    //    private BaslerCamera _BaslerCam;

    //    private DevPhotometry()
    //    {
    //        _CamAgent.CameraStatusChanged += CameraStatusChanged;
    //        _USARTOpe.PortStatusChanged += PortStatusChanged;
    //        _USARTOpe.LightStatusChanged += LightStatusChanged;

    //        try
    //        {
    //            _BaslerCam = _CamAgent.CreateCamera();
    //            if (_BaslerCam == null)
    //            {
    //                HandyControl.Controls.Growl.Error("Failed to create basler camera");
    //                return;
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            HandyControl.Controls.Growl.Error("On creat basler camera " + ex.Message);
    //            return;
    //        }

    //        _BaslerCam.DidGrabImage += ImageGrabbed;
    //        _USARTOpe.GetDeviceConfig();
    //    }

    //    #region events handlers
    //    public event EventHandler<MarkedMat> OnImageGrabbed;
    //    private void ImageGrabbed(object sender, MarkedMat mmat)
    //    {
    //        OnImageGrabbed?.Invoke(this, mmat);
    //    }


    //    public event EventHandler<CameraStatusChangedEventArgs> OnCameraStatusChanged;
    //    private void CameraStatusChanged(object sender, CameraStatusChangedEventArgs e)
    //    {
    //        OnCameraStatusChanged?.Invoke(this, e);
    //        return;
    //    }


    //    public event EventHandler<PortStatusChangedEventArgs> OnPortStatusChanged;
    //    private void PortStatusChanged(object sender, PortStatusChangedEventArgs e)
    //    {
    //        OnPortStatusChanged?.Invoke(this, e);
    //        return;
    //    }


    //    public short Light0WaveLength => _USARTOpe.Light0WaveLength;
    //    public short Light1WaveLength => _USARTOpe.Light1WaveLength;
    //    public short Light2WaveLength => _USARTOpe.Light2WaveLength;
    //    public short Light3WaveLength => _USARTOpe.Light3WaveLength;

    //    public event EventHandler<LightStatusChangedEventArgs> OnLightStatusChanged;
    //    private void LightStatusChanged(object sender, LightStatusChangedEventArgs e)
    //    {
    //        OnLightStatusChanged?.Invoke(this, e);
    //        return;
    //    }
    //    #endregion


    //    #region commands
    //    public double SetFrameRate(double frame_rate)
    //    {
    //        return _USARTOpe.SetFrameRate(frame_rate);
    //    }


    //    public double SetExposure(double exposure)
    //    {
    //        return _BaslerCam.SetExposure(exposure);
    //    }


    //    public double SetGain(double gain)
    //    {
    //        return _BaslerCam.SetGain(gain);
    //    }


    //    public void SwitchLight(int light_no, bool enable)
    //    {
    //        _USARTOpe.SwitchLight(light_no, enable);
    //    }


    //    public void SetLightPower(int light_no, double power_percent)
    //    {
    //        _USARTOpe.SetLightPower(light_no, power_percent);
    //    }
    //    #endregion


    //    #region properties
    //    public double CurrentExposure => _BaslerCam.GetCurrentExposure();

    //    public double CurrentFrameRate => _BaslerCam.GetCurrentFrameRate();

    //    public double CurrentGain => _BaslerCam.GetCurrentGain();

    //    public double MaximumExposure => _BaslerCam.GetMaximumExposure();

    //    public double MaximumFrameRate => _BaslerCam.GetMaximumFrameRate();

    //    public double MaximumGain => _BaslerCam.GetMaximumGain();

    //    public double MinimumExposure => _BaslerCam.GetMinimumExposure();

    //    public double MinimumFrameRate => _BaslerCam.GetMinimumFrameRate();

    //    public double MinimumGain => _BaslerCam.GetMinimumGain();

    //    public int VisionWidth => _BaslerCam.GetVisionWidth();

    //    public int VisionHeight => _BaslerCam.GetVisionHeight();
    //    #endregion
    //}
}
