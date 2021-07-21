using InperStudio.Lib.Bean;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using InperStudioControlLib.Lib.DeviceAgency;
using InperStudioControlLib.Lib.DeviceAgency.ControlDept;
using InperStudioControlLib.Lib.Helper;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

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
            InperDeviceHelper.Instance.DeviceInit();
        }
        public MainWindowViewModel()
        {
            _ = Task.Factory.StartNew(() => { InperDeviceHelper.Instance.DisplayProc(); });
        }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            windowView = View as MainWindowView;

            LeftToolsControlViewModel = new LeftToolsControlViewModel(windowManager);
            ManulControlViewModel = new ManulControlViewModel(windowManager);
            ActiveItem = new DataShowControlViewModel(windowManager);
        }
        protected override void OnClose()
        {
            try
            {
                SystemSleepHelper.ResotreSleep();
                InperGlobalClass.CameraSignalSettings.LightMode.ForEach(x =>
                {
                    if (x.IsChecked)
                    {
                        DevPhotometry.Instance.SwitchLight(x.GroupId, false);
                    }
                });
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
            finally
            {
                //RequestClose();
                Environment.Exit(0);
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

        public void InperFile()
        {

        }
        public int Test(int a, int b)
        {
            return a + b;
        }
    }
}
