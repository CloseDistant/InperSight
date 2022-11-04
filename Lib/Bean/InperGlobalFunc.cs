using HandyControl.Controls;
using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace InperSight.Lib.Bean
{
    public class InperGlobalFunc
    {
        public static System.Windows.Window GetWindowByNameChar(string cft)
        {
            if (cft != null)
            {
                if (Application.Current.Windows.OfType<System.Windows.Window>().Count() > 1)
                {
                    var window = Application.Current.Windows.OfType<System.Windows.Window>().FirstOrDefault(x => x.Title.Contains(cft));

                    return window ?? default;
                }

            }
            return default;

        }
        public static childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
        /// <summary>
        /// 消息提醒 0-info 1-warning 2-error
        /// </summary>
        /// <param name="text"></param>
        /// <param name="type"></param>
        public static void ShowRemainder(string text, int type = 0)
        {
            switch (type)
            {
                case 0:
                    Growl.Info(new GrowlInfo() { Message = text, Token = "SuccessMsg", WaitTime = 1 });
                    break;
                case 1:
                    Growl.Warning(new GrowlInfo() { Message = text, Token = "SuccessMsg", WaitTime = 1 });
                    break;
                case 2:
                    Growl.Error(new GrowlInfo() { Message = text, Token = "SuccessMsg", WaitTime = 1 });
                    break;
            }
        }
        public static void SaveScreenToImageByPoint(int left, int top, int width, int height, string path)
        {
            Image image = new Bitmap(width, height);
            Graphics gc = Graphics.FromImage(image);
            gc.CopyFromScreen(left, top, 0, 0, new System.Drawing.Size(width, height));

            image.Save(path, System.Drawing.Imaging.ImageFormat.Bmp);
        }
    }
}
