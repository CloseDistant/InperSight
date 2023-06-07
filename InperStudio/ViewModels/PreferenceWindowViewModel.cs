using InperStudio.Lib.Bean;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using InperStudioControlLib.Lib.Config;
using SciChart.Core.Extensions;
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
                view.AnalogColorList.SelectedIndex = SkinColorList.IndexOf(InperConfig.Instance.ThemeColor);
                view.custom.IsChecked = true;
            }
            if (InperConfig.Instance.Language.ToLower() == "en_us")
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
            view.sprit.IsChecked = InperGlobalClass.IsDisplaySprit;

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
                if (InperConfig.Instance.ThemeColor == "#6523A5")
                {
                    view.AnalogColorList.SelectedIndex = 0;
                }
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
                InperConfig.Instance.Language = "en_us";
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
                        InperJsonHelper.SetDisplaySetting("true", "analog");
                        //InperProductConfig.DisplayNodeWrite(DisplayEnum.Analog, true);
                        var window = InperClassHelper.GetWindowByNameChar("inper");
                        if (window != null)
                        {
                            (window.DataContext as MainWindowViewModel).LeftToolsControlViewModel.InitConfig();
                        }
                    }
                    if (cb.Name == "trigger")
                    {
                        InperGlobalClass.IsDisplayTrigger = true;
                        InperJsonHelper.SetDisplaySetting("true", "trigger");
                        //InperProductConfig.DisplayNodeWrite(DisplayEnum.Trigger, true);
                    }
                    if (cb.Name == "note")
                    {
                        InperGlobalClass.IsDisplayNote = true;
                        InperJsonHelper.SetDisplaySetting("true", "note");
                        //InperProductConfig.DisplayNodeWrite(DisplayEnum.Note, true);
                    }
                    if (cb.Name == "sprit")
                    {
                        InperGlobalClass.IsDisplaySprit = true;
                        InperJsonHelper.SetDisplaySetting("true", "sprit");
                        //InperProductConfig.DisplayNodeWrite(DisplayEnum.Sprit, true);
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
                        InperJsonHelper.SetDisplaySetting("false", "analog");
                        //InperProductConfig.DisplayNodeWrite(DisplayEnum.Analog, false);

                        var window = InperClassHelper.GetWindowByNameChar("inper");
                        if (window != null)
                        {
                            RoutedEventArgs eventArgs = new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent, (window.DataContext as MainWindowViewModel).LeftToolsControlViewModel.view.bd1);
                            (window.DataContext as MainWindowViewModel).LeftToolsControlViewModel.view.bd1.RaiseEvent(eventArgs);
                            (window.DataContext as MainWindowViewModel).LeftToolsControlViewModel.CameraShow();
                            InperDeviceHelper.Instance.CameraChannels.RemoveWhere(x => x.Type == ChannelTypeEnum.Analog.ToString());
                        }
                    }
                    if (cb.Name == "trigger")
                    {
                        InperGlobalClass.IsDisplayTrigger = false;
                        InperJsonHelper.SetDisplaySetting("false", "trigger");
                        //InperProductConfig.DisplayNodeWrite(DisplayEnum.Trigger, false);
                        InperGlobalClass.AdditionRecordConditionsStart = AdditionRecordConditionsTypeEnum.Immediately;
                        InperGlobalClass.AdditionRecordConditionsStop = AdditionRecordConditionsTypeEnum.Immediately;
                        InperGlobalClass.EventSettings.Channels.RemoveWhere(x => x.Type == ChannelTypeEnum.TriggerStart.ToString() || x.Type == ChannelTypeEnum.TriggerStop.ToString());
                    }
                    if (cb.Name == "note")
                    {
                        InperGlobalClass.IsDisplayNote = false;
                        //InperProductConfig.DisplayNodeWrite(DisplayEnum.Note, false);
                        InperJsonHelper.SetDisplaySetting("false", "note");
                    }
                    if (cb.Name == "sprit")
                    {
                        InperGlobalClass.IsDisplaySprit = false;
                        //InperProductConfig.DisplayNodeWrite(DisplayEnum.Sprit, false);
                        InperJsonHelper.SetDisplaySetting("false", "sprit");
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
