using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Synthesis;

namespace Shamisen.Tests.IO.OpenTK
{
    public sealed class WaveformViewModel
    {
        public WaveformViewModel(string name, Func<SampleFormat, IFrequencyGeneratorSource> generateFunc)
        {
            ArgumentNullException.ThrowIfNull(name);
            Name = name;
            ArgumentNullException.ThrowIfNull(generateFunc);
            GenerateFunc = generateFunc;
        }

        public string Name { get; }
        public Func<SampleFormat, IFrequencyGeneratorSource> GenerateFunc { get; }
    }
}
