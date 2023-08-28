using InperStudio.Lib.Bean;
using InperStudio.Lib.Data;
using InperStudio.Lib.Helper;
using InperStudioControlLib.Lib.Config;
using InperStudioControlLib.Lib.Helper;
using log4net;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using SciChart.Charting.Visuals;
using System;
using System.Device.Location;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace InperStudio
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        public static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static SqlDataInit SqlDataInit;
        private static System.Threading.Mutex mutex;

        //string apiKey = "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";
        protected override void OnStartup(StartupEventArgs e)
        {
            if (Process.GetProcessesByName("UpgradeClient").ToList().Count > 0)
            {
                //MessageBox.Show("正在运行中");
                Environment.Exit(0);
                return;
            }
            mutex = new System.Threading.Mutex(true, "OnlyRun_CRNS");
            if (!mutex.WaitOne(0, false))
            {
                //if (MessageBoxResult.OK == MessageBox.Show("The application is already running！"))
                {
                    RaiseOtherProcess();
                    this.Shutdown();
                }
            }
            // Set this code once in App.xaml.cs or application startup
            SciChartSurface.SetRuntimeLicenseKey("RYLnOXgjEruY2Nh2EmZx86QLNsGKveD+J1b1iVWLO/mjGdIVZKUyaBJdgcEa9nqdJwNoEMA6Y9b3ltO3v3TqSVlvFRdm8W1FPibVBc7QmCjDO6jljcyZUV8STM8SFkdVuNKQNwKFlhDba8OY9fuPQBSMY/V0atNGBTc4cscDyOyofJxxPjZbUenA+PkmFAE+tHD4m0VU2dkUqFKYGkf6czcfLOCrIwcnxFJKjHr/+qLSTgDwF3Mh4k34YP7g0MxGBdlllTJTDHKBruQ1ZiBYPOZptgAnNM9VjLi1DWoOrjIzNaHfA+16tcLmdRWunHdPPQU/f+uCOaNjfcHPASoAuNWRoDd1f9xZw8ZdGWJXJ9XtSLjCzod+cTQP02fxJABATC/V9y0RqudDwdUCyGduKX0ESOz6JqHTCR7qysmdy+xWgg27t31PUeXT0WuGhwcEARl5o2UM4eVemwnCmErwX6mS6ac9Pv2gWKRxuiUkgE0UxOf9O/cVxfJ+IcwlQP+BxlPF6uMnUSfjSTu5GCohkSHANUzWcVJSPjNvwjDi2xhr/mDfSIV4yn33wjG7/aJBf2cXuKgIhyA6RzId2FNB2ZstwGD1zPqVT3A=");

            #region 配置文件初始化

            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            string appConfigDir = Path.Combine(Environment.CurrentDirectory, "Config");
            if (!Directory.Exists(appConfigDir))
            {
                _ = Directory.CreateDirectory(appConfigDir);
            }
            string configPath = Path.Combine(appConfigDir, "ProductConfig.config");
            string configPathexe = Path.Combine(appConfigDir, assemblyName + ".exe.config");
            string jsonPath = Path.Combine(appConfigDir, "UserConfig.json");
            if (!File.Exists(configPathexe))
            {
                File.Copy(Environment.CurrentDirectory + @"\" + assemblyName + ".exe.config", configPathexe);
            }
            if (!File.Exists(configPath))
            {
                File.Copy(Environment.CurrentDirectory + @"\ProductConfig.config", configPath);
            }
            //if (!File.Exists(jsonPath))
            {
                File.Copy(Environment.CurrentDirectory + @"\UserConfig.json", jsonPath, true);
            }
            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "UnderBin")))
            {
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "UnderBin"));
            }
            RegisterEvents();
            #endregion

            InperGlobalClass.DataPath = InperGlobalClass.DataPath == string.Empty ? Environment.CurrentDirectory + @"\Data\" : InperGlobalClass.DataPath;
            InperGlobalClass.DataFolderName = InperGlobalClass.DataFolderName == string.Empty ? DateTime.Now.ToString("yyyyMMddHHmmss") : InperGlobalClass.DataFolderName;
            if (!Directory.Exists(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName)))
            {
                _ = Directory.CreateDirectory(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName));
            }
            SystemSleepHelper.PreventSleep(true);

            base.OnStartup(e);
        }
        public static void RaiseOtherProcess()
        {
            try
            {
                Process proc = Process.GetCurrentProcess();
                foreach (Process otherProc in Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName))
                {
                    if (proc.Id != otherProc.Id)
                    {
                        IntPtr hWnd = otherProc.MainWindowHandle;
                        if (IsIconic(hWnd))
                        {
                            ShowWindowAsync(hWnd, 9);
                        }
                        SetForegroundWindow(hWnd);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Info(ex.ToString());
            }
        }

        private void RegisterEvents()
        {
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                InperLogExtentHelper.LogExtent(args.Exception, "app");
                args.SetObserved();
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) => Log.Info(args.ExceptionObject.ToString());
        }
    }
}
