using InperProtocolStack;
using InperProtocolStack.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperDeviceManagement
{
    public enum IOMode
    {
        IOM_INPUT,
        IOM_OUTPUT
    }
    public class PhotometryDevice
    {
        public UsbControlShell _CS;
        private BaslerCamera _BC;

        public List<LightDesc> LightSourceList => _CS.LightSourceList;


        public Dictionary<string, uint> DeviceIOIDs { get; private set; } = new Dictionary<string, uint>();


        public void SyncDeviceInfo()
        {
            InqLightConfig();

            return;
        }

        public event EventHandler<DevInfoUpdatedEventArgs> OnDevInfoUpdated;
        public event EventHandler<DevNotificationEventArgs> OnDevNotification;

        public PhotometryDevice(UsbControlShell cs, BaslerCamera bc)
        {
            _CS = cs;
            _BC = bc;
            _CS.OnDevInfoUpdated += DevInfoUpdated;
            _CS.OnDevNotification += DevNotified;
            _BC.DidGrabImage += OnImageGrabbed;

            // stub of device IO
            DeviceIOIDs = new Dictionary<string, uint>
            {
                ["DIO-1"] = 0,
                ["DIO-2"] = 1,
                ["DIO-3"] = 2,
                ["DIO-4"] = 3,
                ["DIO-5"] = 4,
                ["DIO-6"] = 5,
                ["DIO-7"] = 6,
                ["DIO-8"] = 7,
            };

            // SyncDeviceInfo();
        }

        private void DevInfoUpdated(object sender, DevInfoUpdatedEventArgs e)
        {
            OnDevInfoUpdated?.Invoke(this, e);
            return;
        }


        private void DevNotified(object sender, DevNotificationEventArgs e)
        {
            OnDevNotification?.Invoke(this, e);
            return;
        }


        public event EventHandler<MarkedMat> DidGrabImage;
        private void OnImageGrabbed(Object sender, MarkedMat mmat)
        {
            DidGrabImage?.Invoke(this, mmat);
        }

        #region Support From Basler Camera
        public double GetCurrentGain()
        {
            return _BC.CurrentGain;
        }

        public double SetGain(double gain)
        {
            return _BC.SetGain(gain);
        }

        public double GetMaximumGain()
        {
            return _BC.MaximumGain;
        }

        public double GetMinimumGain()
        {
            return _BC.MinimumGain;
        }


        public double GetCurrentExposure()
        {
            return _BC.CurrentExposure;
        }

        public double SetExposure(double exposure)
        {
            return _BC.SetExposure(exposure);
        }

        public double GetMaximumExposure()
        {
            return _BC.MaximumExposure;
        }

        public double GetMinimumExposure()
        {
            return _BC.MinimumExposure;
        }


        public double GetCurrentFrameRate()
        {
            return _BC.CurrentFrameRate;
        }

        public double GetMaximumFrameRate()
        {
            return _BC.MaximumFrameRate;
        }

        public double GetMinimumFrameRate()
        {
            return _BC.MinimumFrameRate;
        }


        public int GetVisionWidth()
        {
            return _BC.VisionWidth;
        }

        public int GetVisionHeight()
        {
            return _BC.VisionHeight;
        }

        public CameraRunningMode RunningMode => _BC.RunningMode;
        #endregion


        #region Support From Control Shell
        public void SetFrameRate(double frame_rate)
        {
            _CS.SetFrameRate(frame_rate);
        }


        public void SwitchLight(uint light_id, bool enable)
        {
            _CS.SwitchLight(light_id, enable);
        }


        public void SetLightPower(uint light_id, double power_in_percent)
        {
            _CS.SetLightPower(light_id, power_in_percent / 100);
        }


        public void SaveID(uint software_id, uint hardware_id, string camera_id)
        {
            _CS.SaveID(software_id, hardware_id, camera_id);
        }


        public void InqID()
        {
            _CS.InqID();
        }


        public void InqLightConfig()
        {
            _CS.InqLightConfig();
        }


        public void SetMeasureMode(bool enable)
        {
            _CS.SetMeasureMode(enable);
            if (enable)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }


        public void SetIOMode(uint io_id, IOMode io_mode)
        {
            uint mode = 0;
            if (IOMode.IOM_INPUT == io_mode)
            {
                mode = 0;
            }
            else if (IOMode.IOM_OUTPUT == io_mode)
            {
                mode = 1;
            }

            _CS.SetIOMode(io_id, mode);
        }


        public void OuputIO(uint io_id, uint stat)
        {
            _CS.OuputIO(io_id, stat);
        }

        #endregion

        public void AdIsCollect(bool isStart)
        {
            _CS.ADIsStartCollect(isStart);
        }
        public void SetAdframeRate(uint frameRate, uint[] array)
        {
            _CS.SetAdframeRate(frameRate, array);
        }
        public void RemoveAdSampling()
        {
            _CS.RemoveAdSampling();
        }
        public void Start()
        {
            _BC.SwitchRunningMode(CameraRunningMode.Trigger);
            _CS.Start();
            return;
        }


        public void Stop()
        {
            _CS.Stop();
            _BC.SwitchRunningMode(CameraRunningMode.FreeRun);
            return;
        }
    }
}
