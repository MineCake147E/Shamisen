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
            Name = name ?? throw new ArgumentNullException(nameof(name));
            GenerateFunc = generateFunc ?? throw new ArgumentNullException(nameof(generateFunc));
        }

        public string Name { get; }
        public Func<SampleFormat, IFrequencyGeneratorSource> GenerateFunc { get; }
    }
}
