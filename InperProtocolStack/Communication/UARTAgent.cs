using InperProtocolStack.TransmissionCtrl;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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

    [Serializable]
    public class UsbAdData
    {
        public UsbAdData(uint channelId, long time, List<double> values)
        {
            ChannelId = channelId;
            Time = time;
            Values = values;
        }

        public uint ChannelId { get; private set; }
        public long Time { get; private set; }
        public List<double> Values { get; private set; }
    }

    public class UARTAgent
    {

        public event EventHandler<OnDataReceivedEventArgs> OnDataReceived;
        //public event EventHandler<OnDataReceivedEventArgs> OnDataReceivedUsb;
        public bool IsStart = false;
        public void RaiseDataReceivedEvent(byte[] data)
        {
            OnDataReceived?.Invoke(this, new OnDataReceivedEventArgs(data));
        }


        private SerialPort _ComPort;
        private readonly UsbDevice MyUsbDevice;
        private UsbEndpointReader readerAD;
        private int adLength = 0;
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

            // open write endpoint 1.
            //UsbEndpointWriter writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);

            reader.DataReceived += Reader_DataReceived;
            reader.DataReceivedEnabled = true;
        }
        public void SetSampling(int sampling)
        {

            if (sampling >= 10000)
            {
                adLength = 1024 + 64;
            }
            else if (sampling >= 5000)
            {
                adLength = 512 + 64;
            }
            else if (sampling >= 1000)
            {
                adLength = 256 + 64;
            }
            else if (sampling >= 500)
            {
                adLength = 128 + 64;
            }
            else if (sampling >= 100)
            {
                adLength = 64 + 64;
            }
            else if (sampling >= 50)
            {
                adLength = 32 + 64;
            }
            else if (sampling >= 30)
            {
                adLength = 16 + 64;
            }
            else if (sampling >= 16)
            {
                adLength = 8 + 64;
            }
            //else if (sampling >= 8)
            else
            {
                adLength = 4 + 64;
            }
            //else
            //{
            //    adLength = 2 + 64;
            //}

            readerAD = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep02, adLength);
            readerAD.DataReceived += ReaderAD_DataReceived;
            readerAD.DataReceivedEnabled = true;

        }
        public void RemoveSampling()
        {
            readerAD.DataReceived -= ReaderAD_DataReceived;
            readerAD.DataReceivedEnabled = false;
        }

        public ConcurrentQueue<IntPtr> ADPtrQueues = new ConcurrentQueue<IntPtr>();
        private unsafe void ReaderAD_DataReceived(object sender, EndpointDataEventArgs e)
        {
            if (IsStart)
            {
                //lock (_DataCacheObj)
                //{
                if (e.Count != 0)
                {
                    fixed (byte* pb = &e.Buffer[0])
                    {
                        ADPtrQueues.Enqueue((IntPtr)pb);
                        //UsbAdDataStru res = Marshal.PtrToStructure<UsbAdDataStru>((IntPtr)pb);
                        //_ADDataCache.Enqueue(res);
                        //UsbStrSet((IntPtr)pb);
                    }
                }
                //}
                //_ = _DataAutoResetEvent.Set();
            }
        }
        private void Reader_DataReceived(object sender, EndpointDataEventArgs e)
        {

            byte[] data = null;
            if (e.Buffer[0] == 0x48 && e.Buffer[1] == 0x57)
            {
                int length = BitConverter.ToUInt16(e.Buffer.Skip(12).Take(2).ToArray(), 0);

                data = e.Buffer.Take(20 + length).ToArray();
                RaiseDataReceivedEvent(data);
                //Console.WriteLine("---" + BitConverter.ToString(data));
            }

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
