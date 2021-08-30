using DirectShowLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows;

namespace InperStudio.Lib.Helper
{
    public class InperComputerInfoHelper
    {
        public static InperComputerInfoHelper Instance { get; } = new InperComputerInfoHelper();

        public string CpuID;
        public string MacAddress;
        public string DiskID;
        public string IpAddress;
        public string LoginUserName;
        public string ComputerName;
        public string SystemType;

        public List<KeyValuePair<int, string>> ListCamerasData;


        private InperComputerInfoHelper()
        {
            CpuID = GetCpuID();
            MacAddress = GetMacAddress();
            DiskID = GetDiskID();
            IpAddress = GetIPAddress();
            LoginUserName = GetUserName();
            SystemType = GetSystemType();
            ComputerName = GetComputerName();
            GetCameraList();
        }


        string GetCpuID()
        {
            try
            {
                string cpuInfo = String.Empty;
                ManagementClass mc = new ManagementClass("Win32_Processor");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                }
                moc = null;
                mc = null;
                return cpuInfo;
            }
            catch
            {
                return "unknow";
            }
            finally { }
        }


        string GetMacAddress()
        {
            try
            {
                string mac = String.Empty;
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        mac = mo["MacAddress"].ToString();
                        break;
                    }
                }
                moc = null;
                mc = null;
                return mac;
            }
            catch
            {
                return "unknow";
            }
            finally { }
        }


        string GetDiskID()
        {
            try
            {
                String HDid = String.Empty;
                ManagementClass mc = new ManagementClass("Win32_DiskDrive");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    HDid = (string)mo.Properties["Model"].Value;
                }
                moc = null;
                mc = null;
                return HDid;
            }
            catch
            {
                return "unknow";
            }
            finally { }
        }


        string GetIPAddress()
        {
            try
            {
                string st = String.Empty;
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        System.Array ar;
                        ar = (System.Array)(mo.Properties["IpAddress"].Value);
                        st = ar.GetValue(0).ToString();
                        break;
                    }
                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return "unknow";
            }
            finally { }
        }


        string GetUserName()
        {
            try
            {
                string un = String.Empty;
                un = Environment.UserName;
                return un;
            }
            catch
            {
                return "unknow";
            }
            finally { }
        }


        string GetComputerName()
        {
            try
            {
                return System.Environment.MachineName;
            }
            catch
            {
                return "unknow";
            }
            finally { }
        }


        string GetSystemType()
        {
            try
            {
                string st = String.Empty;
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    st = mo["SystemType"].ToString();
                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return "unknow";
            }
            finally { }
        }


        string GetTotalPhysicalMemory()
        {
            try
            {
                string st = String.Empty;
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    st = mo["TotalPhysicalMemory"].ToString();
                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return "unknow";
            }
            finally { }
        }

        void GetCameraList()
        {
            ListCamerasData = new List<KeyValuePair<int, string>>();

            DsDevice[] _SystemCamereas = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            int _DeviceIndex = 0;
            foreach (DsDevice _Camera in _SystemCamereas)
            {
                ListCamerasData.Add(new KeyValuePair<int, string>(_DeviceIndex, _Camera.Name));
                _DeviceIndex++;
            }
            return;
        }
        public static void SaveFrameworkElementToImage(FrameworkElement ui, string filename, string path)
        {
            System.IO.FileStream ms = new System.IO.FileStream(filename, System.IO.FileMode.Create);
            System.Windows.Media.Imaging.RenderTargetBitmap bmp = new System.Windows.Media.Imaging.RenderTargetBitmap((int)ui.ActualWidth, (int)ui.ActualHeight, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            bmp.Render(ui);
            System.Windows.Media.Imaging.JpegBitmapEncoder encoder = new System.Windows.Media.Imaging.JpegBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bmp));
            encoder.Save(ms);
            ms.Close();

            File.Copy(filename, path + "/" + filename, true);
            File.Delete(Environment.CurrentDirectory + "/" + filename);
        }

        public static void SaveScreenToImageByPoint(int left, int top, int width, int height,string path)
        {
            Image image = new Bitmap(width, height);
            Graphics gc = Graphics.FromImage(image);
            gc.CopyFromScreen(left, top, 0, 0, new System.Drawing.Size(width, height));

            image.Save(path, System.Drawing.Imaging.ImageFormat.Bmp);
        }
    }
}
