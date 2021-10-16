using System;

namespace Shamisen.Core.Tests.CoreFx.TestUtils
{
    public readonly struct NeumaierAccumulator
    {
        private readonly double sum, c;

        public NeumaierAccumulator(double sum, double c)
        {
            this.sum = sum;
            this.c = c;
        }

        public double Sum => sum + c;

        public static NeumaierAccumulator operator +(NeumaierAccumulator left, double right)
        {
            double sum = left.sum;
            double c = left.c;
            double t = sum + right;
            if (Math.Abs(sum) >= Math.Abs(right))
            {
                c += sum - t + right;
            }
            else
            {
                c += right - t + sum;
            }
            sum = t;
            return new(sum, c);
        }

        public static NeumaierAccumulator operator -(NeumaierAccumulator left, double right) => left + -right;

    }
}
