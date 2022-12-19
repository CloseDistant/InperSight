﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Media;

namespace InperStudio.Lib.Helper
{
    public class InperClassHelper
    {
        public static object ClassDeepCopy(object _object)
        {
            Type T = _object.GetType();
            object o = Activator.CreateInstance(T);
            PropertyInfo[] PI = T.GetProperties();
            for (int i = 0; i < PI.Length; i++)
            {
                PropertyInfo P = PI[i];
                P.SetValue(o, P.GetValue(_object));
            }
            return o;
        }
        public static Window GetWindowByNameChar(string cft)
        {
            if (cft != null)
            {
                if (Application.Current.Windows.OfType<Window>().Count() > 1)
                {
                    var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.Title.Contains(cft));

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
        public static object GetAdditionRecordConditionsType()
        {
            return default;
        }
        public static void Clone<T>(T source, ref T destination)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, source);
                memoryStream.Position = 0;
                destination = (T)formatter.Deserialize(memoryStream);
            }
        }
    }
}