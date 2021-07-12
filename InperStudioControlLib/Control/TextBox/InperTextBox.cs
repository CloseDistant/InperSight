using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace InperStudioControlLib.Control.TextBox
{
    public class InperTextBox : HandyControl.Controls.TextBox
    {


        #region DependencyProperty
        public static readonly DependencyProperty InperMaxValueProperty = DependencyProperty.Register(
          "InperMaxValue", typeof(double), typeof(InperTextBox), new PropertyMetadata(0));
        public double InperMaxValue
        {
            get => (double)GetValue(dp: InperMaxValueProperty);
            set => SetValue(dp: InperMaxValueProperty, value);
        }
        public static readonly DependencyProperty InperMinValueProperty = DependencyProperty.Register(
         "InperMinValue", typeof(double), typeof(InperTextBox), new PropertyMetadata(0));
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
        }

        private void InperTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (InperVerify)
            {
                var tbox = sender as InperTextBox;
                if (!string.IsNullOrEmpty(tbox.Text))
                {
                    Regex rx = new Regex(@"^[0-9]*$");
                    if (rx.IsMatch(tbox.Text))
                    {
                        var res = double.Parse(tbox.Text);
                        if (res < InperMinValue)
                        {
                            
                        }
                    }
                }
            }
        }
    }
}
