using InperStudio.Lib.Bean;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using InperStudioControlLib.Lib.Config;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Media;

namespace InperStudio.ViewModels
{
    public class PreferenceWindowViewModel : Screen
    {
        private PreferenceWindowView view;
        public List<string> SkinColorList { get; set; } = InperColorHelper.ColorPresetList;
        public PreferenceWindowViewModel()
        {
        }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            view = this.View as PreferenceWindowView;
            if (InperConfig.Instance.ThemeColor == "#6523A5")
            {
                view._default.IsChecked = true;
            }
            else
            {
                view.custom.IsChecked = true;
                view.AnalogColorList.SelectedIndex = SkinColorList.IndexOf(InperConfig.Instance.ThemeColor);
            }
            if (InperConfig.Instance.Language == "en_us")
            {
                view.en_us.IsChecked = true;
            }
            else
            {
                view.zh_cn.IsChecked = true;
            }
            view.analog.IsChecked = InperGlobalClass.IsDisplayAnalog;
            view.trigger.IsChecked = InperGlobalClass.IsDisplayTrigger;
            view.note.IsChecked = InperGlobalClass.IsDisplayNote;

            view.ConfirmClickEvent += View_ConfirmClickEvent;
        }

        private void View_ConfirmClickEvent(object arg1, System.Windows.Input.ExecutedRoutedEventArgs arg2)
        {
            this.RequestClose();
        }

        public void PopButton_Click(object sender, RoutedEventArgs e)
        {
            this.view.pop.IsOpen = true;
        }
        public void AnalogColorList_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = (sender as System.Windows.Controls.ListBox).SelectedItem;
                Application.Current.Resources["InperTheme"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(item.ToString()));
                InperConfig.Instance.ThemeColor = item.ToString();
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Default_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Resources["InperTheme"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6523A5"));
                InperConfig.Instance.ThemeColor = "#6523A5";
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Custom_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                view.AnalogColorList.SelectedIndex = 0;
                Application.Current.Resources["InperTheme"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(view.AnalogColorList.SelectedItem.ToString()));
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void English_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //Application.Current.Resources.MergedDictionaries.RemoveAt(Application.Current.Resources.MergedDictionaries.Count - 1);
                //Application.Current.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri("/InperStudio;component/Lib/Resources/en_us.xaml", UriKind.Relative)) as ResourceDictionary);
                //Application.Current.Resources["InperFontFamily"] = "Arial";
                InperConfig.Instance.Language = "en-US";
                InperClassHelper.SetLanguage(InperConfig.Instance.Language);
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Chinese_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //Application.Current.Resources.MergedDictionaries.RemoveAt(Application.Current.Resources.MergedDictionaries.Count - 1);
                //Application.Current.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri("/InperStudio;component/Lib/Resources/zh_cn.xaml", UriKind.Relative)) as ResourceDictionary);
                //Application.Current.Resources["InperFontFamily"] = "微软雅黑";
                InperConfig.Instance.Language = "zh_cn";
                InperClassHelper.SetLanguage(InperConfig.Instance.Language);
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Display_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var cb = sender as System.Windows.Controls.CheckBox;
                if (cb != null)
                {
                    if (cb.Name == "analog")
                    {
                        InperGlobalClass.IsDisplayAnalog = true;
                        InperProductConfig.DisplayNodeWrite(DisplayEnum.Analog, true);
                    }
                    if (cb.Name == "trigger")
                    {
                        InperGlobalClass.IsDisplayTrigger = true;
                        InperProductConfig.DisplayNodeWrite(DisplayEnum.Trigger, true);
                    }
                    if (cb.Name == "note")
                    {
                        InperGlobalClass.IsDisplayNote = true;
                        InperProductConfig.DisplayNodeWrite(DisplayEnum.Note, true);
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Display_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                var cb = sender as System.Windows.Controls.CheckBox;
                if (cb != null)
                {
                    if (cb.Name == "analog")
                    {
                        InperGlobalClass.IsDisplayAnalog = false;
                        InperProductConfig.DisplayNodeWrite(DisplayEnum.Analog, false);
                    }
                    if (cb.Name == "trigger")
                    {
                        InperGlobalClass.IsDisplayTrigger = false;
                        InperProductConfig.DisplayNodeWrite(DisplayEnum.Trigger, false);
                    }
                    if (cb.Name == "note")
                    {
                        InperGlobalClass.IsDisplayNote = false;
                        InperProductConfig.DisplayNodeWrite(DisplayEnum.Note, false);
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
    }
}
