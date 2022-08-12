using InperProtocolStack.CmdPhotometry;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InperProtocolStack.UnderUpgrade
{
    public class UnderUpgradeFunc
    {
        private AutoResetEvent _RecvAREvent = new AutoResetEvent(false);
        UsbDevice MyUsbDevice;
        public bool DeviceIsRestart = false;
        public bool DeviceIsExist = false;
        CancellationTokenSource tokenSource = new CancellationTokenSource(new TimeSpan(0, 0, 3));
        CancellationTokenSource tokenSourceHash = new CancellationTokenSource(new TimeSpan(0, 0, 3));
        CancellationTokenSource tokenSourceRun = new CancellationTokenSource(new TimeSpan(0, 0, 3));
        CancellationTokenSource tokenSourceSend = new CancellationTokenSource(new TimeSpan(0, 0, 3));
        public UnderUpgradeFunc()
        {
            UsbRegistry usb = null;
            UsbRegDeviceList allDevices = UsbDevice.AllDevices;
            foreach (UsbRegistry usbRegistry in allDevices)
            {
                if (usbRegistry.Name.Contains("Inper USB Port"))
                {
                    usb = usbRegistry;
                }
            }
            if (usb != null)
            {
                MyUsbDevice = UsbDevice.OpenUsbDevice(new UsbDeviceFinder(usb.Vid, usb.Pid));
                if (MyUsbDevice == null)
                {
                    return;
                }
                if (MyUsbDevice is IUsbDevice wholeUsbDevice)
                {
                    _ = wholeUsbDevice.SetConfiguration(1);
                    _ = wholeUsbDevice.ClaimInterface(0);
                }
                DeviceIsExist = true;
                UsbEndpointReader reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01, 128);
                reader.DataReceived += Reader_DataReceived;
                reader.DataReceivedEnabled = true;
            }
        }
        public async void UnderInitJumpUpgrade(CancellationTokenSource cancellation)
        {
            if (MyUsbDevice == null)
            {
                cancellation.Cancel();
                return;
            }
            MyUsbDevice.Open();
            Write(new CmdInitMcu().GetBytes().ToArray());
            await TaskExecute(tokenSource.Token);
            CmdAppRun cmdAppRun = new CmdAppRun();
            Write(cmdAppRun.GetBytes().ToArray());
            await TaskExecute(tokenSourceRun.Token);
            MyUsbDevice.Close();
            cancellation.Cancel();
        }
        async Task TaskExecute(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(100);
            }
        }

        public async void SendFile(FileInfo[] files, CancellationTokenSource cancellation)
        {
            if (MyUsbDevice == null)
            {
                cancellation.Cancel();
                return;
            }

            _ = MyUsbDevice.Open();

            Write(new CmdInitMcu().GetBytes().ToArray());
            await TaskExecute(tokenSource.Token);
            foreach (FileInfo x in files.OrderBy(x => x.LastWriteTime).ToList())
            {
                Console.WriteLine("进入到文件");
                Write(new CmdGetHashVal().GetBytes().ToArray()); //获取下位机hsah值
                await TaskExecute(tokenSourceHash.Token);
                if (!DeviceIsRestart)
                {
                    cancellation.Cancel();
                    if (MyUsbDevice.IsOpen)
                    {
                        MyUsbDevice.Close();
                    }
                    return;
                }

                if (x.Extension.Contains("bin"))
                {
                    string _hashval = string.Empty;
                    using (FileStream fs = new FileStream(x.FullName, FileMode.Open, FileAccess.ReadWrite))
                    {
                        System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                        byte[] retVal = md5.ComputeHash(fs);
                        _hashval = BitConverter.ToString(retVal).Replace("-", ""); //获取当前文件hash值

                    }
                    if (hashval == _hashval)
                    {
                        File.Delete(x.FullName);
                        continue;
                    }

                    CmdSetFileLength length = new CmdSetFileLength();
                    length.SetCmdParam((uint)x.Length);
                    Write(length.GetBytes().ToArray());

                    tokenSource = new CancellationTokenSource(new TimeSpan(0, 0, 3));
                    await TaskExecute(tokenSource.Token);

                    byte[] bytes = new byte[] { };
                    using (FileStream fs = new FileStream(x.FullName, FileMode.Open, FileAccess.Read))
                    {
                        BinaryReader binreader = new BinaryReader(fs);
                        bytes = binreader.ReadBytes((int)fs.Length);
                    }
                    count = 0; isstart = true;
                    CmdSendFile cmd1 = new CmdSendFile();

                    while (isstart)
                    {
                        tokenSourceSend = new CancellationTokenSource(new TimeSpan(0, 0, 3));
                        List<byte> re = bytes.Skip(send_length * count).Take(send_length).ToList();
                        if (re.Count > 0)
                        {
                            cmd1.SetCmdParam(re);
                            Write(cmd1.GetBytes().ToArray());
                            await TaskExecute(tokenSourceSend.Token);
                        }
                        if (re.Count < send_length)
                        {
                            isstart = false;

                            CmdGetHashVal cmdHash = new CmdGetHashVal();
                            Write(cmdHash.GetBytes().ToArray());
                            tokenSourceHash = new CancellationTokenSource(new TimeSpan(0, 0, 3));
                            await TaskExecute(tokenSourceHash.Token);

                            if (hashval == _hashval)
                            {
                                File.Delete(x.FullName);
                                continue;
                            }
                        }
                    }
                }
            }
            CmdAppRun cmdAppRun = new CmdAppRun();
            Write(cmdAppRun.GetBytes().ToArray());
            await TaskExecute(tokenSourceRun.Token);

            MyUsbDevice.Close();
            cancellation.Cancel();
        }
        private void Write(byte[] data)
        {
            MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01).Write(data, 10, out int mcuLength);
        }

        private bool isstart = false;
        private string hashval = string.Empty;
        private int count = 0;
        private readonly int send_length = 2048;
        //private event Action<string, bool> GetHashVal;
        private unsafe void Reader_DataReceived(object sender, EndpointDataEventArgs e)
        {
            if (e.Buffer[3] == 0xFF)
            {
                Console.WriteLine(BitConverter.ToString(e.Buffer));
                int res = BitConverter.ToInt32(e.Buffer.Skip(16).Take(4).ToArray(), 0);
                if (res == 0)
                {
                    tokenSource.Cancel();
                }
            }
            if (e.Buffer[3] == 0xEF)
            {
                var _hashVal = e.Buffer.Skip(20).Take(16).ToArray();
                hashval = BitConverter.ToString(_hashVal).Replace("-", "");
                Console.WriteLine(hashval);
                DeviceIsRestart = true;
                tokenSourceHash.Cancel();
            }
            if (isstart)
            {
                if (e.Buffer[3] == 0xEB)
                {
                    count++;
                    tokenSourceSend.Cancel();
                    byte[] rec_length = e.Buffer.Skip(20).Take(4).ToArray();
                    int length = BitConverter.ToInt32(rec_length, 0);
                    Console.WriteLine(length + "---" + count * send_length);

                }
            }
        }
    }
}
