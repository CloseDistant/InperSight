using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InperStudio.Views.Control
{
    /// <summary>
    /// ResizableRectangle.xaml 的交互逻辑
    /// </summary>
    public partial class ResizableRectangle : UserControl
    {
        public ResizableRectangle()
        {
            InitializeComponent();
        }
        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double offsetX = e.HorizontalChange;
            double offsetY = e.VerticalChange;

            if (sender == bottomRightThumb)
            {
                double newWidth = this.ActualWidth + offsetX;
                double newHeight = this.ActualHeight + offsetY;

                double maxWidth = (this.Parent as Canvas).ActualWidth - Canvas.GetLeft(this);
                double maxHeight = (this.Parent as Canvas).ActualHeight - Canvas.GetTop(this);

                if (newWidth > maxWidth)
                    newWidth = maxWidth;

                if (newHeight > maxHeight)
                    newHeight = maxHeight;


                if (newWidth > 0 && newHeight > 0)
                {
                    this.Width = newWidth;
                    this.Height = newHeight;
                }
            }
        }
        private void DragThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double left = Canvas.GetLeft(this);
            double top = Canvas.GetTop(this);

            // 计算新的位置
            double newLeft = left + e.HorizontalChange;
            double newTop = top + e.VerticalChange;

            // 确保新的位置在Canvas的边界范围内
            double canvasWidth = (this.Parent as Canvas).ActualWidth;
            double canvasHeight = (this.Parent as Canvas).ActualHeight;
            double controlWidth = this.ActualWidth;
            double controlHeight = this.ActualHeight;
           
            if (newLeft < 0)
                newLeft = 0;
            else if (newLeft + controlWidth > canvasWidth)
                newLeft = canvasWidth - controlWidth;

            if (newTop < 0)
                newTop = 0;
            else if (newTop + controlHeight > canvasHeight)
                newTop = canvasHeight - controlHeight;

            // 更新控件的位置
            Canvas.SetLeft(this, newLeft);
            Canvas.SetTop(this, newTop);
        }

        private void dragThumb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        // ... The rest of the code for resizing using thumbs remains the same ...
    }
}
