using System;
using System.Linq;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Shamisen.Benchmarks
{
    public class PlaybackSpeedColumn : IColumn
    {
        #region Properties
        public PlaybackSpeedColumn(Func<BenchmarkCase, int> frameSelector, Func<BenchmarkCase, int> sampleRateSelector)
        {
            FrameSelector = frameSelector ?? throw new ArgumentNullException(nameof(frameSelector));
            SampleRateSelector = sampleRateSelector ?? throw new ArgumentNullException(nameof(sampleRateSelector));
        }

        public string Id => "PlaybackSpeed";
        public string ColumnName => "Speed vs Playback";
        public bool AlwaysShow => true;
        public ColumnCategory Category => ColumnCategory.Custom;
        public int PriorityInCategory => 0;
        public bool IsNumeric => true;
        public UnitType UnitType => UnitType.Dimensionless;
        public string Legend => $"x times faster than playback";

        public Func<BenchmarkCase, int> FrameSelector { get; }
        public Func<BenchmarkCase, int> SampleRateSelector { get; }
        public bool IsAvailable(Summary summary) => true;
        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
        public string GetValue(Summary summary, BenchmarkCase benchmarkCase) => GetValue(summary, benchmarkCase, summary.Style);
        #endregion
        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
        {
            try
            {
                int? nf = FrameSelector?.Invoke(benchmarkCase);
                int? ndst = SampleRateSelector?.Invoke(benchmarkCase);
                double? playbackTime = (double)nf / ndst * 1.0E9f;
                double mean = summary[benchmarkCase].ResultStatistics.Mean;
                double? speed = playbackTime / mean;
                return $"{speed}";
            }
            catch (Exception ex)
            {
                foreach (string item in benchmarkCase.Parameters.Items.Select(a => $"{a}").ToArray())
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine(ex.ToString());
                return "N/A";
            }
        }

    }
}
