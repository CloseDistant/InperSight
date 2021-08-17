using InperStudio.Lib.Bean;
using InperStudio.Lib.Data.Model;
using InperStudio.Lib.Helper.JsonBean;
using InperStudio.Views;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace InperStudio.ViewModels
{
    public class LeftToolsControlViewModel : Screen
    {
        #region properties
        private IWindowManager windowManager;
        private BindableCollection<EventChannelJson> manualEvents = new BindableCollection<EventChannelJson>();
        public BindableCollection<EventChannelJson> ManulEvents { get => manualEvents; set => SetAndNotify(ref manualEvents, value); }
        #endregion

        #region
        public LeftToolsControlViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
            ManulEvents = EventSettingsViewModel.ManualEvents;
        }
        public LeftToolsControlViewModel()
        {
        }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
        }

        public void NoteContent_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {

                    RichTextBox rich = sender as RichTextBox;

                    ScrollViewer scroll = (rich.Parent as Grid).FindName("TagScroll") as ScrollViewer;

                    StackPanel stack = scroll.FindName("TagListValue") as StackPanel;

                    string text = new TextRange(rich.Document.ContentStart, rich.Document.ContentEnd).Text.Replace("\r\n", "");

                    rich.Document.Blocks.Clear();
                    if (!string.IsNullOrEmpty(text))
                    {
                        TextBlock tb = new TextBlock() { MaxWidth = 300, FontSize = 12, FontFamily = new FontFamily("Arial"), Foreground = new SolidColorBrush(Color.FromRgb(132, 132, 132)), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 3, 0, 3) };
                        string time = DateTime.Now.ToString("G");

                        tb.Text = time + @"：" + text;
                        _ = stack.Children.Add(tb);
                        
                        _ = App.SqlDataInit.sqlSugar.Insertable<Note>(new Note()
                        {
                            Text = text,
                            CreateTime = DateTime.Parse(time)
                        }).ExecuteCommand();
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        #endregion
    }
}
