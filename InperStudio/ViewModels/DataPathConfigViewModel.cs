using InperStudio.Lib.Bean;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Screen = Stylet.Screen;
using Tag = InperStudio.Lib.Data.Model.Tag;

namespace InperStudio.ViewModels
{
    public class DataPathConfigViewModel : Screen
    {
        #region properties
        private readonly DataConfigPathTypeEnum typeEnum;
        private DataPathConfigView view;
        public static ObservableCollection<Tag> DataList { get; set; } = new ObservableCollection<Tag>();
        private string _tagName;

        public string TagName
        {
            get => _tagName;
            set => SetAndNotify(ref _tagName, value);
        }

        #endregion

        #region method
        public DataPathConfigViewModel(DataConfigPathTypeEnum typeEnum)
        {
            this.typeEnum = typeEnum;
        }
        protected override void OnViewLoaded()
        {
            view = this.View as DataPathConfigView;
            switch (typeEnum)
            {
                case DataConfigPathTypeEnum.Path:
                    view.Path.Visibility = System.Windows.Visibility.Visible;
                    break;
                case DataConfigPathTypeEnum.Load:
                    view.Load.Visibility = System.Windows.Visibility.Visible;
                    break;
                case DataConfigPathTypeEnum.Save:
                    view.Save.Visibility = System.Windows.Visibility.Visible;
                    break;
            }
            if (string.IsNullOrEmpty(view.pathText.Content.ToString()))
            {
                view.pathText.Content = System.Environment.CurrentDirectory + @"\Data\";
            }
            if (string.IsNullOrEmpty(view.fileName.Text))
            {
                view.fileName.Text = DateTime.Now.ToString("yyyyMMddHHmmss");
            }
        }
        public void AddItemCmd()
        {
            if (string.IsNullOrEmpty(TagName))
            {
                return;
            }
            DataList.Insert(0, new Tag
            {
                Value = TagName,
                Type = 0,
                DateTime = DateTime.Now
            });
            TagName = string.Empty;
        }
        public void ChoosePath(string type)
        {
            try
            {
                if (type == DataConfigPathTypeEnum.Path.ToString())
                {
                    FolderBrowserDialog openFileDialog = new FolderBrowserDialog();
                    openFileDialog.SelectedPath = InperGlobalClass.DataPath;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        view.pathText.Content = openFileDialog.SelectedPath + @"\";
                    }
                }
                if (type == DataConfigPathTypeEnum.Load.ToString())
                {
                    System.Windows.Window window = InperClassHelper.GetWindowByNameChar("Camera Signal");
                    window?.Close();
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void InperDialogWindow_ConfirmClickEvent(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                string newName = view.fileName.Text;

                if (string.IsNullOrEmpty(newName))
                {
                    InperGlobalClass.ShowReminderInfo("The file name can’t be empty!");
                    return;
                }
                if (newName != InperGlobalClass.DataFolderName)
                {
                    if (Directory.Exists(Path.Combine(InperGlobalClass.DataPath, newName)))
                    {
                        if (Directory.GetFiles(Path.Combine(InperGlobalClass.DataPath, newName)).Count() > 0)
                        {
                            if (System.Windows.MessageBox.Show(newName + "文件夹已存在，是否覆盖?", "Ask", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                            {
                                view.fileName.Text = InperGlobalClass.DataFolderName;
                                return;
                            }
                            else
                            {
                                InperGlobalClass.DataFolderName = newName;
                                DelectDir(Path.Combine(InperGlobalClass.DataPath, newName));
                                Directory.CreateDirectory(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName));
                            }
                        }
                        else
                        {
                            InperGlobalClass.DataFolderName = newName;
                            Directory.Delete(Path.Combine(InperGlobalClass.DataPath, newName));
                            Directory.CreateDirectory(Path.Combine(InperGlobalClass.DataPath, newName));
                        }
                    }
                    else
                    {
                        InperGlobalClass.DataFolderName = newName;
                        Directory.CreateDirectory(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName));
                    }
                }
                InperJsonHelper.SetDataPathSetting(InperGlobalClass.DataPath);
                RequestClose();
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void FileName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                //var tb = sender as System.Windows.Controls.TextBox;
                //InperGlobalClass.DataFolderName = tb.Text.Replace(" ", "");
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        #endregion
        public void DelectDir(string srcPath)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos(); //返回目录中所有文件和子目录
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo) //判断是否文件夹
                    {
                        DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                        subdir.Delete(true); //删除子目录和文件
                    }
                    else
                    {
                        File.Delete(i.FullName); //删除指定文件
                    }
                }

            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void OpenFilePath()
        {
            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
                psi.Arguments = "/e,/select," + System.IO.Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName);
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
    }
}
