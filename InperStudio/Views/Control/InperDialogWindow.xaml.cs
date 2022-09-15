using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace InperStudio.Views.Control
{
    /// <summary>
    /// InperDialogWindow.xaml 的交互逻辑
    /// </summary>
    public partial class InperDialogWindow : Window
    {
        public event EventHandler<int> ClickEvent;
        public InperDialogWindow(string text)
        {
            InitializeComponent();
            this.tipsContent.Text = text;
        }
        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            ClickEvent?.Invoke(this, 0);
            this.Close();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            ClickEvent?.Invoke(this, 1);
            this.Close();
        }

        private void Cancle_Click(object sender, RoutedEventArgs e)
        {
            ClickEvent?.Invoke(this, 2);
            this.Close();
        }
    }
}
