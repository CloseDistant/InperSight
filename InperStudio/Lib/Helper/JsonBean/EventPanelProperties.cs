﻿using SciChart.Charting.Model.ChartSeries;
using System.Linq;
using System.Threading.Tasks;

namespace InperStudio.Lib.Helper.JsonBean
{
    public class EventPanelProperties
    {
        public bool HeightAuto { get; set; } = false;
        public bool HeightFixed { get; set; } = true;
        public double HeightFixedValue { get; set; } = 100;
        public bool DisplayLockedBottom { get; set; } = true;
        private bool displayNameVisible = true;
        public bool DisplayNameVisible
        {
            get { return displayNameVisible; }
            set
            {
                displayNameVisible = value;

                if (InperDeviceHelper.Instance.EventChannelChart.Annotations.Count > 0)
                {
                    Task.Factory.StartNew(() =>
                    {
                        InperDeviceHelper.Instance.EventChannelChart.Annotations.ToList().ForEach(x =>
                        {
                            (x as VerticalLineAnnotationViewModel).ShowLabel = value;
                        });
                    });
                }

            }
        }
    }
}
