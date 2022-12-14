using InperSight.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenCvSharp.ML.DTrees;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using InperSight.Lib.Config;
using Stylet;
using InperSight.Lib.Bean.Data.Model;

namespace InperSight.ViewModels
{
    public class NoteSettingViewModel : Screen
    {
        public static List<Note> NotesCache = new List<Note>();
        public NoteSettingViewModel()
        {

        }
        protected override void OnViewLoaded()
        {
            (View as NoteSettingView).ConfirmClickEvent += NoteSettingViewModel_ConfirmClickEvent;

            if (NotesCache.Count > 0)
            {
                NotesCache.ForEach(x =>
                {
                    TextBlock tb = new TextBlock() { MaxWidth = 300, FontSize = 12, FontFamily = new FontFamily("Arial"), Foreground = new SolidColorBrush(Color.FromRgb(132, 132, 132)), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 3, 0, 3) };
                    tb.Text = x.CreateTime + @"：" + x.Text;
                    (View as NoteSettingView).TagListValue.Children.Add(tb);
                });
            }
        }

        private void NoteSettingViewModel_ConfirmClickEvent(object arg1, ExecutedRoutedEventArgs arg2)
        {
            var rich = (View as NoteSettingView).NoteContent;
            string text = new TextRange(rich.Document.ContentStart, rich.Document.ContentEnd).Text.Replace("\r\n", "");
            if (!string.IsNullOrEmpty(text))
            {
                ScrollViewer scroll = (rich.Parent as Grid).FindName("TagScroll") as ScrollViewer;

                StackPanel stack = scroll.FindName("TagListValue") as StackPanel;
                TextBlock tb = new TextBlock() { MaxWidth = 300, FontSize = 12, FontFamily = new FontFamily("Arial"), Foreground = new SolidColorBrush(Color.FromRgb(132, 132, 132)), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 3, 0, 3) };
                string time = DateTime.Now.ToString("G");

                tb.Text = time + @"：" + text;
                _ = stack.Children.Add(tb);
                Note note = new Note()
                {
                    Text = text,
                    CreateTime = DateTime.Parse(time)
                };

                NotesCache.Add(note);
            }

            this.RequestClose();
        }

        public void NoteContent_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                this.View.Dispatcher.BeginInvoke(new Action(() =>
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
                            Note note = new Note()
                            {
                                Text = text,
                                CreateTime = DateTime.Parse(time)
                            };
                            //if (App.SqlDataInit != null)
                            //{
                            //    _ = App.SqlDataInit.sqlSugar.Insertable(note).ExecuteCommand();
                            //}
                            //else
                            //{
                            NotesCache.Add(note);
                            //}
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
    }
}
