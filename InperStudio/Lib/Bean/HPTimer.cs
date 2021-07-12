using InperStudio.Lib.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Bean
{
    public class HPTimer
    {
        static HPTimer()
        {
            TimeGetDevCaps(ref _caps, Marshal.SizeOf(_caps));
        }

        public HPTimer()
        {
            Running = false;
            _interval = _caps.periodMin;
            _resolution = _caps.periodMin;
            _callback = new TimerCallback(TimerEventCallback);
        }

        ~HPTimer()
        {
            TimeKillEvent(_id);
        }

        /// <summary>
        /// 系统定时器回调
        /// </summary>
        /// <param name="id">定时器编号</param>
        /// <param name="msg">预留，不使用</param>
        /// <param name="user">用户实例数据</param>
        /// <param name="param1">预留，不使用</param>
        /// <param name="param2">预留，不使用</param>
        private delegate void TimerCallback(int id, int msg, int user, int param1, int param2);

        #region 动态库接口

        /// <summary>
        /// 查询设备支持的定时器分辨率
        /// </summary>
        /// <param name="ptc">定时器分辨率结构体指针</param>
        /// <param name="cbtc">定时器分辨率结构体大小</param>
        /// <returns></returns>
        [DllImport("winmm.dll", EntryPoint = "timeGetDevCaps")]
        private static extern TimerError TimeGetDevCaps(ref TimerCaps ptc, int cbtc);

        /// <summary>
        /// 绑定定时器事件
        /// </summary>
        /// <param name="delay">延时：毫秒</param>
        /// <param name="resolution">分辨率</param>
        /// <param name="callback">回调接口</param>
        /// <param name="user">用户提供的回调数据</param>
        /// <param name="eventType"></param>
        [DllImport("winmm.dll", EntryPoint = "timeSetEvent")]
        private static extern int TimeSetEvent(int delay, int resolution, TimerCallback callback, int user, int eventType);

        /// <summary>
        /// 终止定时器
        /// </summary>
        /// <param name="id">定时器编号</param>
        [DllImport("winmm.dll", EntryPoint = "timeKillEvent")]
        private static extern TimerError TimeKillEvent(int id);

        #endregion


        /// <summary>时间间隔：毫秒</summary>
        public int Interval
        {
            get { return _interval; }
            set
            {
                if (value < _caps.periodMin || value > _caps.periodMax)
                    throw new Exception("Invalid Interval.");
                _interval = value;
            }
        }

        public bool Running { get; private set; }


        public event Action Tick;


        public void Start()
        {
            if (!Running)
            {
                _id = TimeSetEvent(_interval, _resolution, _callback, 0,
                    (int)EventType01.TIME_PERIODIC | (int)EventType02.TIME_KILL_SYNCHRONOUS);
                if (_id == 0) throw new Exception("Start Timer Failed.");
                Running = true;
            }
        }

        public void Stop()
        {
            if (Running)
            {
                TimeKillEvent(_id);
                Running = false;
            }
        }


        private void TimerEventCallback(int id, int msg, int user, int param1, int param2)
        {
            Tick?.Invoke();
        }


        private static TimerCaps _caps;
        private int _interval;
        private int _resolution;
        private TimerCallback _callback;
        private int _id;
    }
}
