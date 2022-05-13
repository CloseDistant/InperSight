using HandyControl.Controls;
using HandyControl.Data;
using InperStudioControlLib.Control.InperAdorner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace InperStudioControlLib.Control.TextBox
{
    public enum InperTextType
    {
        String,
        Double,
        Int
    }
    public class InperTextBox : HandyControl.Controls.TextBox
    {
        #region DependencyProperty
        public static readonly DependencyProperty InperTextTypeProperty = DependencyProperty.Register(
            "InperTextType", typeof(InperTextType), typeof(InperTextBox), new PropertyMetadata(null));
        public InperTextType InperTextType
        {
            get => (InperTextType)GetValue(dp: InperTextTypeProperty);
            set => SetValue(dp: InperTextTypeProperty, value);
        }
        public static readonly DependencyProperty InperStringLengthProperty = DependencyProperty.Register(
            "InperStringLength", typeof(int), typeof(InperTextBox), new PropertyMetadata(null));
        public double InperStringLength
        {
            get => (int)GetValue(dp: InperStringLengthProperty);
            set => SetValue(dp: InperStringLengthProperty, value);
        }
        public static readonly DependencyProperty InperMaxValueProperty = DependencyProperty.Register(
          "InperMaxValue", typeof(double), typeof(InperTextBox), new PropertyMetadata(null));
        public double InperMaxValue
        {
            get => (double)GetValue(dp: InperMaxValueProperty);
            set => SetValue(dp: InperMaxValueProperty, value);
        }
        public static readonly DependencyProperty InperMinValueProperty = DependencyProperty.Register(
         "InperMinValue", typeof(double), typeof(InperTextBox), new PropertyMetadata(null));
        public double InperMinValue
        {
            get => (double)GetValue(dp: InperMinValueProperty);
            set => SetValue(dp: InperMinValueProperty, value);
        }
        public static readonly DependencyProperty InperVerifyProperty = DependencyProperty.Register(
        "InperVerify", typeof(bool), typeof(InperTextBox), new PropertyMetadata(false));
        public bool InperVerify
        {
            get => (bool)GetValue(dp: InperVerifyProperty);
            set => SetValue(dp: InperVerifyProperty, value);
        }
        #endregion
        public InperTextBox()
        {
            this.TextChanged += InperTextBox_TextChanged;
            //this.LostFocus += InperTextBox_LostFocus;
        }

        private void InperTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                InperTextBox tbox = sender as InperTextBox;
                if (string.IsNullOrEmpty(tbox.Text))
                {
                    return;
                }
                Regex rx = new Regex(@"^[+-]?\d*[.]?\d*$");
                if (rx.IsMatch(tbox.Text))
                {
                    double res = double.Parse(tbox.Text);
                    if (res < InperMinValue)
                    {
                        this.Foreground = Brushes.Red;
                        Growl.Warning(new GrowlInfo() { Message = "不能小于" + InperMinValue, Token = "SuccessMsg", WaitTime = 1 });
                        this.Text = InperMinValue.ToString();
                        return;
                    }
                    if (res > InperMaxValue && InperMaxValue > InperMinValue)
                    {
                        this.Foreground = Brushes.Red;
                        Growl.Warning(new GrowlInfo() { Message = "不能大于" + InperMaxValue, Token = "SuccessMsg", WaitTime = 1 });
                        this.Text = InperMaxValue.ToString();
                        return;
                    }
                }
            }
            finally
            {
            }
        }

        public event Action<object, System.Windows.Controls.TextChangedEventArgs> InperTextChanged;
        private void InperTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            InperTextBox tbox = sender as InperTextBox;
            if (InperVerify)
            {
                try
                {
                    if (this.InperTextType == InperTextType.Double || this.InperTextType == InperTextType.Int)
                    {
                        if (!string.IsNullOrEmpty(tbox.Text))
                        {
                            Regex rx = new Regex(@"^[+-]?\d*[.]?\d*$");
                            if (rx.IsMatch(tbox.Text))
                            {
                                double res = double.Parse(tbox.Text);
                                if (res < InperMinValue)
                                {
                                    this.Foreground = Brushes.Red;
                                    //Growl.Warning(new GrowlInfo() { Message = "The value cannot be less than " + InperMinValue, Token = "SuccessMsg", WaitTime = 1 });
                                    this.Text = InperMinValue.ToString();
                                    return;
                                }
                                if (res > InperMaxValue && InperMaxValue > InperMinValue)
                                {
                                    this.Foreground = Brushes.Red;
                                    //Growl.Warning(new GrowlInfo() { Message = "The value cannot exceed " + InperMaxValue, Token = "SuccessMsg", WaitTime = 1 });
                                    this.Text = InperMaxValue.ToString();
                                    return;
                                }
                            }
                            else
                            {
                                this.Foreground = Brushes.Red;
                                //Growl.Warning(new GrowlInfo() { Message = "Invalid input", Token = "SuccessMsg", WaitTime = 1 });
                                return;
                            }
                        }
                    }
                    if (this.InperTextType == InperTextType.String)
                    {
                        if (!string.IsNullOrEmpty(tbox.Text))
                        {
                            if (tbox.Text.Length > InperStringLength)
                            {
                                this.Foreground = Brushes.Red;
                                //Growl.Warning(new GrowlInfo() { Message = "No more than " + InperStringLength + " characters", Token = "SuccessMsg", WaitTime = 1 });
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            this.Foreground = Brushes.Black;
            InperTextChanged?.Invoke(sender, e);
        }
    }
}
