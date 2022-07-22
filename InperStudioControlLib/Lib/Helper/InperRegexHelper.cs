using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InperStudioControlLib.Lib.Helper
{
    public class InperRegexHelper
    {
        /// <summary>
        /// 是否包含特殊字符（只包含汉字 字母 数字 返回true）
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsSpecialChar(string str)
        {
            Regex regExp = new Regex("[^0-9a-zA-Z\u4e00-\u9fa5]");
            if (regExp.IsMatch(str))
            {
                return true;
            }
            return false;
        }

    }
}
