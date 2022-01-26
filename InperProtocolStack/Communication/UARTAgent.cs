using LibUsbDotNet;
using LibUsbDotNet.Main;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InperProtocolStack.Communication
{
    public class OnDataReceivedEventArgs : EventArgs
    {
        private byte[] _Data;
        public OnDataReceivedEventArgs(byte[] data)
        {
            _Data = data;
        }
        public byte[] Data => _Data;
    }
    public class UsbAdData
    {
        public UsbAdData(uint channelId, long time, List<int> values)
        {
            ChannelId = channelId;
            Time = time;
            Values = values;
        }

        public uint ChannelId { get; private set; }
        public long Time { get; private set; }
        public List<int> Values { get; private set; }
    }

    public class UARTAgent
    {

        public event EventHandler<OnDataReceivedEventArgs> OnDataReceived;
        public event EventHandler<OnDataReceivedEventArgs> OnDataReceivedUsb;
        public void RaiseDataReceivedEvent(byte[] data)
        {
            OnDataReceived?.Invoke(this, new OnDataReceivedEventArgs(data));
        }


        private SerialPort _ComPort;
        private readonly UsbDevice MyUsbDevice;
        public UARTAgent(int vid, int pid)
        {
            MyUsbDevice = UsbDevice.OpenUsbDevice(new UsbDeviceFinder(vid, pid));
            if (MyUsbDevice == null)
            {
                System.Diagnostics.Debug.WriteLine("未找到usb设备");
                return;
            }
            IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
            if (!ReferenceEquals(wholeUsbDevice, null))
            {
                // This is a "whole" USB device. Before it can be used, 
                // the desired configuration and interface must be selected.
                // Select config #1
                _ = wholeUsbDevice.SetConfiguration(1);
                // Claim interface #0.
                _ = wholeUsbDevice.ClaimInterface(0);
            }
            UsbEndpointReader reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01, 128);
            UsbEndpointReader readerAD = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep02, 1088);

            // open write endpoint 1.
            //UsbEndpointWriter writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);

            reader.DataReceived += Reader_DataReceived;
            reader.DataReceivedEnabled = true;
            readerAD.DataReceived += ReaderAD_DataReceived;
            readerAD.DataReceivedEnabled = true;
        }
        public Queue<byte[]> DataCache = new Queue<byte[]>();
        public readonly object _DataCacheObj = new object();
        public readonly AutoResetEvent _DataAutoResetEvent = new AutoResetEvent(false);
        private void ReaderAD_DataReceived(object sender, EndpointDataEventArgs e)
        {
            Monitor.Enter(_DataCacheObj);
            //byte[] vs = new byte[e.Buffer.Count()];
            if (e.Count > 0)
            {
                //for (int i = 0; i < e.Buffer.Count(); i++)
                //{
                //    vs[i] = e.Buffer[i];
                //}
                //Console.WriteLine(e.Buffer[0]);
                DataCache.Enqueue(e.Buffer);
                Monitor.Exit(_DataCacheObj);
                _ = _DataAutoResetEvent.Set();
                return;
            }
            Monitor.Exit(_DataCacheObj);
        }
        private void Reader_DataReceived(object sender, EndpointDataEventArgs e)
        {
            byte[] data = e.Buffer;
            RaiseDataReceivedEvent(data);
        }
        public UARTAgent(string p_name)
        {
            _ComPort = new SerialPort();
            _ComPort.BaudRate = 115200; //9216000;
            _ComPort.Parity = Parity.None;
            _ComPort.DataBits = 8;
            _ComPort.StopBits = StopBits.One;
            _ComPort.Encoding = Encoding.ASCII;
            _ComPort.PortName = p_name;

            if (_ComPort.IsOpen == false)
            {
                try
                {
                    _ComPort.Open();
                    _ComPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
                    System.Diagnostics.Debug.WriteLine("Optogenetics found at " + p_name);
                }
                catch (Exception e)
                {
                    _ComPort = null;
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
        }


        private byte[] ReadData(object sender)
        {
            byte[] ReData = new byte[((SerialPort)sender).BytesToRead];
            ((SerialPort)sender).Read(ReData, 0, ReData.Length);
            return ReData;
        }


        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] data = ReadData(sender);
            RaiseDataReceivedEvent(data);
        }

        private bool SendDataUsb(UsbDevice myUsbDevice, List<byte> data)
        {
            if (myUsbDevice == null)
            {
                return false;
            }
            if (myUsbDevice.IsOpen)
            {
                byte[] data_barray = data.ToArray();
                try
                {
                    string cmd = "";
                    data_barray.ToList().ForEach(x =>
                    {
                        cmd += x.ToString("X2") + " ";
                    });
                    Console.WriteLine(cmd);
                    _ = myUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01).Write(data_barray, 10, out int bytesWritten);
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("On Sending Data to Serial Port: " + ex.Message);
                }
            }
            return false;
        }
        public bool SendDataUsb(List<byte> data)
        {
            return SendDataUsb(MyUsbDevice, data);
        }
        private bool SendData(SerialPort com, List<byte> data)
        {
            if (com == null)
            {
                return false;
            }
            if (com.IsOpen)
            {
                byte[] data_barray = data.ToArray();
                try
                {
                    com.Write(data_barray, 0, data_barray.Length);
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("On Sending Data to Serial Port: " + ex.Message);
                }
            }
            return false;
        }
        public bool SendData(List<byte> data)
        {
            SendData(_ComPort, data);
            return true;
        }
    }
}
