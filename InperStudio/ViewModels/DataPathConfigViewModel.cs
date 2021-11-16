using HandyControl.Controls;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Data.Model;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using InperStudioControlLib.Lib.Config;
using InperStudioControlLib.Lib.DeviceAgency;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    if (window != null)
                    {
                        window.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void InperDialogWindow_ConfirmClickEvent(object sender, ExecutedRoutedEventArgs e)
        {
            if (Directory.Exists(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName)))
            {
                Growl.Warning("Filename exsited", "SuccessMsg");
            }
            else
            {
                RequestClose();
            }
        }
        #endregion
    }
}
