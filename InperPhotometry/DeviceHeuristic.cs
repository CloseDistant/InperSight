using Basler.Pylon;
using InperDeviceManagement;
using InperProtocolStack;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InperPhotometry
{
    public class DeviceHeuristic
    {
        public static readonly DeviceHeuristic Instance = new DeviceHeuristic();
        private DeviceHeuristic()
        {
            UpdateDeviceList();
        }


        public List<PhotometryDevice> DeviceList { get; private set; } = new List<PhotometryDevice>();

        public void UpdateDeviceList()
        {
            var cam_info_list = FindAllBaslerCameras();
            UsbRegistry com_port_list = FindAllPhotometryUsb();
            //var com_port_list = FindAllPhotometryComPorts();

            //if (cam_info_list.Count > 0 && com_port_list.Count > 0)
            //{
            //    var cam_info = cam_info_list.First();
            //    var com_name = com_port_list.First();
            //    DeviceList.Add(new PhotometryDevice(new ControlShell(com_name), new BaslerCamera(cam_info)));
            //}
            if (cam_info_list.Count > 0 && com_port_list != null)
            {
                var cam_info = cam_info_list.First();
                DeviceList.Add(new PhotometryDevice(new UsbControlShell(com_port_list.Vid, com_port_list.Pid), new BaslerCamera(cam_info)));
            }

            return;
        }
        #region Find Internal Device Cameras
        private List<ICameraInfo> FindAllBaslerCameras()
        {
            List<ICameraInfo> cam_info_list = new List<ICameraInfo>();

            foreach (var ci in CameraFinder.Enumerate(DeviceType.Usb))
            {
                if (ci[CameraInfoKey.ModelName] == "acA720-520um")
                {
                    cam_info_list.Add(ci);
                }
            }

            return cam_info_list;
        }
        #endregion


        #region Find Communication Ports
        private SerialPort GetDevice(string PName)
        {
            SerialPort com = new SerialPort();
            com.BaudRate = 115200;//9216000;
            com.Parity = Parity.None;
            com.DataBits = 8;
            com.StopBits = StopBits.One;
            com.Encoding = Encoding.ASCII;
            com.PortName = PName;

            return com;
        }


        private readonly string _DeviceVIDStr = "1904";
        private readonly int _DeviceVID = 0x1904;
        private readonly string _DevicePIDStr = "5741";
        private readonly int _DevicePID = 0x5741;
        private HashSet<string> GetComNames(string vid, string pid)
        {
            string pattern = string.Format("^VID_{0}.PID_{1}", vid, pid);
            Regex _rx = new Regex(pattern, RegexOptions.IgnoreCase);
            HashSet<string> comports = new HashSet<string>();

            RegistryKey rk1 = Registry.LocalMachine;
            RegistryKey rk2 = rk1.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum");

            foreach (string s3 in rk2.GetSubKeyNames())
            {
                RegistryKey rk3 = rk2.OpenSubKey(s3);
                foreach (string s in rk3.GetSubKeyNames())
                {
                    if (_rx.Match(s).Success)
                    {
                        RegistryKey rk4 = rk3.OpenSubKey(s);
                        foreach (string s2 in rk4.GetSubKeyNames())
                        {
                            RegistryKey rk5 = rk4.OpenSubKey(s2);
                            RegistryKey rk6 = rk5.OpenSubKey("Device Parameters");
                            comports.Add((string)rk6.GetValue("PortName"));
                        }
                    }
                }
            }

            return comports;
        }

        private UsbRegistry FindAllPhotometryUsb()
        {
            UsbRegDeviceList allDevices = UsbDevice.AllDevices;
            foreach (UsbRegistry usbRegistry in allDevices)
            {
                if (usbRegistry.Name.Contains("Inper USB Port"))
                {
                    return usbRegistry;
                }
            }
            return null;
        }
        private List<string> FindAllPhotometryComPorts()
        {
            HashSet<string> port_names = GetComNames(vid: _DeviceVIDStr, pid: _DevicePIDStr);
            List<string> active_port_names = new List<string>();

            foreach (string pn in port_names)
            {
                SerialPort com = GetDevice(pn);
                if (com.IsOpen == false)
                {
                    try
                    {
                        com.Open();
                        active_port_names.Add(pn);
                        System.Diagnostics.Debug.WriteLine("Optogenetics found at " + pn);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message);
                    }
                    finally
                    {
                        if (com.IsOpen)
                        {
                            com.Close();
                        }
                        com.Dispose();
                    }
                }
            }
            return active_port_names;
        }
        #endregion
    }
}
