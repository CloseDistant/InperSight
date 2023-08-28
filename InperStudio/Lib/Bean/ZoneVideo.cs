using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace InperStudio.Lib.Bean
{
    public class ZoneVideo
    {
        /// <summary>
        /// zone的名称
        /// </summary>
        public string Name { get; set; }
        public string DisplayName { get; set; }
        /// <summary>
        /// 相机自定义名称
        /// </summary>
        public string VideoShowName { get; set; }
        /// <summary>
        /// 相机原本名称
        /// </summary>
        public string VideoName { get; set; }
        public FrameworkElement Shape { get; set; }
        /// <summary>
        /// 是否展示
        /// </summary>
        public bool IsActive { get; set; } = false;
        /// <summary>
        /// 相机是否激活
        /// </summary>
        public bool IsActiveVideo { get; set; } = false;
    }
}
