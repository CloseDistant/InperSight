using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static InperStudio.Lib.Helper.FilterTools.FilterData;

namespace InperStudio.Lib.Helper.FilterTools
{
    /// <summary>
    /// (Fs/2)*Wc=fc，截止频率为fc【Wc为归一化频率】
    /// 相位延迟：(1/Fs)*(N-1)/2;
    /// 阶数：N-1(N为长度)
    /// </summary>
    public class FiltterTool
    {
        /// <summary>
        /// RC高通滤波
        /// </summary>
        /// <param name="DataArray">数据源</param>
        /// <param name="fc">截止频率</param>
        /// <param name="fl">采样频率</param>
        /// <returns></returns>
        public static double RCHighFilter(double[] DataArray, double fc, double fl)
        {
            double RC = 1 / (2 * Math.PI * fc);
            double a = RC / (RC + (1 / fl));//滤波系数
            double[] result = new double[DataArray.Length];
            result[0] = DataArray[0];
            for (int i = 1; i < DataArray.Length; i++)
            {
                result[i] = a * (DataArray[i] - DataArray[i - 1]) + a * result[i - 1];
            }
            return result.Last();
        }
        /// RC低通滤波
        /// </summary>
        /// <param name="DataArray">数据源</param>
        /// <param name="fc">截止频率</param>
        /// <param name="fl">采样频率</param>
        public static double RCLowPass(double[] DataArray, double fc, double fl)
        {
            double a = fc * 2 * Math.PI / fl; //滤波系数
            double[] result = new double[DataArray.Length];
            result[0] = DataArray[0];
            for (int i = 1; i < DataArray.Length; i++)
            {
                result[i] = a * DataArray[i] + (1 - a) * result[i - 1];
            }
            return result.Last();
        }
        static FilterData filterData = new FilterData();
        public static double NotchPass(double input, double framerate, double frequency)
        {
            if (filterData.NotchFilter == null)
            {
                filterData.Start(framerate, frequency);
            }
            else
            {
                input = filterData.Update(input);
            }
            return input;
        }
    }
    public class IIRFilter
    {
        public float a1;
        public float a2;

        public float b0;
        public float b1;
        public float b2;

        public float x1;
        public float x2;
        public float y1;
        public float y2;

        // two parameters indicate a 2nd order Butterworth low-pass filter
        // equation obtained here: https://www.codeproject.com/Tips/1092012/A-Butterworth-Filter-in-Csharp
        // note that you can also use the five-parameter-solution described below. This is just for convenience. 
        public IIRFilter(float samplingrate, float frequency)
        {
            const float pi = 3.14159265358979f;
            const float k = 1.966092f;//运放负反馈增益
            float ts = 1 / samplingrate;//采样周期
            float w0 = 2 * pi * frequency; //陷波器中心角频率
            float Q = 1 / (4 - 2 * k);//品质因数

            float m0 = (float)(Q * Math.Sqrt(ts) * Math.Sqrt(w0) + 4 * Q);
            float m1 = (float)-(8 * Q - 2 * Q * Math.Sqrt(ts) * Math.Sqrt(w0));
            float m2 = m0;
            float n0 = (float)(Q * Math.Sqrt(ts) * Math.Sqrt(w0) + 2 * ts * w0 + 4 * Q);
            float n1 = m1;
            float n2 = (float)(Q * Math.Sqrt(ts) * Math.Sqrt(w0) - 2 * ts * w0 + 4 * Q);

            a1 = (-n1) / n0;
            a2 = (-n2) / n0;
            b0 = m0 / n0;
            b1 = m1 / n0;
            b2 = m2 / n0;
        }
    }
    public class FilterData
    {

        public IIRFilter NotchFilter;
        public void Start(double framerate, double frequency)
        {
            NotchFilter = new IIRFilter((float)framerate, (float)frequency); // 2nd order low pass butterworth 26hz at 500Hz sampling rate
        }


        public double Update(double input)
        {
            // input = SomeDataStream; // continuously stream your data in here. Need not be in Update(), as this is limited to 60Hz. Streaming data into Unity is not part of this script. 
            return filter((float)input);
        }

        // filter data. Each IIRFilter stores two data points of filtered and unfiltered data. Therefore, filtering should be continuous and not be switched on and off. 
        // Furthermore, each IIRFilter may only process one data stream. If you intend to filter two data streams with the same kind of filter, you need to initialize 
        // two IIRFilters accordingly (e.g. "Notch50_1" and "Notch50_2"), each filtering only one data stream. 
        private float filter(float x0)
        {
            float y = 0;
            try
            {
                // float y = iirfilter.a1 * x0 + iirfilter.a2 * iirfilter.x1 + iirfilter.b0 * iirfilter.x2 + iirfilter.b1 * iirfilter.y1 + iirfilter.b2 * iirfilter.y2;
                y = x0 * NotchFilter.b0 + NotchFilter.x1 * NotchFilter.b1 + NotchFilter.x2 * NotchFilter.b2 + NotchFilter.y1 * NotchFilter.a1 + NotchFilter.y2 * NotchFilter.a2;

                NotchFilter.x2 = NotchFilter.x1;
                NotchFilter.x1 = x0;
                NotchFilter.y2 = NotchFilter.y1;
                NotchFilter.y1 = y;

            }
            catch (Exception ex)
            {

            }
            return y;
        }
    }
}
