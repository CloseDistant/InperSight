using MathNet.Numerics.Statistics;
using System;

namespace InperStudio.Lib.Bean
{
    public class Derivative
    {
        private double _SamplingRate = 50;
        public double SamplingRate
        {
            get
            {
                return _SamplingRate;
            }
            set
            {
                _SamplingRate = value;
                InitBasicData();
            }
        }
        public double Tau0 { get; } = 0.2;     // De-Noising
        public double Tau1 { get; } = 0.75;    // Averaging
        public double Tau2 { get; } = 3;       // Minimization
        public double Beta { get; } = 0.2;     // Exponential Weight


        public Derivative()
        {
            Beta = Math.Exp(-(0.4 / Tau0));
            InitBasicData();
            return;
        }


        public Derivative(double sampling_rate, double tau0 = 0.2, double tau1 = 0.75, double tau2 = 3)
        {
            SamplingRate = sampling_rate;
            Tau0 = tau0;
            Tau1 = tau1;
            Tau2 = tau2;
            Beta = Math.Exp(-(0.4 / Tau0));
            InitBasicData();

            return;
        }

        private MovingStatistics _BaseLineAvr;
        private MovingStatistics _Avrs;

        private double LastResult = 0;
        public double ProcessSignal(double signal)
        {
            _BaseLineAvr.Push(signal);
            _Avrs.Push(_BaseLineAvr.Mean);
            double MinBLWindAverage = _Avrs.Minimum;
            double r = MinBLWindAverage == 0 ? 0 : (signal - MinBLWindAverage) / MinBLWindAverage;
            LastResult = (Beta * LastResult) + (1 - Beta) * r;

            return LastResult;
        }


        private int _WindowSize;
        private int _BaseLineWindowSize;

        private void InitBasicData()
        {
            _WindowSize = (int)((Tau2 + (Tau1 / 2)) * _SamplingRate);
            _WindowSize = _WindowSize > 0 ? _WindowSize : 1;
            _BaseLineWindowSize = (int)(Tau1 * _SamplingRate);
            _BaseLineWindowSize = _BaseLineWindowSize > 0 ? _BaseLineWindowSize : 1;
            _BaseLineAvr = new MovingStatistics(_BaseLineWindowSize);
            _Avrs = new MovingStatistics(_WindowSize);
            return;
        }
    }
}

