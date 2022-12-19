using InperSight.Lib.Bean;
using InperSight.Lib.Config;
using InperSight.Lib.Config.Json;
using InperSight.Lib.Helper;
using InperSight.ViewModels;
using InperStudioControlLib.Lib.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InperSight.Views.Control
{
    /// <summary>
    /// MainTitleContentArea.xaml 的交互逻辑
    /// </summary>
    public partial class MainTitleContentArea : Border
    {
        public InperGlobalClass InperGlobalClass { get; set; } = new InperGlobalClass();
        public List<string> SkinColorList { get; set; } = InperColorHelper.ColorPresetList;
        public MainTitleContentArea()
        {
            InitializeComponent();
            DataContext = this;
            this.move1.AddHandler(Border.MouseDownEvent, new System.Windows.Input.MouseButtonEventHandler(Border_MouseDown), true);
            this.move2.AddHandler(Border.MouseDownEvent, new System.Windows.Input.MouseButtonEventHandler(Border_MouseDown), true);
        }
        private void ImportConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new()
                {
                    Filter = "Json|*.inper"
                };
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    InperGlobalClass.IsImportConfig = true;
                    if (InperGlobalFunc.GetWindowByNameChar("Insight") != null)
                    {
                        InperGlobalFunc.GetWindowByNameChar("Insight").Close();
                    }
                    InperJsonConfig.filepath = openFileDialog.FileName;
                    InperGlobalClass.CameraSettingJsonBean = JsonHelper.GetCameraSetting();

                    foreach (Window window in System.Windows.Application.Current.Windows)
                    {                        
                        if (window.Name.Contains("MainWindow"))
                        {
                            CameraSettingViewModel _window = new();
                            (window.DataContext as MainWindowViewModel).windowManager.ShowWindow(_window);
                            //_window.RequestClose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        private void SaveConfigAs_Click(object sender, RoutedEventArgs e)
        {
            JsonConfigSaveAs();
        }
        public static bool JsonConfigSaveAs()
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "Json|*.inper";
                // 设置默认的文件名。注意！文件扩展名须与Filter匹配
                dlg.FileName = "InperSightConfig";
                // 显示对话框
                DialogResult r = dlg.ShowDialog();

                if (r == DialogResult.Cancel)
                {
                    return false;
                }
                string fname = dlg.FileName;
                File.Copy(InperJsonConfig.filepath, fname, true);
                InperJsonConfig.filepath = fname;
                InperGlobalClass.IsImportConfig = true;
                InperGlobalFunc.ShowRemainder("Saved successfully");
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
            return true;
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                {
                    if (window.Name.Contains("MainWindow"))
                    {
                        window.DragMove();
                    }
                }

            }
        }
        private void DataFolderName_Click(object sender, RoutedEventArgs e)
        {
            foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
            {
                if (window.Name.Contains("MainWindow"))
                {
                    (window.DataContext as MainWindowViewModel).windowManager.ShowDialog(new DataPathConfigViewModel(DataConfigPathTypeEnum.Path));
                }
            }
        }
        private void SkinList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (sender as System.Windows.Controls.ListBox).SelectedItem;

            System.Windows.Application.Current.Resources["InperTheme"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(item.ToString()));
        }
        private void Skin_Click(object sender, RoutedEventArgs e) => PopupConfig.IsOpen = true;

        private void About_Click(object sender, RoutedEventArgs e)
        {
            foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
            {
                if (window.Name.Contains("MainWindow"))
                {
                    (window.DataContext as MainWindowViewModel).windowManager.ShowDialog(new AboutInperSignalViewModel());
                }
            }
        }
    }
}
