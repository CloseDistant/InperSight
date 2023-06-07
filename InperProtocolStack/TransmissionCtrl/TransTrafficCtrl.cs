using InperProtocolStack.Basis;
using InperProtocolStack.CmdPhotometry;
using InperProtocolStack.Communication;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace InperProtocolStack.TransmissionCtrl
{
    public struct UsbAdDataStru512
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Channel;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Time;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public byte[] Fill;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public short[] Values;
    }
    public struct UsbAdDataStru256
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Channel;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Time;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public byte[] Fill;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public short[] Values;
    }
    public struct UsbAdDataStru128
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Channel;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Time;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public byte[] Fill;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public short[] Values;
    }
    public struct UsbAdDataStru64
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Channel;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Time;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public byte[] Fill;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public short[] Values;
    }
    public struct UsbAdDataStru32
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Channel;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Time;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public byte[] Fill;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public short[] Values;
    }
    public struct UsbAdDataStru16
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Channel;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Time;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public byte[] Fill;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public short[] Values;
    }
    public struct UsbAdDataStru8
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Channel;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Time;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public byte[] Fill;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public short[] Values;
    }
    public struct UsbAdDataStru4
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Channel;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Time;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public byte[] Fill;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public short[] Values;
    }
    public struct UsbAdDataStru2
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Channel;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Time;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public byte[] Fill;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public short[] Values;
    }
    public struct UsbAdDataStru1
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Channel;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Time;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public byte[] Fill;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public short[] Values;
    }
    public class TransTrafficCtrl
    {
        private UARTAgent _UARTA;
        private ReceivingProcessor _RProcessor = new ReceivingProcessor();
        public TransTrafficCtrl(UARTAgent uart_a)
        {
            _UARTA = uart_a;
            _RProcessor.OnMsgReceived += DevMsgReceived;
            _UARTA.OnDataReceived += new EventHandler<OnDataReceivedEventArgs>(OnDataReceived);
            //_UARTA.OnDataReceivedUsb += UARTA_OnDataReceivedUsb;
            _UARTA.OnInputReceived += _UARTA_OnInputReceived;
            _ = Task.Run(() => { ReceivedDataMonitor(); });

            //SetupMetronome();
            //_ = Task.Run(() => { TransmittingProcess(); });


            return;
        }
        public event EventHandler<byte[]> OnInputUpdated;
        private void _UARTA_OnInputReceived(object sender, byte[] e)
        {
            OnInputUpdated?.Invoke(sender, e);
        }

        public event EventHandler<MessageReceivedEventArgs> OnInfoUpdated;

        private void DevMsgReceived(object sender, MessageReceivedEventArgs e)
        {
            Confirm(e.Header.Sequence, e.Header.Intent);
            OnInfoUpdated?.Invoke(this, e);

            return;
        }


        private object _RCacheLock = new object();
        private Queue<byte> _ReceivingCache = new Queue<byte>();
        private AutoResetEvent _RecvAREvent = new AutoResetEvent(false);
        private void OnDataReceived(object sender, OnDataReceivedEventArgs e)
        {
            lock (_RCacheLock)
            {
                foreach (var d in e.Data)
                {
                    _ReceivingCache.Enqueue(d);
                }
            }
            _RecvAREvent.Set();

            return;
        }


        private void ReceivedDataMonitor()
        {
            while (true)
            {
                _RecvAREvent.WaitOne();
                if (Monitor.TryEnter(_RCacheLock))
                {
                    byte[] rd = _ReceivingCache.ToArray();
                    _ReceivingCache.Clear();
                    Monitor.Exit(_RCacheLock);

                    _RProcessor.AppendData(rd);
                }
            }
        }


        private uint _SendingSequence = 0;
        public ConcurrentQueue<Command> _CommandCache = new ConcurrentQueue<Command>();
        private AutoResetEvent _AREvent = new AutoResetEvent(false);
        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        public void Transmit(Command cmd)
        {
            cmd.Sequence = _SendingSequence;
            _SendingSequence++;
            _CommandCache.Enqueue(cmd);

            Task.Run(() => { TransmittingProcess(); });
            return;
        }
        public void OutputSend(List<byte> b)
        {
            _ = _UARTA.SendDataUsb(b, 2);
        }
        async Task TaskExecute(CancellationToken token, List<byte> vs)
        {
            int count = 0;
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(100);
                count++;
                if (count >= 10)
                {
                    _ = _UARTA.SendDataUsb(vs, 1);
                    count = 0;
                }
            }
        }
        int _pLock = 0;
        private async void TransmittingProcess()
        {
            if (Interlocked.Exchange(ref _pLock, 1) == 0)
            {
                if (_CommandCache.TryDequeue(out Command cmd))
                {
                    tokenSource = new CancellationTokenSource();
                    List<byte> vs = cmd.GetBytes();
                    if (vs.Count == 64)
                    {
                        vs = cmd.GetBytes();
                        vs.Add(0);
                    }
                    _ = _UARTA.SendDataUsb(vs, 1);
                    await TaskExecute(tokenSource.Token, vs);
                }
                Interlocked.Exchange(ref _pLock, 0);
                if (_CommandCache.Count > 0)
                {
                    _ = Task.Run(() => { TransmittingProcess(); });
                }
            }
        }


        private void Confirm(uint sequence, uint intent)
        {
            if (intent == 255)
            {
                tokenSource.Cancel();
            }
            return;
        }


        //private System.Timers.Timer _WaitTimeoutTimer = new System.Timers.Timer();
        private void SetupMetronome()
        {
            _AREvent.Set();
            //_WaitTimeoutTimer.Interval = 1000;
            //_WaitTimeoutTimer.AutoReset = false;
            //_WaitTimeoutTimer.Enabled = false;
            //_WaitTimeoutTimer.Elapsed += OnTimeout;
        }


        //private void OnTimeout(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    return;
        //}
    }
}
