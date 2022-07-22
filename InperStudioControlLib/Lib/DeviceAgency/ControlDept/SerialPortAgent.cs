namespace InperStudioControlLib.Lib.DeviceAgency.ControlDept
{
    //public class OnDataReceivedEventArgs : EventArgs
    //{
    //    private byte[] _Data;
    //    public OnDataReceivedEventArgs(byte[] data)
    //    {
    //        _Data = data;
    //    }
    //    public byte[] Data => _Data;
    //}


    //class SerialPortAgent
    //{
    //    private List<SerialPort> _ComDeviceList = new List<SerialPort>();

    //    public event EventHandler<OnDataReceivedEventArgs> OnDataReceived;
    //    public void RaiseDataReceivedEvent(byte[] data)
    //    {
    //        OnDataReceived?.Invoke(this, new OnDataReceivedEventArgs(data));
    //    }

    //    public static SerialPortAgent Instance { get; } = new SerialPortAgent();

    //    private SerialPortAgent()
    //    {
    //        FindNWDevice();
    //        EnableUSBDevicdMonitor();
    //    }


    //    private SerialPort GetDevice(string PName)
    //    {
    //        SerialPort com = new SerialPort();
    //        com.BaudRate = 9216000;
    //        com.Parity = Parity.None;
    //        com.DataBits = 8;
    //        com.StopBits = StopBits.One;
    //        com.Encoding = Encoding.ASCII;
    //        com.PortName = PName;

    //        return com;
    //    }

    //    private void ClearComDevices()
    //    {
    //        if (_ComDeviceList.Count != 0)
    //        {
    //            foreach (var com in _ComDeviceList)
    //            {
    //                if (com.IsOpen)
    //                {
    //                    com.Close();
    //                }
    //            }
    //            _ComDeviceList.Clear();
    //        }
    //        return;
    //    }

    //    private void FindNWDevice()
    //    {
    //        Task.Run(() =>
    //        {
    //            ClearComDevices();
    //            HashSet<string> port_names = GetComNames(vid: _DeviceVIDStr, pid: _DevicePIDStr);
    //            foreach (string name in port_names)
    //            {
    //                SerialPort com = GetDevice(name);

    //                if (com.IsOpen == false)
    //                {
    //                    try
    //                    {
    //                        com.Open();
    //                        com.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
    //                        _ComDeviceList.Add(com);
    //                        System.Diagnostics.Debug.WriteLine("Photometry found at " + name);
    //                    }
    //                    catch (Exception e)
    //                    {
    //                        System.Diagnostics.Debug.WriteLine(e.Message);
    //                    }
    //                }
    //            }
    //        });

    //        return;
    //    }


    //    private byte[] ReadData(object sender)
    //    {
    //        byte[] ReData = new byte[((SerialPort)sender).BytesToRead];
    //        ((SerialPort)sender).Read(ReData, 0, ReData.Length);
    //        return ReData;
    //    }


    //    private void DataReceived(object sender, SerialDataReceivedEventArgs e)
    //    {
    //        byte[] data = ReadData(sender);
    //        RaiseDataReceivedEvent(data);
    //    }


    //    private bool SendData(SerialPort com, List<byte> data)
    //    {
    //        if (com == null)
    //        {
    //            return false;
    //        }
    //        if (com.IsOpen)
    //        {
    //            byte[] data_barray = data.ToArray();
    //            byte[] tail = { 0x0d, 0x0a };
    //            try
    //            {
    //                com.Write(data_barray, 0, data_barray.Length);
    //                //com.Write(tail, 0, 2);
    //                return true;
    //            }
    //            catch (Exception ex)
    //            {
    //                System.Diagnostics.Debug.WriteLine("On Sending Data to Serial Port: " + ex.Message);
    //            }
    //        }
    //        return false;
    //    }

    //    private object _CommChnLock = new object();
    //    public bool SendData(List<byte> data)
    //    {
    //        lock (_CommChnLock)
    //        {
    //            Parallel.ForEach(_ComDeviceList, com =>
    //            {
    //                SendData(com, data);
    //            });
    //        }

    //        return true;
    //    }


    //    private IDeviceNotifier USBDeviceNotifier = DeviceNotifier.OpenDeviceNotifier();
    //    private void EnableUSBDevicdMonitor()
    //    {
    //        USBDeviceNotifier.OnDeviceNotify += new EventHandler<DeviceNotifyEventArgs>(OnDeviceNotifyEvent);
    //        return;
    //    }


    //    private string _DeviceVIDStr = "1904";
    //    private int _DeviceVID = 0x1904;
    //    private string _DevicePIDStr = "5741";
    //    private int _DevicePID = 0x5741;
    //    private HashSet<string> GetComNames(string vid, string pid)
    //    {
    //        string pattern = string.Format("^VID_{0}.PID_{1}", vid, pid);
    //        Regex _rx = new Regex(pattern, RegexOptions.IgnoreCase);
    //        HashSet<string> comports = new HashSet<string>();

    //        RegistryKey rk1 = Registry.LocalMachine;
    //        RegistryKey rk2 = rk1.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum");

    //        foreach (string s3 in rk2.GetSubKeyNames())
    //        {

    //            RegistryKey rk3 = rk2.OpenSubKey(s3);
    //            foreach (string s in rk3.GetSubKeyNames())
    //            {
    //                if (_rx.Match(s).Success)
    //                {
    //                    RegistryKey rk4 = rk3.OpenSubKey(s);
    //                    foreach (string s2 in rk4.GetSubKeyNames())
    //                    {
    //                        RegistryKey rk5 = rk4.OpenSubKey(s2);
    //                        RegistryKey rk6 = rk5.OpenSubKey("Device Parameters");
    //                        comports.Add((string)rk6.GetValue("PortName"));
    //                    }
    //                }
    //            }
    //        }

    //        return comports;
    //    }

    //    private void OnDeviceNotifyEvent(object sender, DeviceNotifyEventArgs e)
    //    {
    //        if (e.EventType == EventType.DeviceRemoveComplete && e.DeviceType == DeviceType.DeviceInterface)
    //        {
    //            if (e.Device.IdVendor == _DeviceVID && e.Device.IdProduct == _DevicePID)
    //            {
    //                ClearComDevices();
    //            }
    //        }
    //        else if (e.EventType == EventType.DeviceArrival && e.DeviceType == DeviceType.DeviceInterface)
    //        {
    //            if (e.Device.IdVendor == _DeviceVID && e.Device.IdProduct == _DevicePID)
    //            {
    //                FindNWDevice();
    //            }
    //        }

    //        return;
    //    }
    //}
}
