using InperStudio.Lib.Bean;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using InperStudio.Views.Control;
using InperStudioControlLib.Lib.Config;
using InperStudioControlLib.Lib.Helper;
using Stylet;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace InperStudio.ViewModels
{
    public class MainWindowViewModel : Conductor<IScreen>
    {
        #region properties
        public IWindowManager windowManager;
        private MainWindowView windowView;
        private LeftToolsControlViewModel leftToolsControlViewModel;
        private ManulControlViewModel manulControlViewModel;
        public LeftToolsControlViewModel LeftToolsControlViewModel { get => leftToolsControlViewModel; set => SetAndNotify(ref leftToolsControlViewModel, value); }
        public ManulControlViewModel ManulControlViewModel { get => manulControlViewModel; set => SetAndNotify(ref manulControlViewModel, value); }
        public static BindableCollection<ListChartValues<TimeSpan, double>> ChartDatas { get; set; } = new BindableCollection<ListChartValues<TimeSpan, double>>();
        #endregion

        #region 构造和重载
        public MainWindowViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
        }
        public MainWindowViewModel()
        {
            //InperDeviceHelper.Instance.DeviceInit();
            _ = Task.Factory.StartNew(() => { InperDeviceHelper.Instance.DisplayProc(); });
        }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            windowView = View as MainWindowView;

            LeftToolsControlViewModel = new LeftToolsControlViewModel(windowManager);
            ManulControlViewModel = new ManulControlViewModel(windowManager);
            ActiveItem = new DataShowControlViewModel(windowManager);

            windowView.NonClientAreaContent = new MainTitleContentArea();
        }
        protected override void OnClose()
        {
            RequestClose();
            string exeName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string[] exeArray = exeName.Split('\\');
            KillProcess(exeArray.Last().Split('.').First());
            Environment.Exit(0);
        }
        private void KillProcess(string processName)
        {
            Process[] myproc = Process.GetProcesses();
            foreach (Process item in myproc)
            {
                if (item.ProcessName == processName)
                {
                    item.Kill();
                }
            }

        }
        public void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (!InperGlobalClass.IsImportConfig)
                {
                    if (MessageBox.Show("Config files unsaved,quit anyway?", "Config", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                InperConfig.Instance.IsSkip = false;
                SystemSleepHelper.ResotreSleep();

                InperDeviceHelper.Instance.LightWaveLength.ToList().ForEach(x =>
                {
                    InperDeviceHelper.Instance.device.SwitchLight((uint)x.GroupId, false);
                    InperDeviceHelper.Instance.device.SetLightPower((uint)x.GroupId, 0);
                    Thread.Sleep(50);
                });
                InperDeviceHelper.Instance.device.Stop();

                string[] files = Directory.GetDirectories(Path.Combine(InperGlobalClass.DataPath));
                if (files.Length > 0)
                {
                    foreach (string s in files)
                    {
                        if (Directory.GetFiles(s).Length == 0)
                        {
                            Directory.Delete(s);
                        }
                    }
                }
                if (System.Windows.Application.Current.Windows.OfType<Window>().Count() > 1)
                {
                    foreach (var x in System.Windows.Application.Current.Windows.OfType<Window>().ToList())
                    {
                        if (!x.Name.StartsWith("MainWindow"))
                        {
                            x.Close();
                            Thread.Sleep(3000);
                        }
                    }
                }
                base.OnClose();
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        #endregion

        #region
        public void ButtonVisibilitySwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            this.windowView.main.ColumnDefinitions[1].Width = new GridLength(0);
        }
        public void ButtonVisibilitySwitch_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as ToggleButton).IsFocused)
            {
                this.windowView.main.ColumnDefinitions[1].Width = GridLength.Auto;
            }
        }

        #endregion
    }
}
