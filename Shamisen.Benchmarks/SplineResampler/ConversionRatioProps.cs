using System;
using System.Diagnostics;

namespace Shamisen.Benchmarks
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public struct ConversionRatioProps
    {
        public int Before { get; set; }
        public int After { get; set; }
        public string Strategy { get; set; }

        public ConversionRatioProps(int before, int after, string strategy)
        {
            Before = before;
            After = after;
            Strategy = strategy;
        }

        public override bool Equals(object obj) => obj is ConversionRatioProps other && Before == other.Before && After == other.After && Strategy == other.Strategy;
        public override int GetHashCode() => HashCode.Combine(Before, After, Strategy);

        public void Deconstruct(out int before, out int after, out string strategy)
        {
            before = Before;
            after = After;
            strategy = Strategy;
        }

        public static implicit operator (int before, int after, string strategy)(ConversionRatioProps value) => (value.Before, value.After, value.Strategy);

        public static implicit operator ConversionRatioProps((int before, int after, string strategy) value) => new ConversionRatioProps(value.before, value.after, value.strategy);

        public static bool operator ==(ConversionRatioProps left, ConversionRatioProps right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ConversionRatioProps left, ConversionRatioProps right)
        {
            return !(left == right);
        }

        private string GetDebuggerDisplay()
        {
            return $"{Before} -> {After} ({Strategy})";
        }

        public override string ToString() => GetDebuggerDisplay();
    }
}
