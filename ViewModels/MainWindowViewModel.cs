using InperSight.Lib.Bean;
using InperSight.Views;
using InperSight.Views.Control;
using Stylet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperSight.ViewModels
{
    public class MainWindowViewModel : Conductor<IScreen>
    {
        #region properties
        public IWindowManager windowManager;
        private MainWindowView windowView;
        private ManulControlViewModel manulControlViewModel;
        public ManulControlViewModel ManulControlViewModel { get => manulControlViewModel; set => SetAndNotify(ref manulControlViewModel, value); }
        #endregion

        #region 构造和重载
        public MainWindowViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
        }
        protected override void OnViewLoaded()
        {
            windowView = View as MainWindowView;

            ManulControlViewModel = new ManulControlViewModel(windowManager);
            ActiveItem = new DataShowControlViewModel(windowManager);

            windowView.NonClientAreaContent = new MainTitleContentArea();
            base.OnViewLoaded();
        }
        protected override void OnClose()
        {
            InperGlobalFunc.DeleteEmptyDirectory(InperGlobalClass.DataPath);
            RequestClose();
            string exeName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string[] exeArray = exeName.Split('\\');
            KillProcess(exeArray.Last().Split('.').First());
            System.Environment.Exit(0);
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
        #endregion
    }
}
