using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Bean
{
    public class BindableObject : INotifyPropertyChanged
    {
        protected Dictionary<string, object> Values = new Dictionary<string, object>();

        #region 接口事件

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region 共有方法

        public T GetValue<T>(Expression<Func<T>> expression)
        {
            return GetValue<T>(GetPropertyName(expression));
        }

        public void SetValue<T>(Expression<Func<T>> expression, T value, Action action = null)
        {
            string propertyName = GetPropertyName(expression);
            if (Values.Keys.Contains(propertyName))
            {
                Values[propertyName] = value;
            }
            else
            {
                Values.Add(propertyName, value);
            }
            OnPropertyChanged(propertyName);
            action?.Invoke();
        }

        #endregion

        #region 私有方法

        private T GetValue<T>(string propertyName)
        {
            if (Values.Keys.Contains(propertyName))
                return (T)Values[propertyName];
            else
                return default;
        }

        private string GetPropertyName(Expression expression)
        {
            MemberExpression me = (expression as LambdaExpression).Body as MemberExpression;
            return (me.Member as MemberInfo).Name;
        }

        #endregion
    }
}
