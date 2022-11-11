using HandyControl.Controls;
using InperSight.Lib.Bean;
using InperSight.Lib.Config;
using InperSight.Views;
using InperStudioControlLib.Lib.Config;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Screen = Stylet.Screen;

namespace InperSight.ViewModels
{
    public enum DataConfigPathTypeEnum
    {
        Path,
        Load,
        Save
    }
    public class DataPathConfigViewModel : Screen
    {
        #region properties
        private readonly DataConfigPathTypeEnum typeEnum;
        private DataPathConfigView view;
        public static ObservableCollection<Lib.Bean.Data.Model.Tag> DataList { get; set; } = new ObservableCollection<Lib.Bean.Data.Model.Tag>();
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
            DataList.Insert(0, new Lib.Bean.Data.Model.Tag
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
                    System.Windows.Window window = InperGlobalFunc.GetWindowByNameChar("Camera Signal");
                    if (window != null)
                    {
                        window.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        public void InperDialogWindow_ConfirmClickEvent(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                string newName = view.fileName.Text;

                if (string.IsNullOrEmpty(newName))
                {
                    InperGlobalFunc.ShowRemainder("The file name can’t be empty!");
                    return;
                }
                if (newName != InperGlobalClass.DataFolderName)
                {
                    if (Directory.Exists(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName)))
                    {
                        if (Directory.GetFiles(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName)).Count() > 0)
                        {
                            if (System.Windows.MessageBox.Show(InperGlobalClass.DataFolderName + "文件夹中检测到数据文件，是否创建新的文件夹?", "Ask", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                            {
                                view.fileName.Text = InperGlobalClass.DataFolderName;
                                return;
                            }
                            else
                            {
                                InperGlobalClass.DataFolderName = newName;
                                Directory.CreateDirectory(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName));
                            }
                        }
                        else
                        {
                            Directory.Delete(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName));
                            InperGlobalClass.DataFolderName = newName;
                            Directory.CreateDirectory(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName));
                        }
                    }
                    else
                    {
                        InperGlobalClass.DataFolderName = newName;
                        Directory.CreateDirectory(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName));
                    }

                }
                RequestClose();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
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
                LoggerHelper.Error(ex.ToString());
            }
        }
        #endregion
    }
}
