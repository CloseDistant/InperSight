using InperStudio.Lib.Bean;
using InperStudio.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace InperStudio.Views.Control
{
    /// <summary>
    /// HeartbeatContrrol.xaml 的交互逻辑
    /// </summary>
    public partial class HeartbeatContrrol : UserControl
    {
        DispatcherTimer timer = new DispatcherTimer();
        public HeartbeatContrrol()
        {
            InitializeComponent();

            int heartbeatCountCheck = 0, showCount = 0;
            long heartSize = 0;
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += (s, e) =>
            {
                long size = 0;
                try
                {
                    size = new FileInfo(System.IO.Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName, "data.db")).Length;
                }
                catch (Exception ex)
                {
                    this.wavePB.WaveFill = Brushes.OrangeRed;
                    App.Log.Error(ex.ToString());
                    return;
                }

                if (heartSize != size)
                {
                    heartSize = size;
                    heartbeatCountCheck = 0;
                    showCount++;
                    this.wavePB.WaveFill = Brushes.LightSeaGreen;
                }
                else
                {
                    heartbeatCountCheck++;
                    if (heartbeatCountCheck > 3)
                    {
                        this.wavePB.WaveFill = Brushes.OrangeRed;
                    }
                }
                this.fileSize.Text = showCount.ToString();
            };
            timer.Start();
            this.Unloaded += (s, e) => timer.Stop();
        }

        private void TransitioningContentControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ManulControlViewModel.sprite.Close();
        }
    }
}
