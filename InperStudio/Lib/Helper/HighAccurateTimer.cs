using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace InperStudio.Lib.Helper
{
    public class TimerEventArgs : EventArgs
    {
        private long clockFrequency;
        public long ClockFrequency
        {
            get { return clockFrequency; }
        }
        private long previousTickCount;
        public long PreviousTickOunt
        {
            get { return previousTickCount; }
        }

        private long currentTickCount;
        public long CurrentTickCount
        {
            get { return currentTickCount; }
        }

        public TimerEventArgs(long clockFreq, long prevTick, long currTick)
        {
            this.clockFrequency = clockFreq;
            this.previousTickCount = prevTick;
            this.currentTickCount = currTick;
        }
    }
    /// <summary>
    /// 高精度定时器事件委托
    /// </summary>
    public delegate void HighTimerEventHandler(object sender, TimerEventArgs e);

    public class HighAccurateTimer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);


        public event HighTimerEventHandler Elapsed;

        Thread thread;
        private object threadLock = new object();

        private long clockFrequency = 0;
        private long intevalTicks = 0;
        private long nextTriggerTime = 0;

        private int intervalMs;
        /// <summary>
        /// 定时器间隔
        /// </summary>
        public int Interval
        {
            get
            {
                return intervalMs;
            }
            set
            {
                intervalMs = value;

            }
        }

        private bool enable;
        /// <summary>
        /// 启动定时器标志
        /// </summary>
        public bool Enabled
        {
            get
            {
                return enable;
            }
            set
            {
                enable = value;
                if (value == true)
                {
                    intevalTicks = (long)(((double)intervalMs / (double)1000) * (double)clockFrequency);
                    long currTick = 0;
                    GetTick(out currTick);
                    nextTriggerTime = currTick + intevalTicks;
                }
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public HighAccurateTimer()
        {
            if (QueryPerformanceFrequency(out clockFrequency) == false)
            {
                return;
            }
            this.intervalMs = 1000;
            this.enable = false;

            thread = new Thread(new ThreadStart(ThreadProc));
            thread.Name = "HighAccuracyTimer";
            thread.Priority = ThreadPriority.Highest;
            thread.Start();

        }

        /// <summary>
        /// 进程主程序
        /// </summary>
        private void ThreadProc()
        {
            long currTime;
            GetTick(out currTime);
            nextTriggerTime = currTime + intevalTicks;
            while (true)
            {
                while (currTime < nextTriggerTime)
                {
                    GetTick(out currTime); //决定时钟的精度
                }
                nextTriggerTime = currTime + intevalTicks;

                if (Elapsed != null && enable == true)
                {
                    Elapsed(this, new TimerEventArgs(clockFrequency, currTime - intevalTicks, currTime));
                }
            }
        }
        /// <summary>
        /// 获得当前时钟计数
        /// </summary>
        /// <param name="currentTickCount">时钟计数</param>
        /// <returns>获得是否成功</returns>
        public bool GetTick(out long currentTickCount)
        {
            if (QueryPerformanceCounter(out currentTickCount) == false)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 注销定时器
        /// </summary>
        public void Destroy()
        {
            enable = false;
            thread.Abort();
        }
    }
}
