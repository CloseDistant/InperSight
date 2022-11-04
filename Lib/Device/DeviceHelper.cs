using InperSight.Lib.Bean;
using InperSight.Lib.Config;
using InperVideo.Camera;
using InperVideo.Interfaces;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InperSight.Lib.Device
{
    public class DeviceHelper
    {
        static DeviceHelper deviceHelper;

        public static DeviceHelper GetDeviceHelper()
        {
            if (deviceHelper == null)
            {
                Interlocked.CompareExchange(ref deviceHelper, new DeviceHelper(), null);
            }
            return deviceHelper;
        }
        public KeyValuePair<int, ICameraInfo>? GetCameraInfo()
        {
            KeyValuePair<int, ICameraInfo>? keyValuePair = null;
            IEnumerable<ICameraInfo> camerInfoList = new CameraInfoesReader().GetCameraInfos();
            int index = 0;
            foreach (var info in camerInfoList)
            {
                if (info.DevicePath.Contains("vid_04b4"))
                {
                    keyValuePair = new KeyValuePair<int, ICameraInfo>(index, info);
                    string _vid = info.DevicePath.Substring(info.DevicePath.IndexOf("vid_") + 4, 4);
                    string _pid = info.DevicePath.Substring(info.DevicePath.IndexOf("pid_") + 4, 4);
                    LoggerHelper.Info("vid=" + _vid);
                    LoggerHelper.Info("pid=" + _pid);
                }
                index++;
            }

            return keyValuePair;
        }
    }
}
