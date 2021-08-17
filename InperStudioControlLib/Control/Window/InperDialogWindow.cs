using HandyControl.Controls;
using HandyControl.Tools;
using InperStudioControlLib.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace InperStudioControlLib.Control.Window
{
    public class InperDialogWindow : System.Windows.Window
    {
        #region event property
        public event Action<object, ExecutedRoutedEventArgs> ConfirmClickEvent;
        public event Action<object> OtherClickEvent;
        #endregion

        public InperDialogWindow()
        {
            Loaded += (s, e) =>
            {
                _ = CommandBindings.Add(new CommandBinding(CustomCommands.CancleCommand, CloseButton_Event));
                _ = CommandBindings.Add(new CommandBinding(CustomCommands.ConfirmCommand, ConfirmButton_Event));
                _ = CommandBindings.Add(new CommandBinding(CustomCommands.OtherCommand, OtherButton_Event));
                _ = CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, (ss, ee) => WindowState = WindowState.Minimized));
                _ = CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, MaxiButton_Event));
                _ = CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseButton_Event));

                (this.Template.FindName("MovePanel", this) as SimplePanel).MouseDown += (se, arg) =>
                {
                    if (arg.LeftButton == MouseButtonState.Pressed)
                    {
                        DragMove();
                    }
                };
            };
            //DataContext = this;
        }
        static InperDialogWindow()
        {
            StyleProperty.OverrideMetadata(typeof(InperDialogWindow), new FrameworkPropertyMetadata(ResourceHelper.GetResource<Style>("InperDialogWindowStyle")));
        }
        private void MaxiButton_Event(object sender, ExecutedRoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }
        private void CloseButton_Event(object sender, ExecutedRoutedEventArgs e) => Close();
        private void ConfirmButton_Event(object sender, ExecutedRoutedEventArgs e) => ConfirmClickEvent?.Invoke(sender, e);
        private void OtherButton_Event(object sender, ExecutedRoutedEventArgs e) => OtherClickEvent?.Invoke(e);

        #region DependencyProperty
        public static readonly DependencyProperty IsShowMiniButtonProperty = DependencyProperty.Register(
            "IsShowMiniButton", typeof(bool), typeof(InperDialogWindow), new PropertyMetadata(true));
        public bool IsShowMiniButton
        {
            get => (bool)GetValue(IsShowMiniButtonProperty);
            set => SetValue(IsShowMiniButtonProperty, value);
        }
        public static readonly DependencyProperty IsShowMaxButtonProperty = DependencyProperty.Register(
           "IsShowMaxButton", typeof(bool), typeof(InperDialogWindow), new PropertyMetadata(true));
        public bool IsShowMaxButton
        {
            get => (bool)GetValue(IsShowMaxButtonProperty);
            set => SetValue(IsShowMaxButtonProperty, value);
        }
        public static readonly DependencyProperty IsShowTopAllButtonProperty = DependencyProperty.Register(
            "IsShowTopAllButton", typeof(bool), typeof(InperDialogWindow), new PropertyMetadata(true));
        public bool IsShowTopAllButton
        {
            get => (bool)GetValue(IsShowTopAllButtonProperty);
            set => SetValue(IsShowTopAllButtonProperty, value);
        }

        public static readonly DependencyProperty IsShowBottomAllButtonProperty = DependencyProperty.Register(
            "IsShowBottomAllButton", typeof(bool), typeof(InperDialogWindow), new PropertyMetadata(true));
        public bool IsShowBottomAllButton
        {
            get => (bool)GetValue(IsShowBottomAllButtonProperty);
            set => SetValue(IsShowBottomAllButtonProperty, value);
        }

        public static readonly DependencyProperty IsShowOtherButtonProperty = DependencyProperty.Register(
            "IsShowOtherButton", typeof(bool), typeof(InperDialogWindow), new PropertyMetadata(true));
        public bool IsShowOtherButton
        {
            get => (bool)GetValue(IsShowOtherButtonProperty);
            set => SetValue(IsShowOtherButtonProperty, value);
        }
        public static readonly DependencyProperty IsShowCancleButtonProperty = DependencyProperty.Register(
            "IsShowCancleButton", typeof(bool), typeof(InperDialogWindow), new PropertyMetadata(true));
        public bool IsShowCancleButton
        {
            get => (bool)GetValue(IsShowCancleButtonProperty);
            set => SetValue(IsShowCancleButtonProperty, value);
        }
        public static readonly DependencyProperty IsShowOkButtonProperty = DependencyProperty.Register(
           "IsShowOkButton", typeof(bool), typeof(InperDialogWindow), new PropertyMetadata(true));
        public bool IsShowOkButton
        {
            get => (bool)GetValue(IsShowOkButtonProperty);
            set => SetValue(IsShowOkButtonProperty, value);
        }

        public static readonly DependencyProperty IsShowTitleProperty = DependencyProperty.Register(
            "IsShowTitle", typeof(bool), typeof(InperDialogWindow), new PropertyMetadata(true));
        public bool IsShowTitle
        {
            get => (bool)GetValue(IsShowTitleProperty);
            set => SetValue(IsShowTitleProperty, value);
        }

        public static readonly DependencyProperty InperCustomDialogBottmProperty = DependencyProperty.Register(
            "InperCustomDialogBottm", typeof(object), typeof(InperDialogWindow), new PropertyMetadata(null));

        public object InperCustomDialogBottm
        {
            get => GetValue(InperCustomDialogBottmProperty);
            set => SetValue(InperCustomDialogBottmProperty, value);
        }
        #endregion
    }
}
